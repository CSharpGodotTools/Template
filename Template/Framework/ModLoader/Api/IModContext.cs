using Godot;

namespace Framework.Mods;

/// <summary>
/// Exposes a minimal host surface to managed C# mod entrypoints.
/// </summary>
public interface IModContext
{
    /// <summary>
    /// Metadata defined for the currently loaded mod.
    /// </summary>
    ModMetadata Metadata { get; }

    /// <summary>
    /// Host node that can be used for adding child nodes or scheduling deferred actions.
    /// </summary>
    Node HostNode { get; }

    /// <summary>
    /// Writes a message through the framework logger with a mod-specific prefix.
    /// </summary>
    /// <param name="message">Message content to write.</param>
    void Log(string message);

    /// <summary>
    /// Resolves a scene-lifetime registered service.
    /// </summary>
    /// <typeparam name="T">Service node type.</typeparam>
    /// <returns>The resolved service instance.</returns>
    T GetService<T>() where T : Node;
}
