using Template.Netcode.Client;

namespace Template.Netcode;

public interface IGameClientFactory
{
    ENetClient CreateClient();
}
