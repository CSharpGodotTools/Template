using Godot;

namespace GodotUtils;

/// <summary>
/// Defines creation callbacks for pooled nodes that use components.
/// </summary>
public interface IComponentPoolable<TNode> where TNode : CanvasItem, IComponentPoolable<TNode>
{
    /// <summary>
    /// Gets the component host for the node.
    /// </summary>
    IComponentHost Components { get; }

    /// <summary>
    /// Invoked when a new <typeparamref name="TNode"/> is created.
    /// </summary>
    void OnCreate(ComponentPool<TNode> pool);
}
