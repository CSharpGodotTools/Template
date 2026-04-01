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
    /// <param name="parent">Parent node that receives pooled node instances.</param>
    /// <param name="createNodeFunc">Factory that creates new pooled nodes when needed.</param>
    protected BaseNodePool(Node parent, Func<TNode> createNodeFunc)
    {
        _core = new PoolCore<TNode>(parent ?? throw new ArgumentNullException(nameof(parent)), createNodeFunc);
    }

    /// <summary>
    /// Returns an available node or creates a new one if all are in use.
    /// </summary>
    /// <returns>Active node instance ready for use.</returns>
    public abstract TNode Acquire();

    /// <summary>
    /// Releases the node back to the pool.
    /// </summary>
    /// <param name="node">Node to release.</param>
    public abstract void Release(TNode node);

    /// <summary>
    /// Queue frees all inactive and active nodes in the pool.
    /// </summary>
    public void QueueFreeAll() => _core.QueueFreeAll();

    /// <summary>
    /// Gets the onCreate callback. Can be null if no special creation handling needed.
    /// </summary>
    /// <returns>Creation callback invoked for newly created nodes.</returns>
    private protected virtual Action<TNode>? OnCreateCallback => null;

    /// <summary>
    /// Gets the onAcquire callback. Can be null if no special acquire handling needed.
    /// </summary>
    /// <returns>Acquire callback invoked for each acquired node.</returns>
    private protected virtual Action<TNode>? OnAcquireCallback => null;

    /// <summary>
    /// Gets the onRelease callback. Can be null if no special release handling needed.
    /// </summary>
    /// <returns>Release callback invoked for each released node.</returns>
    private protected virtual Action<TNode>? OnReleaseCallback => null;

    /// <summary>
    /// Performs the actual acquire with registered callbacks.
    /// </summary>
    /// <returns>Active node instance.</returns>
    private protected TNode PerformAcquire() => _core.Acquire(OnCreateCallback, OnAcquireCallback);

    /// <summary>
    /// Performs the actual release with registered callbacks.
    /// </summary>
    /// <param name="node">Node to release.</param>
    private protected void PerformRelease(TNode node) => _core.Release(node, OnReleaseCallback);
}
