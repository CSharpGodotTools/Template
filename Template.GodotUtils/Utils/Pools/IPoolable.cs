using Godot;

namespace GodotUtils;

/// <summary>
/// Defines lifecycle callbacks for pooled nodes.
/// </summary>
/// <typeparam name="TNode">Node type participating in pooling lifecycle callbacks.</typeparam>
public interface IPoolable<TNode> where TNode : CanvasItem, IPoolable<TNode>
{
    /// <summary>
    /// Invoked when a new <typeparamref name="TNode"/> is created.
    /// </summary>
    /// <param name="pool">Pool context used to release this node later.</param>
    void OnCreate(IPoolContext<TNode> pool);

    /// <summary>
    /// Invoked when a <typeparamref name="TNode"/> is acquired from the pool.
    /// </summary>
    void OnAcquire();

    /// <summary>
    /// Invoked when a <typeparamref name="TNode"/> is released from the pool.
    /// </summary>
    void OnRelease();
}
