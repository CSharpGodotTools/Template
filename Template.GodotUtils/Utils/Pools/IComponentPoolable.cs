using Godot;

namespace GodotUtils;

/// <summary>
/// Defines creation callbacks for pooled nodes that use components.
/// </summary>
/// <typeparam name="TNode">Node type participating in component-pool callbacks.</typeparam>
public interface IComponentPoolable<TNode> where TNode : CanvasItem, IComponentPoolable<TNode>
{
    /// <summary>
    /// Gets the component host for the node.
    /// </summary>
    /// <returns>Component list that should be toggled on acquire/release.</returns>
    ComponentList Components { get; }

    /// <summary>
    /// Invoked when a new <typeparamref name="TNode"/> is created.
    /// </summary>
    /// <param name="pool">Owning pool instance for this node.</param>
    void OnCreate(ComponentPool<TNode> pool);
}
