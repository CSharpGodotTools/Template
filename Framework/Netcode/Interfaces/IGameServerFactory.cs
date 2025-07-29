using __TEMPLATE__.Netcode.Server;

namespace __TEMPLATE__.Netcode;

public interface IGameServerFactory
{
    ENetServer CreateServer();
}
