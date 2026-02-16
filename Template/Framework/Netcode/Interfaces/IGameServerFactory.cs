using Framework.Netcode.Server;

namespace Framework.Netcode;

/// <summary>
/// Creates game-specific server wrappers for Net bootstrap orchestration.
/// </summary>
public interface IGameServerFactory
{
    GodotServer CreateServer();
}
