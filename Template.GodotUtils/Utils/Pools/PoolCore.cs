#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Internal pooling engine shared by node-pool wrappers.
/// </summary>
/// <typeparam name="TNode">Canvas-item node type managed by the pool.</typeparam>
internal sealed class PoolCore<TNode> where TNode : CanvasItem
{
    /// <summary>
    /// Nodes currently in use by the pool.
    /// </summary>
    public IEnumerable<TNode> ActiveNodes => _activeNodes;

    private readonly Func<TNode> _createNodeFunc;
    private readonly Node _parent;
    private readonly Stack<TNode> _inactiveNodes = []; // The nodes NOT in use
    private readonly HashSet<TNode> _activeNodes = []; // The nodes in use

    /// <summary>
    /// Creates a pool of nodes using <paramref name="createNodeFunc"/> and attaches them as children of <paramref name="parent"/> to avoid expensive <c>QueueFree()</c> calls.
    /// </summary>
    /// <param name="parent">Parent node that owns pooled instances.</param>
    /// <param name="createNodeFunc">Factory used to create new pooled nodes.</param>
    public PoolCore(Node parent, Func<TNode> createNodeFunc)
    {
        _createNodeFunc = createNodeFunc ?? throw new ArgumentNullException(nameof(createNodeFunc));
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    /// <summary>
    /// Returns an available <typeparamref name="TNode"/> or creates a new one if all are in use.
    /// </summary>
    /// <param name="onCreate">Callback invoked when a new node instance is created.</param>
    /// <param name="onAcquire">Callback invoked whenever a node is acquired.</param>
    /// <returns>Active pooled node ready for use.</returns>
    public TNode Acquire(Action<TNode>? onCreate, Action<TNode>? onAcquire)
    {
        TNode node;

        // Is there an inactive node that can be activated?
        // Reuse inactive node when available.
        if (_inactiveNodes.Count > 0)
        {
            // O(1) lookup time
            node = _inactiveNodes.Pop();
        }
        // Otherwise create and initialize a new node.
        else
        {
            // No inactive nodes found, need to create a new node
            node = _createNodeFunc();
            onCreate?.Invoke(node);
            _parent.AddChild(node);

#if DEBUG
            // For performance, this is only done in debug mode
            node.Name = $"{typeof(TNode).Name}_{node.GetInstanceId()}";
#endif
        }

        // Keep track of this active node
        _activeNodes.Add(node);

        // Activate the node
        node.Show();
        onAcquire?.Invoke(node);

        return node;
    }

    /// <summary>
    /// Releases the <paramref name="node"/> from the pool.
    /// </summary>
    /// <param name="node">Node to release back to the inactive pool.</param>
    /// <param name="onRelease">Callback invoked after the node is released.</param>
    public void Release(TNode node, Action<TNode>? onRelease)
    {
        // Only release nodes that are currently active.
        // Ignore duplicate releases or foreign nodes.
        if (!_activeNodes.Remove(node))
            return;

        // Mark the active node as inactive
        _inactiveNodes.Push(node);

        // Deactivate the node
        node.Hide();
        onRelease?.Invoke(node);
    }

    /// <summary>
    /// Queue frees all inactive and active nodes in the pool.
    /// </summary>
    public void QueueFreeAll()
    {
        // Queue free inactive nodes
        while (_inactiveNodes.Count > 0)
            _inactiveNodes.Pop().QueueFree();

        // Queue free active nodes
        while (_activeNodes.Count > 0)
        {
            TNode node = GetAnyActiveNode(_activeNodes);
            _activeNodes.Remove(node);
            node.QueueFree();
        }
    }

    /// <summary>
    /// Returns an arbitrary active node from the set.
    /// </summary>
    /// <param name="nodes">Active node set.</param>
    /// <returns>One active node instance.</returns>
    private static TNode GetAnyActiveNode(HashSet<TNode> nodes)
    {
        foreach (TNode node in nodes)
            return node;

        throw new InvalidOperationException("Active node set is empty.");
    }
}
#nullable disable
