using Framework.Netcode.Client;

namespace Framework.Netcode;

/// <summary>
/// Creates game-specific client wrappers for Net bootstrap orchestration.
/// </summary>
public interface IGameClientFactory
{
    GodotClient CreateClient();
}
