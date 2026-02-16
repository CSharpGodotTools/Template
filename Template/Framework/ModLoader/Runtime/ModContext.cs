using Godot;

namespace Framework.Mods;

internal sealed class ModContext(Node hostNode, ModMetadata metadata) : IModContext
{
    public ModMetadata Metadata { get; } = metadata;
    public Node HostNode { get; } = hostNode;

    public void Log(string message)
    {
        GameFramework.Logger.Log($"[Mod:{Metadata.Id}] {message}");
    }

    public T GetService<T>() where T : Node
    {
        return GameFramework.Services.Get<T>();
    }
}
