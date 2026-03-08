using Framework.Netcode;
using Framework.Netcode.Server;

namespace Framework.Netcode.Examples.TopDown2;

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
