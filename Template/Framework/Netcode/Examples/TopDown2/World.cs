using Godot;
using System;

namespace __TEMPLATE__.Netcode.Examples.TopDown2;

public partial class World : Node, ISceneDependencyReceiver
{
    private const int Port = 25565;
    private const string Ip = "127.0.0.1";

    private Net<GameClient, GameServer> _net = null!;
    private ILoggerService _loggerService = null!;
    private IApplicationLifetime _applicationLifetime = null!;
    private bool _isConfigured;

    public void Configure(GameServices services)
    {
        _loggerService = services.Logger;
        _applicationLifetime = services.ApplicationLifetime;
        _isConfigured = true;
    }

    public override void _Ready()
    {
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(World)} was not configured before _Ready.");

        _net = new Net<GameClient, GameServer>(_loggerService, _applicationLifetime);
        _net.StartServer(Port);
        _net.StartClient(Ip, Port);
    }
}
