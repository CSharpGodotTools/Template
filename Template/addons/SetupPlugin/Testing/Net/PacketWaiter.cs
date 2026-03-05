using Framework.Netcode;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

public static class PacketWaiter
{
    public static async Task<PacketWaitDiagnostics> WaitForPacketAsync<TPacket>(
        PacketCapture<TPacket> capture,
        ENetTestHarness<TPacket> harness,
        TimeSpan timeout)
        where TPacket : ClientPacket
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        PacketWaitDiagnostics diagnostics = new();

        while (stopwatch.Elapsed < timeout)
        {
            harness.Client.HandlePackets();

            if (capture.IsSet)
            {
                diagnostics.Received = true;
                return diagnostics;
            }

            diagnostics.ClientRunning = harness.Client.IsRunning;
            diagnostics.ClientConnected = harness.Client.IsConnected;
            diagnostics.ServerRunning = harness.Server.IsRunning;

            await Task.Delay(10);
        }

        return diagnostics;
    }
}
