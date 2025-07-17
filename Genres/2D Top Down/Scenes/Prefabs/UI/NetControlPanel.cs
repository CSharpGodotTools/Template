using __TEMPLATE__.Netcode;
using __TEMPLATE__.Netcode.Client;
using __TEMPLATE__.Netcode.Server;

namespace __TEMPLATE__.TopDown2D;

[Service]
public partial class NetControlPanel : NetControlPanelLow
{
    public override void StartClientButtonPressed(string username)
    {
        Services.Get<Level>().PlayerUsername = username;
    }

    public override IGameServerFactory GameServerFactory() => new GameServerFactory();
    public override IGameClientFactory GameClientFactory() => new GameClientFactory();
}

public class GameServerFactory : IGameServerFactory
{
    public ENetServer CreateServer() => new GameServer();
}

public class GameClientFactory : IGameClientFactory
{
    public ENetClient CreateClient() => new GameClient();
}
