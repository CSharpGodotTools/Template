using Framework.Netcode;
using Framework.Netcode.Server;

namespace Framework.Netcode.Examples.TopDown2;

public class GameServer : GodotServer
{
    protected override void RegisterPackets()
    {
        OnPacket<CPacketPlayerPosition>(OnReceivePlayerPosition);
    }

    private void OnReceivePlayerPosition(PacketFromPeer<CPacketPlayerPosition> peer)
    {
        Log(peer.Packet);
    }
}
