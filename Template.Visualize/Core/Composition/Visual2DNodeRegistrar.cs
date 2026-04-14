#if DEBUG
using Godot;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Registers visualization panels for 2D/canvas anchors inside dedicated canvas layers.
/// </summary>
internal sealed class Visual2DNodeRegistrar
{
    private static readonly Vector2 _defaultOffset2D = new(100, 100);

    /// <summary>
    /// Creates and registers 2D visualization nodes for an anchor.
    /// </summary>
    /// <param name="context">2D registration context.</param>
    /// <returns>Tracked visual node info for update and cleanup.</returns>
    public VisualNodeInfo Register(Visual2DRegistrationContext context)
    {
        // Fall back to a deterministic default when no 2D position can be resolved.
        if (!VisualNodeLocator.TryGetGlobalPosition2D(context.PositionalNode, out Vector2 initialPosition))
        {
            PrintUtils.Warning($"[Visualize] No 2D positional parent found for '{context.AnchorNode.Name}'. Using fallback position {_defaultOffset2D}.");
            initialPosition = _defaultOffset2D;
        }

        Vector2 baselinePanelSize = GetPanelSize(context.VisualPanel);
        context.VisualPanel.GlobalPosition = initialPosition - GetAnchorOffset(baselinePanelSize);
        Vector2 offset = CalculateVerticalOffset2D(context.VisualPanel, context.ExistingTrackers);

        CanvasLayer canvasLayer = VisualUiElementFactory.CreateCanvasLayer(context.AnchorNode.Name, context.AnchorNode.GetInstanceId());
        canvasLayer.AddChild(context.VisualPanel);
        context.AnchorNode.CallDeferred(Node.MethodName.AddChild, canvasLayer);

        void UpdatePosition()
        {
            // Update panel position only while a valid 2D anchor position is available.
            if (VisualNodeLocator.TryGetGlobalPosition2D(context.PositionalNode, out Vector2 position))
                context.VisualPanel.GlobalPosition = position - GetAnchorOffset(baselinePanelSize) + offset;
        }

        return new VisualNodeInfo(context.Actions, context.AnchorNode, canvasLayer, UpdatePosition);
    }

    /// <summary>
    /// Computes vertical offset to reduce overlap with existing 2D visualization panels.
    /// </summary>
    /// <param name="visualPanel">Panel being placed.</param>
    /// <param name="existingTrackers">Existing visual trackers.</param>
    /// <returns>Accumulated overlap offset.</returns>
    private static Vector2 CalculateVerticalOffset2D(Control visualPanel, ICollection<VisualNodeInfo> existingTrackers)
    {
        Vector2 offset = Vector2.Zero;

        foreach (VisualNodeInfo tracker in existingTrackers)
        {
            // Only canvas-layer based visuals participate in 2D overlap offset calculations.
            if (tracker.VisualRoot is not CanvasLayer existingCanvasLayer)
                continue;

            // Skip malformed canvas layers that do not contain a control child.
            if (existingCanvasLayer.GetChildCount() == 0 || existingCanvasLayer.GetChild(0) is not Control existingControl)
                continue;

            // Accumulate offset only when panel rectangles intersect.
            if (!ControlsOverlapping(visualPanel, existingControl))
                continue;

            offset += new Vector2(0, existingControl.GetRect().Size.Y);
        }

        return offset;
    }

    /// <summary>
    /// Checks whether two controls' rectangles intersect.
    /// </summary>
    /// <param name="control1">First control.</param>
    /// <param name="control2">Second control.</param>
    /// <returns><see langword="true"/> when rectangles overlap.</returns>
    private static bool ControlsOverlapping(Control control1, Control control2)
    {
        Rect2 rect1 = control1.GetRect();
        Rect2 rect2 = control2.GetRect();

        return rect1.Intersects(rect2);
    }

    /// <summary>
    /// Converts current anchor settings and panel size into a global anchor offset.
    /// </summary>
    /// <param name="panelSize">Panel size used for offset calculation.</param>
    /// <returns>Computed anchor offset.</returns>
    private static Vector2 GetAnchorOffset(Vector2 panelSize)
    {
        return panelSize * (Vector2.One - VisualAnchorSettings.NormalizedAnchor);
    }

    /// <summary>
    /// Resolves panel size from minimum size metadata with fallback to current control size.
    /// </summary>
    /// <param name="visualPanel">Panel whose size should be measured.</param>
    /// <returns>Resolved panel size.</returns>
    private static Vector2 GetPanelSize(Control visualPanel)
    {
        Vector2 panelSize = visualPanel.GetCombinedMinimumSize();

        // Fall back to current size when minimum-size metadata has not been established yet.
        if (panelSize == Vector2.Zero)
            panelSize = visualPanel.Size;

        return panelSize;
    }
}
#endif
