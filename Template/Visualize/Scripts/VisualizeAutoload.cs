using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Template;

public partial class VisualizeAutoload : Node
{
#if DEBUG
    private readonly Dictionary<ulong, VisualNodeInfo> _nodeTrackers = [];

    public override void _Ready()
    {
        foreach (Node node in GetTree().Root.GetChildren<Node>())
        {
            AddVisualNode(node);
        }

        GetTree().NodeAdded += AddVisualNode;
        GetTree().NodeRemoved += RemoveVisualNode;
    }

    private void AddVisualNode(Node node)
    {
        VisualData visualData = VisualizeAttributeHandler.RetrieveData(node);

        if (visualData != null)
        {
            (Control visualPanel, List<Action> actions) = VisualUI.CreateVisualPanel(GetTree(), visualData);

            ulong instanceId = node.GetInstanceId();

            Node positionalNode = GetClosestParentOfType(node, typeof(Node2D), typeof(Control));

            if (positionalNode == null)
            {
                PrintUtils.Warning($"[Visualize] No positional parent node could be found for {node.Name} so its visual panel will be created at position (100, 100)");
            }

            if (positionalNode != null)
            {
                // Immediately set the visual panels position to the positional nodes position
                if (positionalNode is Node2D node2D)
                {
                    visualPanel.GlobalPosition = node2D.GlobalPosition;
                }
                else if (positionalNode is Control control)
                {
                    visualPanel.GlobalPosition = control.GlobalPosition;
                }
            }
            else
            {
                visualPanel.GlobalPosition = new Vector2(100, 100);
            }

            // Ensure the added visual panel is not overlapping with any other visual panels
            IEnumerable<Control> controls = _nodeTrackers.Select(x => x.Value.VisualControl);

            Vector2 offset = Vector2.Zero;

            foreach (Control existingControl in controls)
            {
                if (existingControl == visualPanel)
                {
                    continue; // Skip checking against itself
                }

                if (ControlsOverlapping(visualPanel, existingControl))
                {
                    // Move vbox down by the existing controls height
                    offset += new Vector2(0, existingControl.GetRect().Size.Y);
                }
            }

            _nodeTrackers.Add(instanceId, new VisualNodeInfo(actions, visualPanel, positionalNode ?? node, offset));
        }
    }

    private static bool ControlsOverlapping(Control control1, Control control2)
    {
        // Get the bounding rectangles of the control nodes
        Rect2 rect1 = control1.GetRect();
        Rect2 rect2 = control2.GetRect();

        // Check if the rectangles intersect
        return rect1.Intersects(rect2);
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
    }

    public override void _Process(double delta)
    {
        foreach (KeyValuePair<ulong, VisualNodeInfo> kvp in _nodeTrackers)
        {
            VisualNodeInfo info = kvp.Value;
            Node node = info.Node;
            Control visualControl = info.VisualControl;

            // Update position based on node type
            if (node != null) // Checking null here every frame is costly. No need to update the position if the position never changes!
            {
                if (node is Node2D node2D)
                {
                    visualControl.GlobalPosition = node2D.GlobalPosition + info.Offset;
                }
                else if (node is Control control)
                {
                    visualControl.GlobalPosition = control.GlobalPosition + info.Offset;
                }
            }

            // Execute actions
            foreach (Action action in info.Actions)
            {
                action();
            }
        }
    }

    private static Node GetClosestParentOfType(Node node, params Type[] typesToCheck)
    {
        // Check if the current node is of one of the specified types
        if (IsNodeOfType(node, typesToCheck))
        {
            return node;
        }

        // Recursively get the parent and check its type
        Node parent = node.GetParent();

        while (parent != null)
        {
            if (IsNodeOfType(parent, typesToCheck))
            {
                return parent;
            }

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
            {
                return true;
            }
        }

        return false;
    }
#endif
}

public class VisualNodeInfo(List<Action> actions, Control visualControl, Node node, Vector2 offset)
{
    public List<Action> Actions { get; } = actions ?? throw new ArgumentNullException(nameof(actions));
    public Control VisualControl { get; } = visualControl ?? throw new ArgumentNullException(nameof(visualControl));
    public Vector2 Offset { get; } = offset;
    public Node Node { get; } = node ?? throw new ArgumentNullException(nameof(node));
}
