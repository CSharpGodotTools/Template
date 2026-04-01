#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Holds runtime visualization state shared across debug UI components.
/// </summary>
public sealed class VisualizeAutoload : IDisposable
{
    /// <summary>
    /// Active autoload instance used by static visualization helpers.
    /// </summary>
    public static VisualizeAutoload? Instance { get; private set; }

    private readonly Dictionary<Node, VBoxContainer> _attributedLogContainers = [];
    private readonly Dictionary<Node, VBoxContainer> _nonAttributedLogContainers = [];

    /// <summary>
    /// Creates the debug visualization autoload singleton.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when another instance is already active.</exception>
    public VisualizeAutoload()
    {
        // Prevent duplicate autoload registration because static helpers rely on a single instance.
        if (Instance != null)
            throw new InvalidOperationException($"{nameof(VisualizeAutoload)} was initialized already");

        Instance = this;
    }

    /// <summary>
    /// Attempts to resolve the attributed log container for a node.
    /// </summary>
    /// <param name="node">Node used as the lookup key.</param>
    /// <param name="vbox">Resolved log container when found.</param>
    /// <returns><see langword="true"/> when a container exists for the node.</returns>
    public bool TryGetLogContainer(Node node, out VBoxContainer? vbox)
    {
        ArgumentNullException.ThrowIfNull(node);
        return _attributedLogContainers.TryGetValue(node, out vbox);
    }

    /// <summary>
    /// Registers or replaces the attributed log container associated with a node.
    /// </summary>
    /// <param name="node">Node used as the lookup key.</param>
    /// <param name="vbox">Container that receives attributed log labels.</param>
    public void RegisterLogContainer(Node node, VBoxContainer vbox)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(vbox);
        _attributedLogContainers[node] = vbox;
    }

    /// <summary>
    /// Returns an existing non-attributed log container or creates one using the provided factory.
    /// </summary>
    /// <param name="node">Node used as the lookup key.</param>
    /// <param name="factory">Factory invoked when the node has no existing container.</param>
    /// <returns>Existing or newly created non-attributed log container.</returns>
    public VBoxContainer GetOrCreateNonAttributeLogContainer(Node node, Func<VBoxContainer> factory)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(factory);

        // Lazily create non-attributed containers so nodes that never log do not allocate UI.
        if (!_nonAttributedLogContainers.TryGetValue(node, out VBoxContainer? vbox))
        {
            vbox = factory();
            _nonAttributedLogContainers[node] = vbox;
        }

        return vbox;
    }

    /// <summary>
    /// Removes all tracked log containers associated with a node.
    /// </summary>
    /// <param name="node">Node to unregister.</param>
    public void UnregisterNode(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _attributedLogContainers.Remove(node);
        _nonAttributedLogContainers.Remove(node);
    }

    /// <summary>
    /// Clears tracked state and resets the singleton instance.
    /// </summary>
    public void Dispose()
    {
        _attributedLogContainers.Clear();
        _nonAttributedLogContainers.Clear();
        Instance = null;
    }
}
#endif
