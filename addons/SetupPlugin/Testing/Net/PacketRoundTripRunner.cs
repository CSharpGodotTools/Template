using GdUnit4;
using static GdUnit4.Assertions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Framework.Netcode;

namespace Template.Setup.Testing;

public static class PacketRoundTripRunner
{
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
            if (sendWatch.IsRunning)
            {
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
        if (!waitDiagnostics.Received)
        {
            string diagnostic = PacketTimeoutDiagnostics.Build(expected, harness, waitDiagnostics, packetTimeout);
            throw new TimeoutException(diagnostic);
        }
        AssertBool(waitDiagnostics.Received).IsTrue();

        if (receiveMs >= 0)
        {
            TestOutput.Timing("Packet received", receiveMs);
        }

        AssertBool(expected.Equals(capture.Packet)).IsTrue();
    }
}
