using Godot;

namespace __TEMPLATE__.Netcode.Examples.TopDown2;

/// <summary>
/// Minimal top-down netcode scene used to spin up local server/client pairs.
/// </summary>
public partial class World : Node
{
    /// <summary>
    /// Default test server port.
    /// </summary>
    private const int Port = 25565;

    /// <summary>
    /// Default local host address.
    /// </summary>
    private const string Ip = "127.0.0.1";

    /// <summary>
    /// Net coordinator for example scene.
    /// </summary>
    private Net<GameClient, GameServer> _net = null!;

    public override void _Ready()
    {
        _net = new Net<GameClient, GameServer>(Game.Logger, Game.Application, Game.BackgroundTasks);
        _net.StartServer(Port);
        _net.StartClient(Ip, Port);
    }
}
