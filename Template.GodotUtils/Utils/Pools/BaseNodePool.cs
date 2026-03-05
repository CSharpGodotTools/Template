#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Base class for node pooling to eliminate expensive QueueFree calls.
/// Provides common infrastructure for different node lifecycle patterns.
/// </summary>
/// <typeparam name="TNode">The node type managed by this pool.</typeparam>
public abstract class BaseNodePool<TNode> : INodePool<TNode> where TNode : CanvasItem
{
    /// <summary>
    /// Nodes currently in use by the pool.
    /// </summary>
    public IEnumerable<TNode> ActiveNodes => _core.ActiveNodes;

    private protected readonly PoolCore<TNode> _core;

    /// <summary>
    /// Creates a pool of nodes using <paramref name="createNodeFunc"/> and attaches them as children
    /// of <paramref name="parent"/> to avoid expensive QueueFree calls.
    /// </summary>
    protected BaseNodePool(Node parent, Func<TNode> createNodeFunc)
    {
        _core = new PoolCore<TNode>(parent ?? throw new ArgumentNullException(nameof(parent)), createNodeFunc);
    }

    /// <summary>
    /// Returns an available node or creates a new one if all are in use.
    /// </summary>
    public abstract TNode Acquire();

    /// <summary>
    /// Releases the node back to the pool.
    /// </summary>
    public abstract void Release(TNode node);

    /// <summary>
    /// Queue frees all inactive and active nodes in the pool.
    /// </summary>
    public void QueueFreeAll() => _core.QueueFreeAll();

    /// <summary>
    /// Gets the onCreate callback. Can be null if no special creation handling needed.
    /// </summary>
    private protected virtual Action<TNode>? OnCreateCallback => null;

    /// <summary>
    /// Gets the onAcquire callback. Can be null if no special acquire handling needed.
    /// </summary>
    private protected virtual Action<TNode>? OnAcquireCallback => null;

    /// <summary>
    /// Gets the onRelease callback. Can be null if no special release handling needed.
    /// </summary>
    private protected virtual Action<TNode>? OnReleaseCallback => null;

    /// <summary>
    /// Performs the actual acquire with registered callbacks.
    /// </summary>
    private protected TNode PerformAcquire() => _core.Acquire(OnCreateCallback, OnAcquireCallback);

    /// <summary>
    /// Performs the actual release with registered callbacks.
    /// </summary>
    private protected void PerformRelease(TNode node) => _core.Release(node, OnReleaseCallback);
}
