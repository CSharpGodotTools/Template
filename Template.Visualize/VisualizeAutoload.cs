#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

public sealed class VisualizeAutoload : IDisposable
{
    public static VisualizeAutoload? Instance { get; private set; }

    private readonly Dictionary<Node, VBoxContainer> _attributedLogContainers = [];
    private readonly Dictionary<Node, VBoxContainer> _nonAttributedLogContainers = [];

    public VisualizeAutoload()
    {
        if (Instance != null)
            throw new InvalidOperationException($"{nameof(VisualizeAutoload)} was initialized already");

        Instance = this;
    }

    public bool TryGetLogContainer(Node node, out VBoxContainer? vbox)
    {
        ArgumentNullException.ThrowIfNull(node);
        return _attributedLogContainers.TryGetValue(node, out vbox);
    }

    public void RegisterLogContainer(Node node, VBoxContainer vbox)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(vbox);
        _attributedLogContainers[node] = vbox;
    }

    public VBoxContainer GetOrCreateNonAttributeLogContainer(Node node, Func<VBoxContainer> factory)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(factory);

        if (!_nonAttributedLogContainers.TryGetValue(node, out VBoxContainer? vbox))
        {
            vbox = factory();
            _nonAttributedLogContainers[node] = vbox;
        }

        return vbox;
    }

    public void UnregisterNode(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _attributedLogContainers.Remove(node);
        _nonAttributedLogContainers.Remove(node);
    }

    public void Dispose()
    {
        _attributedLogContainers.Clear();
        _nonAttributedLogContainers.Clear();
        Instance = null;
    }
}
#endif
