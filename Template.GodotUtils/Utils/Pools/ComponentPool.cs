#nullable enable
using Godot;
using System;

namespace GodotUtils;

/// <summary>
/// Manages a pool of component-based nodes to eliminate expensive QueueFree calls.
/// Nodes must implement IComponentPoolable to receive lifecycle callbacks.
/// </summary>
/// <typeparam name="TNode">Node type implementing IComponentPoolable.</typeparam>
public sealed class ComponentPool<TNode> : BaseNodePool<TNode>
    where TNode : CanvasItem, IComponentPoolable<TNode>
{
    private readonly Action<TNode> _onCreateAction;

    /// <summary>
    /// Creates a pool of component nodes.
    /// </summary>
    /// <param name="parent">Parent node that receives pooled node instances.</param>
    /// <param name="createNodeFunc">Factory that creates new pooled nodes.</param>
    public ComponentPool(Node parent, Func<TNode> createNodeFunc)
        : base(parent, createNodeFunc)
    {
        _onCreateAction = node => node.OnCreate(this);
    }

    /// <summary>
    /// Returns an available node or creates a new one if all are in use.
    /// </summary>
    /// <returns>Active node instance ready for use.</returns>
    public override TNode Acquire() => _core.Acquire(_onCreateAction, OnAcquireCallback);

    /// <summary>
    /// Releases the node back to the pool.
    /// </summary>
    /// <param name="node">Node to release.</param>
    public override void Release(TNode node) => _core.Release(node, OnReleaseCallback);

    private protected override Action<TNode> OnAcquireCallback => static node => node.Components.SetActive(true);
    private protected override Action<TNode> OnReleaseCallback => static node => node.Components.SetActive(false);
}
