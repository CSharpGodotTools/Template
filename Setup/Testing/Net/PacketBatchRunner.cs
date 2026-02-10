using GdUnit4;
using static GdUnit4.Assertions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Framework.Netcode;

namespace Template.Setup.Testing;

public static class PacketBatchRunner
{
    public static async Task RunAsync<TPacket>(
        Func<TPacket> createPacket,
        int count,
        TimeSpan connectTimeout,
        TimeSpan batchTimeout,
        bool suppressLogs)
        where TPacket : ClientPacket
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Batch count must be positive.");
        }

        int received = 0;
        Exception mismatch = null;
        TPacket expected = createPacket();

        await using ENetTestHarness<TPacket> harness = new((packet, _) =>
        {
            if (mismatch != null)
            {
                return;
            }

            if (!expected.Equals(packet))
            {
                mismatch = new Exception($"Packet mismatch at index {received} for {typeof(TPacket).Name}.");
                return;
            }

            Interlocked.Increment(ref received);
        });

        TestOutput.Step("Connecting client/server");
        ENetOptions options = suppressLogs ? CreateQuietOptions() : new ENetOptions();
        bool connected = await harness.ConnectAsync(connectTimeout, options);
        AssertBool(connected).IsTrue();

        TestOutput.Step($"Sending {count}x {typeof(TPacket).Name}");
        for (int i = 0; i < count; i++)
        {
            harness.Send(createPacket(), log: !suppressLogs);
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < batchTimeout)
        {
            harness.Client.HandlePackets();

            if (mismatch != null)
            {
                throw mismatch;
            }

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
