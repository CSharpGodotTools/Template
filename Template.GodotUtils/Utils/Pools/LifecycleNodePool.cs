using Godot;
using System;

namespace GodotUtils;

/// <summary>
/// Manages a pool of lifecycle-enabled nodes to eliminate expensive QueueFree calls.
/// Nodes must implement IPoolable to receive acquisition and release callbacks.
/// </summary>
/// <typeparam name="TNode">Node type implementing IPoolable.</typeparam>
public sealed class LifecycleNodePool<TNode> : BaseNodePool<TNode>, IPoolContext<TNode>
    where TNode : CanvasItem, IPoolable<TNode>
{
    private readonly Action<TNode> _onCreateAction;

    /// <summary>
    /// Creates a pool of lifecycle-managed nodes.
    /// </summary>
    public LifecycleNodePool(Node parent, Func<TNode> createNodeFunc)
        : base(parent, createNodeFunc)
    {
        _onCreateAction = node => node.OnCreate(this);
    }

    /// <summary>
    /// Returns an available node or creates a new one if all are in use.
    /// </summary>
    public override TNode Acquire() => _core.Acquire(_onCreateAction, OnAcquireCallback);

    /// <summary>
    /// Releases the node back to the pool.
    /// </summary>
    public override void Release(TNode node) => _core.Release(node, OnReleaseCallback);

    private protected override Action<TNode> OnAcquireCallback => static node => node.OnAcquire();
    private protected override Action<TNode> OnReleaseCallback => static node => node.OnRelease();
}
