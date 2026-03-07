using Godot;
using System;

namespace GodotUtils;

/// <summary>
/// Manages a pool of basic nodes to eliminate expensive QueueFree calls.
/// For nodes without special lifecycle requirements.
/// </summary>
/// <typeparam name="TNode">Node type to pool.</typeparam>
public sealed class NodePool<TNode> : BaseNodePool<TNode> where TNode : CanvasItem
{
    /// <summary>
    /// Creates a pool of basic nodes.
    /// </summary>
    public NodePool(Node parent, Func<TNode> createNodeFunc)
        : base(parent, createNodeFunc)
    {
    }

    /// <summary>
    /// Returns an available node or creates a new one if all are in use.
    /// </summary>
    public override TNode Acquire() => _core.Acquire(null, null);

    /// <summary>
    /// Releases the node back to the pool.
    /// </summary>
    public override void Release(TNode node) => _core.Release(node, null);
}
