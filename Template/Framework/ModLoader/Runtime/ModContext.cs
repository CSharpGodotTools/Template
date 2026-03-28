using Godot;

namespace __TEMPLATE__.Mods;

internal sealed class ModContext(Node hostNode, ModMetadata metadata, ILoggerService logger, Services services) : IModContext
{
    private readonly ILoggerService _logger = logger;
    private readonly Services _services = services;

    public ModMetadata Metadata { get; } = metadata;
    public Node HostNode { get; } = hostNode;

    public void Log(string message)
    {
        _logger.Log($"[Mod:{Metadata.Id}] {message}");
    }

    public T GetService<T>() where T : Node
    {
        return _services.Get<T>();
    }
}
