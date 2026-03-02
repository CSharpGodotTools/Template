using Godot;

namespace Framework.Netcode.Examples.TopDown2;

public partial class World : Node
{
    private const int Port = 25565;
    private const string Ip = "127.0.0.1";

    private Net<GameClient, GameServer> _net;

    public override void _Ready()
    {
        _net = new Net<GameClient, GameServer>();
        _net.StartServer(Port);
        _net.StartClient(Ip, Port);
    }
}
