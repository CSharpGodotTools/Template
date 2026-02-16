using Godot;

namespace GodotUtils;

/// <summary>
/// Defines lifecycle callbacks for pooled nodes.
/// </summary>
public interface IPoolable<TNode> where TNode : CanvasItem, IPoolable<TNode>
{
    /// <summary>
    /// Invoked when a new <typeparamref name="TNode"/> is created.
    /// </summary>
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
