#if DEBUG
using Godot;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class Visual2DNodeRegistrar
{
    private static readonly Vector2 _defaultOffset2D = new(100, 100);

    public VisualNodeInfo Register(Visual2DRegistrationContext context)
    {
        if (!VisualNodeLocator.TryGetGlobalPosition2D(context.PositionalNode, out Vector2 initialPosition))
        {
            PrintUtils.Warning($"[Visualize] No 2D positional parent found for '{context.AnchorNode.Name}'. Using fallback position {_defaultOffset2D}.");
            initialPosition = _defaultOffset2D;
        }

        context.VisualPanel.GlobalPosition = initialPosition;
        Vector2 offset = CalculateVerticalOffset2D(context.VisualPanel, context.ExistingTrackers);

        CanvasLayer canvasLayer = VisualUiElementFactory.CreateCanvasLayer(context.AnchorNode.Name, context.AnchorNode.GetInstanceId());
        canvasLayer.AddChild(context.VisualPanel);
        context.AnchorNode.CallDeferred(Node.MethodName.AddChild, canvasLayer);

        void UpdatePosition()
        {
            if (VisualNodeLocator.TryGetGlobalPosition2D(context.PositionalNode, out Vector2 position))
            {
                context.VisualPanel.GlobalPosition = position + offset;
            }
        }

        return new VisualNodeInfo(context.Actions, context.AnchorNode, canvasLayer, UpdatePosition);
    }

    private static Vector2 CalculateVerticalOffset2D(Control visualPanel, ICollection<VisualNodeInfo> existingTrackers)
    {
        Vector2 offset = Vector2.Zero;

        foreach (VisualNodeInfo tracker in existingTrackers)
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

    private static bool ControlsOverlapping(Control control1, Control control2)
    {
        Rect2 rect1 = control1.GetRect();
        Rect2 rect2 = control2.GetRect();

        return rect1.Intersects(rect2);
    }
}
#endif
