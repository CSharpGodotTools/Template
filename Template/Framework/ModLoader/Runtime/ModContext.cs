using Godot;

namespace __TEMPLATE__.Mods;

/// <summary>
/// Default runtime implementation of <see cref="IModContext"/> for managed mod entrypoints.
/// </summary>
/// <param name="hostNode">Host scene node that loaded the mod.</param>
/// <param name="metadata">Immutable mod metadata snapshot.</param>
/// <param name="logger">Logger service used for mod-prefixed log output.</param>
/// <param name="services">Service locator exposed to mod entrypoints.</param>
internal sealed class ModContext(Node hostNode, ModMetadata metadata, ILoggerService logger, Services services) : IModContext
{
    private readonly ILoggerService _logger = logger;
    private readonly Services _services = services;

    /// <inheritdoc />
    public ModMetadata Metadata { get; } = metadata;

    /// <inheritdoc />
    public Node HostNode { get; } = hostNode;

    /// <inheritdoc />
    public void Log(string message)
    {
        _logger.Log($"[Mod:{Metadata.Id}] {message}");
    }

    /// <inheritdoc />
    public T GetService<T>() where T : Node
    {
        return _services.Get<T>();
    }
}
