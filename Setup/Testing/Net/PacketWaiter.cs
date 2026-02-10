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
        where TPacket : Framework.Netcode.ClientPacket
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
            diagnostics.LastOutgoingCount = harness.Client.OutgoingCount;
            diagnostics.LastGodotPacketCount = harness.Client.GodotPacketCount;
            diagnostics.LastCommandCount = harness.Client.CommandCount;

            if (diagnostics.LastOutgoingCount > 0)
            {
                diagnostics.SawOutgoingEnqueue = true;
            }
            else if (diagnostics.SawOutgoingEnqueue)
            {
                diagnostics.SawOutgoingDrain = true;
            }

            await Task.Delay(10);
        }

        return diagnostics;
    }
}
