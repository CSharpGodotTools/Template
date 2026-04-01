using __TEMPLATE__.Netcode;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

namespace Template.Setup.Testing;

/// <summary>
/// Executes high-volume packet round-trip tests against the ENet harness.
/// </summary>
public static class PacketBatchRunner
{
    /// <summary>
    /// Sends a packet factory output in batches and verifies all packets are received.
    /// </summary>
    /// <typeparam name="TPacket">Packet type used for the batch run.</typeparam>
    /// <param name="createPacket">Factory that creates each packet instance.</param>
    /// <param name="count">Number of packets to send in the batch.</param>
    /// <param name="connectTimeout">Timeout for initial client/server connection.</param>
    /// <param name="batchTimeout">Timeout for receiving the full packet batch.</param>
    /// <param name="suppressLogs">Whether to suppress packet-level ENet logging.</param>
    /// <returns>A task that completes when the batch verification finishes.</returns>
    public static async Task RunAsync<TPacket>(
        Func<TPacket> createPacket,
        int count,
        TimeSpan connectTimeout,
        TimeSpan batchTimeout,
        bool suppressLogs)
        where TPacket : ClientPacket
    {
        // Batch runs require at least one packet to validate round-trip behavior.
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Batch count must be positive.");
        }

        int received = 0;
        Exception? mismatch = null;
        TPacket expected = createPacket();

        await using ENetTestHarness<TPacket> harness = new((packet, _) =>
        {
            // Ignore subsequent packets after the first mismatch is recorded.
            if (mismatch != null)
            {
                return;
            }

            // Capture the first payload mismatch and stop further comparisons.
            if (!expected.Equals(packet))
            {
                mismatch = new Exception($"Packet mismatch at index {received} for {typeof(TPacket).Name}.");
                return;
            }

            Interlocked.Increment(ref received);
        });

        TestOutput.Step("Connecting client/server");
        // Quiet options keep stress runs readable while preserving behavior.
        ENetOptions options = suppressLogs ? CreateQuietOptions() : new ENetOptions();
        bool connected = await harness.ConnectAsync(connectTimeout, options);
        AssertBool(connected).IsTrue();

        TestOutput.Step($"Sending {count}x {typeof(TPacket).Name}");
        for (int i = 0; i < count; i++)
        {
            harness.Send(createPacket(), log: !suppressLogs);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        // Poll until all packets arrive or timeout is exceeded.
        while (stopwatch.Elapsed < batchTimeout)
        {
            harness.Client.HandlePackets();

            // Fail fast as soon as the receive callback reports a mismatch.
            if (mismatch != null)
            {
                throw mismatch;
            }

            // Exit early once the expected packet count has been reached.
            if (Volatile.Read(ref received) >= count)
            {
                TestOutput.Timing("Batch received", stopwatch.ElapsedMilliseconds);
                return;
            }

            await Task.Delay(10);
        }

        throw new TimeoutException(
            $"Timed out after {batchTimeout.TotalSeconds:0.##}s waiting for {count} " +
            $"{typeof(TPacket).Name} packets; received {received}.");
    }

    /// <summary>
    /// Creates ENet options with packet print output disabled.
    /// </summary>
    /// <returns>ENet options tuned for low-noise batch execution.</returns>
    private static ENetOptions CreateQuietOptions()
    {
        return new ENetOptions
        {
            PrintPacketData = false,
            PrintPacketByteSize = false,
            PrintPacketReceived = false,
            PrintPacketSent = false
        };
    }
}
