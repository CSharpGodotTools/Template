using __TEMPLATE__.Netcode;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

namespace Template.Setup.Testing;

/// <summary>
/// Executes single-packet round-trip verification using the ENet test harness.
/// </summary>
public static class PacketRoundTripRunner
{
    /// <summary>
    /// Sends one packet and verifies that the captured packet matches expectations.
    /// </summary>
    /// <typeparam name="TPacket">Packet type being validated.</typeparam>
    /// <param name="expected">Expected packet payload.</param>
    /// <param name="connectTimeout">Timeout for harness connection setup.</param>
    /// <param name="packetTimeout">Timeout for packet capture completion.</param>
    /// <returns>A task that completes after validation succeeds or throws.</returns>
    public static async Task RunAsync<TPacket>(
        TPacket expected,
        TimeSpan connectTimeout,
        TimeSpan packetTimeout)
        where TPacket : ClientPacket
    {
        PacketCapture<TPacket> capture = new();
        Stopwatch sendWatch = new();
        long receiveMs = -1;

        await using ENetTestHarness<TPacket> harness = new((packet, _) =>
        {
            // Record receive latency only while the send timer is active.
            if (sendWatch.IsRunning)
            {
                // Record first observed receive time only.
                long elapsed = sendWatch.ElapsedMilliseconds;
                Interlocked.CompareExchange(ref receiveMs, elapsed, -1);
            }

            capture.Set(packet);
        });

        TestOutput.Step("Connecting client/server");
        bool connected = await harness.ConnectAsync(connectTimeout);
        AssertBool(connected).IsTrue();

        TestOutput.Step($"Sending packet {expected.GetType().Name}");
        sendWatch.Start();
        harness.Send(expected);

        Console.WriteLine("[Test] Waiting for packet capture...");
        PacketWaitDiagnostics waitDiagnostics = await PacketWaiter.WaitForPacketAsync(capture, harness, packetTimeout);
        // Raise timeout diagnostics when capture did not receive a packet.
        if (!waitDiagnostics.Received)
        {
            string diagnostic = PacketTimeoutDiagnostics.Build(expected, harness, waitDiagnostics, packetTimeout);
            throw new TimeoutException(diagnostic);
        }
        AssertBool(waitDiagnostics.Received).IsTrue();

        // Emit timing output only when receive latency was successfully captured.
        if (receiveMs >= 0)
        {
            TestOutput.Timing("Packet received", receiveMs);
        }

        AssertBool(expected.Equals(capture.Packet)).IsTrue();
    }
}
