#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotUtils.Debugging;

internal sealed class VisualNodeManager
{
    private static readonly Type[] _positionNodeTypes = [typeof(Node2D), typeof(Control), typeof(Node3D)];
    private static readonly Vector2 _defaultOffset2D = new(100, 100);
    private static readonly Vector2I _subViewportSize = new(1024, 1024);
    private const float Base3DHeightOffset = -0.2f;
    private const float Extra3DHeightPerPanel = 0.8f;
    private const float SpritePixelSize = 0.004f;

    private readonly Dictionary<ulong, VisualNodeInfo> _nodeTrackers = [];

    public void Register(Node node, object visualizedObject)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(visualizedObject);

        VisualData? visualData = VisualizeAttributeHandler.RetrieveData(visualizedObject, node);
        if (visualData == null)
        {
            node.TreeExited += () => RemoveVisualNodesForAnchor(node);
            return;
        }

        (Control visualPanel, IReadOnlyList<Action> actions) = VisualUI.CreateVisualPanel(visualData);
        Node? positionalNode = GetClosestParentOfType(node, _positionNodeTypes) ?? node;

        if (positionalNode is Node3D node3D)
        {
            Register3D(node, node3D, visualPanel, actions);
        }
        else
        {
            Register2D(node, positionalNode, visualPanel, actions);
        }

        node.TreeExited += () => RemoveVisualNodesForAnchor(node);
    }

    public void Update()
    {
        foreach (VisualNodeInfo info in _nodeTrackers.Values)
        {
            info.UpdatePosition();

            foreach (Action action in info.Actions)
            {
                action();
            }
        }
    }

    private void Register2D(Node anchorNode, Node positionalNode, Control visualPanel, IReadOnlyList<Action> actions)
    {
        if (!TryGetGlobalPosition2D(positionalNode, out Vector2 initialPosition))
        {
            PrintUtils.Warning($"[Visualize] No 2D positional parent found for '{anchorNode.Name}'. Using fallback position {_defaultOffset2D}.");
            initialPosition = _defaultOffset2D;
        }

        visualPanel.GlobalPosition = initialPosition;
        Vector2 offset = CalculateVerticalOffset2D(visualPanel);

        CanvasLayer canvasLayer = VisualUiElementFactory.CreateCanvasLayer(anchorNode.Name, anchorNode.GetInstanceId());
        canvasLayer.AddChild(visualPanel);
        anchorNode.CallDeferred(Node.MethodName.AddChild, canvasLayer);

        void UpdatePosition()
        {
            if (TryGetGlobalPosition2D(positionalNode, out Vector2 position))
            {
                visualPanel.GlobalPosition = position + offset;
            }
        }

        _nodeTrackers.Add(visualPanel.GetInstanceId(), new VisualNodeInfo(actions, anchorNode, canvasLayer, UpdatePosition));
    }

    private void Register3D(Node anchorNode, Node3D node3D, Control visualPanel, IReadOnlyList<Action> actions)
    {
        SubViewport subViewport = new()
        {
            Name = "VisualizeSubViewport",
            TransparentBg = true,
            Disable3D = true,
            Size = _subViewportSize,
            RenderTargetUpdateMode = SubViewport.UpdateMode.WhenVisible
        };

        CenterPanelInSubViewport(visualPanel);
        subViewport.AddChild(visualPanel);

        Sprite3D sprite3D = new()
        {
            Name = $"Visualizing3D {anchorNode.Name} {anchorNode.GetInstanceId()}",
            Texture = subViewport.GetTexture(),
            PixelSize = SpritePixelSize,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            DoubleSided = true
        };

        sprite3D.AddChild(subViewport);
        anchorNode.CallDeferred(Node.MethodName.AddChild, sprite3D);

        Vector3 offset = CalculateVerticalOffset3D();

        void UpdatePosition()
        {
            CenterPanelInSubViewport(visualPanel);
            sprite3D.GlobalPosition = node3D.GlobalPosition + offset;
        }

        _nodeTrackers.Add(visualPanel.GetInstanceId(), new VisualNodeInfo(actions, anchorNode, sprite3D, UpdatePosition));
    }

    private void RemoveVisualNodesForAnchor(Node anchorNode)
    {
        List<ulong> trackersToRemove = [];

        foreach (KeyValuePair<ulong, VisualNodeInfo> tracker in _nodeTrackers)
        {
            if (tracker.Value.AnchorNode != anchorNode)
            {
                continue;
            }

            tracker.Value.VisualRoot.QueueFree();
            trackersToRemove.Add(tracker.Key);
        }

        foreach (ulong id in trackersToRemove)
        {
            _nodeTrackers.Remove(id);
        }

        VisualizeAutoload.Instance?.UnregisterNode(anchorNode);
    }

    private Vector2 CalculateVerticalOffset2D(Control visualPanel)
    {
        Vector2 offset = Vector2.Zero;

        foreach (VisualNodeInfo tracker in _nodeTrackers.Values)
        {
            if (tracker.VisualRoot is not CanvasLayer existingCanvasLayer)
            {
                continue;
            }

            if (existingCanvasLayer.GetChildCount() == 0 || existingCanvasLayer.GetChild(0) is not Control existingControl)
            {
                continue;
            }

            if (!ControlsOverlapping(visualPanel, existingControl))
            {
                continue;
            }

            offset += new Vector2(0, existingControl.GetRect().Size.Y);
        }

        return offset;
    }

    private Vector3 CalculateVerticalOffset3D()
    {
        int current3DPanelCount = _nodeTrackers.Values.Count(tracker => tracker.VisualRoot is Sprite3D);
        return new Vector3(0, Base3DHeightOffset + current3DPanelCount * Extra3DHeightPerPanel, 0);
    }

    private void CenterPanelInSubViewport(Control visualPanel)
    {
        Vector2 viewportSize = new(_subViewportSize.X, _subViewportSize.Y);

        Vector2 panelSize = visualPanel.GetCombinedMinimumSize();
        if (panelSize == Vector2.Zero)
        {
            panelSize = visualPanel.Size;
        }

        Vector2 centered = (viewportSize - panelSize) * 0.5f;
        visualPanel.Position = new Vector2(Mathf.Max(0, centered.X), Mathf.Max(0, centered.Y));
    }

    private static bool TryGetGlobalPosition2D(Node? node, out Vector2 position)
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
        {
            return node;
        }

        Node? parent = node.GetParent();

        while (parent != null)
        {
            if (IsNodeOfType(parent, typesToCheck))
            {
                return parent;
            }

            parent = parent.GetParent();
        }

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

    private static bool ControlsOverlapping(Control control1, Control control2)
    {
        Rect2 rect1 = control1.GetRect();
        Rect2 rect2 = control2.GetRect();

        return rect1.Intersects(rect2);
    }
}
#endif
