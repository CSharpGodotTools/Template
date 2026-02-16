using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Creates a pool of nodes to eliminate expensive QueueFree calls.
/// <code>
/// // Create the pool
/// NodePool&lt;Projectile&gt; pool = new(parentNode, () => projectilePackedScene.Instantiate());
/// 
/// // Get a projectile from the pool
/// Projectile projectile = pool.Acquire();
/// 
/// // Projectile goes off screen or dies
/// projectile.Release(); // Never use QueueFree()
/// </code>
/// </summary>
/// <typeparam name="TNode">The nodes managed in the pool.</typeparam>
public class NodePool<TNode> where TNode : CanvasItem
{
    /// <summary>
    /// Nodes currently in use by the pool.
    /// </summary>
    public IEnumerable<TNode> ActiveNodes => _core.ActiveNodes;

    private readonly PoolCore<TNode> _core;

    /// <summary>
    /// Creates a pool of nodes using <paramref name="createNodeFunc"/> and attaches them as children of <paramref name="parent"/> to avoid expensive <c>QueueFree()</c> calls.
    /// </summary>
    public NodePool(Node parent, Func<TNode> createNodeFunc)
    {
        _core = new PoolCore<TNode>(parent, createNodeFunc);
    }

    /// <summary>
    /// Returns an available <typeparamref name="TNode"/> or creates a new one if all are in use.
    /// </summary>
    public TNode Acquire() => _core.Acquire(null, null);

    /// <summary>
    /// Releases the <paramref name="node"/> from the pool.
    /// </summary>
    public void Release(TNode node) => _core.Release(node, null);

    /// <summary>
    /// Queue frees all inactive and active nodes in the pool.
    /// </summary>
    public void QueueFreeAll() => _core.QueueFreeAll();
}
