using __TEMPLATE__.Netcode.Server;

namespace __TEMPLATE__.Netcode.Examples.TopDown2;

/// <summary>
/// Minimal sample server that logs incoming TopDown2 player-position packets.
/// </summary>
public class GameServer : GodotServer
{
    /// <summary>
    /// Creates the sample server and registers packet handlers.
    /// </summary>
    public GameServer()
    {
        OnPacket<CPacketPlayerPosition>(OnReceivePlayerPosition);
    }

    /// <summary>
    /// Handles incoming position packets from clients.
    /// </summary>
    /// <param name="peer">Packet and peer metadata.</param>
    private void OnReceivePlayerPosition(PacketFromPeer<CPacketPlayerPosition> peer)
    {
        Log(peer.Packet);
    }
}
