using Template.Netcode.Server;

namespace Template.Netcode;

public interface IGameServerFactory
{
    ENetServer CreateServer();
}
