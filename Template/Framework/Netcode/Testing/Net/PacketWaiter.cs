using __TEMPLATE__.Netcode;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

/// <summary>
/// Polls packet capture state while collecting endpoint diagnostics.
/// </summary>
public static class PacketWaiter
{
    /// <summary>
    /// Waits for packet capture until timeout and returns sampled endpoint status.
    /// </summary>
    /// <typeparam name="TPacket">Packet type expected by the capture.</typeparam>
    /// <param name="capture">Capture container populated by packet handler.</param>
    /// <param name="harness">Active ENet test harness.</param>
    /// <param name="timeout">Maximum wait duration.</param>
    /// <returns>Diagnostics describing receive and endpoint states.</returns>
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

            // Exit immediately once capture receives the expected packet.
            if (capture.IsSet)
            {

                // Capture succeeded, so caller can skip timeout-specific diagnostics.
                diagnostics.Received = true;
                return diagnostics;
            }


            // Snapshot endpoint state on each poll to aid timeout investigations.
            diagnostics.ClientRunning = harness.Client.IsRunning;
            diagnostics.ClientConnected = harness.Client.IsConnected;
            diagnostics.ServerRunning = harness.Server.IsRunning;

            await Task.Delay(10);
        }

        return diagnostics;
    }
}
