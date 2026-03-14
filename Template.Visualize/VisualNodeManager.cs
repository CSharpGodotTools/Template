#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class VisualNodeManager
{
    private static readonly Type[] _positionNodeTypes = [typeof(Node2D), typeof(Control)];
    private static readonly Vector2 _defaultOffset = new(100, 100);
    private readonly Dictionary<ulong, VisualNodeInfo> _nodeTrackers = [];

    public void Register(Node node, object visualizedObject)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(visualizedObject);

        VisualData? visualData = VisualizeAttributeHandler.RetrieveData(visualizedObject, node);

        if (visualData != null)
        {
            (Control visualPanel, IReadOnlyList<Action> actions) = VisualUI.CreateVisualPanel(visualData);

            ulong instanceId = node.GetInstanceId();
            Node? positionalNode = GetClosestParentOfType(node, _positionNodeTypes);

            if (positionalNode == null)
            {
                PrintUtils.Warning($"[Visualize] No positional parent node could be found for {node.Name} so its visual panel will be created at position {_defaultOffset}");
            }

            if (!TryGetGlobalPosition(positionalNode, out Vector2 initialPosition))
            {
                initialPosition = _defaultOffset;
            }

            visualPanel.GlobalPosition = initialPosition;

            Vector2 offset = CalculateVerticalOffset(visualPanel);

            _nodeTrackers.Add(instanceId, new VisualNodeInfo(actions, visualPanel, positionalNode ?? node, offset));
        }

        node.TreeExited += () => RemoveVisualNode(node);
    }

    public void Update()
    {
        foreach (KeyValuePair<ulong, VisualNodeInfo> kvp in _nodeTrackers)
        {
            VisualNodeInfo info = kvp.Value;
            Node node = info.Node;
            Control visualControl = info.VisualControl;

            // Update position based on node type
            if (node != null && TryGetGlobalPosition(node, out Vector2 position))
            {
                visualControl.GlobalPosition = position + info.Offset;
            }

            foreach (Action action in info.Actions)
            {
                action();
            }
        }
    }

    private void RemoveVisualNode(Node node)
    {
        ulong instanceId = node.GetInstanceId();

        if (_nodeTrackers.TryGetValue(instanceId, out VisualNodeInfo? info))
        {
            // GetParent to queue free the CanvasLayer this VisualControl is a child of
            info.VisualControl.GetParent().QueueFree();
            _nodeTrackers.Remove(instanceId);
        }

        VisualizeAutoload.Instance?.UnregisterNode(node);
    }

    private static bool TryGetGlobalPosition(Node? node, out Vector2 position)
    {
        if (node is Node2D node2D)
        {
            position = node2D.GlobalPosition;
            return true;
        }

        if (node is Control control)
        {
            position = control.GlobalPosition;
            return true;
        }

        position = default;
        return false;
    }

    private static Node? GetClosestParentOfType(Node node, params Type[] typesToCheck)
    {
        if (IsNodeOfType(node, typesToCheck))
            return node;

        Node parent = node.GetParent();

        while (parent != null)
        {
            if (IsNodeOfType(parent, typesToCheck))
                return parent;

            parent = parent.GetParent();
        }

        return null;
    }

    private Vector2 CalculateVerticalOffset(Control visualPanel)
    {
        Vector2 offset = Vector2.Zero;

        foreach (VisualNodeInfo tracker in _nodeTrackers.Values)
        {
            Control existingControl = tracker.VisualControl;
            if (!ControlsOverlapping(visualPanel, existingControl))
                continue;

            offset += new Vector2(0, existingControl.GetRect().Size.Y);
        }

        return offset;
    }

    private static bool IsNodeOfType(Node node, Type[] typesToCheck)
    {
        foreach (Type type in typesToCheck)
        {
            if (type.IsInstanceOfType(node))
                return true;
        }

        return false;
    }

    private static bool ControlsOverlapping(Control control1, Control control2)
    {
        Rect2 rect1 = control1.GetRect();
        Rect2 rect2 = control2.GetRect();

        return rect1.Intersects(rect2);
    }
}
#endif
