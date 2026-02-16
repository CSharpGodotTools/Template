#if DEBUG
using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class VisualNodeManager
{
    private static readonly Vector2 DefaultOffset = new(100, 100);
    private readonly Dictionary<ulong, VisualNodeInfo> _nodeTrackers = [];

    public void Register(Node node, params string[] readonlyMembers)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisualData visualData = VisualizeAttributeHandler.RetrieveData(node);

        if (visualData != null)
        {
            (Control visualPanel, IReadOnlyList<Action> actions) = VisualUI.CreateVisualPanel(visualData, readonlyMembers);

            ulong instanceId = node.GetInstanceId();
            Node positionalNode = GetClosestParentOfType(node, typeof(Node2D), typeof(Control));

            if (positionalNode == null)
            {
                PrintUtils.Warning($"[Visualize] No positional parent node could be found for {node.Name} so its visual panel will be created at position {DefaultOffset}");
            }

            if (!TryGetGlobalPosition(positionalNode, out Vector2 initialPosition))
            {
                initialPosition = DefaultOffset;
            }

            visualPanel.GlobalPosition = initialPosition;

            // Ensure the added visual panel is not overlapping with any other visual panels
            Vector2 offset = Vector2.Zero;

            foreach (VisualNodeInfo tracker in _nodeTrackers.Values)
            {
                Control existingControl = tracker.VisualControl;
                if (existingControl == visualPanel)
                    continue; // Skip checking against itself

                if (ControlsOverlapping(visualPanel, existingControl))
                {
                    // Move vbox down by the existing controls height
                    offset += new Vector2(0, existingControl.GetRect().Size.Y);
                }
            }

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

        if (_nodeTrackers.TryGetValue(instanceId, out VisualNodeInfo info))
        {
            // GetParent to queue free the CanvasLayer this VisualControl is a child of
            info.VisualControl.GetParent().QueueFree();
            _nodeTrackers.Remove(instanceId);
        }

        VisualizeAutoload.Instance?.UnregisterNode(node);
    }

    private static bool TryGetGlobalPosition(Node node, out Vector2 position)
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

    private static Node GetClosestParentOfType(Node node, params Type[] typesToCheck)
    {
        // Check if the current node is of one of the specified types
        if (IsNodeOfType(node, typesToCheck))
            return node;

        // Recursively get the parent and check its type
        Node parent = node.GetParent();

        while (parent != null)
        {
            if (IsNodeOfType(parent, typesToCheck))
                return parent;

            parent = parent.GetParent();
        }

        // If no suitable parent is found, return null
        return null;
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
        // Get the bounding rectangles of the control nodes
        Rect2 rect1 = control1.GetRect();
        Rect2 rect2 = control2.GetRect();

        // Check if the rectangles intersect
        return rect1.Intersects(rect2);
    }
}
#endif
