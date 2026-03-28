using Godot;

namespace __TEMPLATE__.Mods;

internal sealed class ModContext(Node hostNode, ModMetadata metadata) : IModContext
{
    public ModMetadata Metadata { get; } = metadata;
    public Node HostNode { get; } = hostNode;

    public void Log(string message)
    {
        Game.Logger.Log($"[Mod:{Metadata.Id}] {message}");
    }

    public T GetService<T>() where T : Node
    {
        return Game.ScopedServices.Get<T>();
    }
}
