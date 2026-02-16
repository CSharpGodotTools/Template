#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

public sealed class VisualizeAutoload : IDisposable
{
    public static VisualizeAutoload Instance { get; private set; }

    private readonly Dictionary<Node, VBoxContainer> _visualNodes = [];
    private readonly Dictionary<Node, VBoxContainer> _visualNodesWithoutAttribute = [];

    public VisualizeAutoload()
    {
        if (Instance != null)
            throw new InvalidOperationException($"{nameof(VisualizeAutoload)} was initialized already");

        Instance = this;
    }

    public bool TryGetLogContainer(Node node, out VBoxContainer vbox)
    {
        ArgumentNullException.ThrowIfNull(node);
        return _visualNodes.TryGetValue(node, out vbox);
    }

    public void RegisterLogContainer(Node node, VBoxContainer vbox)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(vbox);
        _visualNodes[node] = vbox;
    }

    public VBoxContainer GetOrCreateNonAttributeLogContainer(Node node, Func<VBoxContainer> factory)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(factory);

        if (!_visualNodesWithoutAttribute.TryGetValue(node, out VBoxContainer vbox))
        {
            vbox = factory();
            _visualNodesWithoutAttribute[node] = vbox;
        }

        return vbox;
    }

    public void UnregisterNode(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _visualNodes.Remove(node);
        _visualNodesWithoutAttribute.Remove(node);
    }

    public void Update()
    {
        Visualize.Update();
    }

    public void Dispose()
    {
        _visualNodes.Clear();
        _visualNodesWithoutAttribute.Clear();
        Instance = null;
    }
}

internal sealed class VisualNodeInfo(IReadOnlyList<Action> actions, Control visualControl, Node node, Vector2 offset)
{
    public IReadOnlyList<Action> Actions { get; } = actions ?? throw new ArgumentNullException(nameof(actions));
    public Control VisualControl { get; } = visualControl ?? throw new ArgumentNullException(nameof(visualControl));
    public Vector2 Offset { get; } = offset;
    public Node Node { get; } = node ?? throw new ArgumentNullException(nameof(node));
}
#endif
