using Godot;
using ENet;
using __TEMPLATE__.Netcode;
using __TEMPLATE__.Netcode.Server;

namespace __TEMPLATE__.TopDown2D;

public class CPacketPosition : ClientPacket
{
    [NetSend(1)]
    public Vector2 Position { get; set; }

    public override void Handle(ENetServer s, Peer client)
    {
        GameServer server = (GameServer)s;
        server.Players[client.ID].Position = Position;
    }
}

