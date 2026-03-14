using __TEMPLATE__.Netcode.Server;

namespace __TEMPLATE__.Netcode.Examples.TopDown2;

public class GameServer : GodotServer
{
    public GameServer()
    {
        OnPacket<CPacketPlayerPosition>(OnReceivePlayerPosition);
    }

    private void OnReceivePlayerPosition(PacketFromPeer<CPacketPlayerPosition> peer)
    {
        Log(peer.Packet);
    }
}
