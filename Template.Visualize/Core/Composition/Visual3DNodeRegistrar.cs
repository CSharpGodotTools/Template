#if DEBUG
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GodotUtils.Debugging;

internal sealed class Visual3DNodeRegistrar
{
    private static readonly Vector2I _subViewportSize = new(1024, 1024);
    private const float Base3DHeightOffset = 0.0f;
    private const float Extra3DHeightPerPanel = 0.8f;
    private const float SpritePixelSize = 0.004f;

    public VisualNodeInfo Register(Visual3DRegistrationContext context)
    {
        SubViewport subViewport = new()
        {
            Name = "VisualizeSubViewport",
            TransparentBg = true,
            Disable3D = true,
            Size = _subViewportSize,
            RenderTargetUpdateMode = SubViewport.UpdateMode.WhenVisible
        };

        Vector2 baselinePanelSize = GetPanelSize(context.VisualPanel);
        CenterPanelInSubViewport(context.VisualPanel, baselinePanelSize);
        subViewport.AddChild(context.VisualPanel);

        Sprite3D sprite3D = new()
        {
            Name = $"Visualizing3D {context.AnchorNode.Name} {context.AnchorNode.GetInstanceId()}",
            Texture = subViewport.GetTexture(),
            PixelSize = SpritePixelSize,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            DoubleSided = true
        };

        sprite3D.AddChild(subViewport);
        context.AnchorNode.CallDeferred(Node.MethodName.AddChild, sprite3D);

        Vector3 offset = CalculateVerticalOffset3D(context.ExistingTrackers);

        void UpdatePosition()
        {
            CenterPanelInSubViewport(context.VisualPanel, baselinePanelSize);
            sprite3D.GlobalPosition = context.AnchorNode3D.GlobalPosition + offset;
        }

        return new VisualNodeInfo(context.Actions, context.AnchorNode, sprite3D, UpdatePosition);
    }

    private static Vector3 CalculateVerticalOffset3D(ICollection<VisualNodeInfo> existingTrackers)
    {
        int current3DPanelCount = existingTrackers.Count(tracker => tracker.VisualRoot is Sprite3D);
        return new Vector3(0, Base3DHeightOffset + current3DPanelCount * Extra3DHeightPerPanel, 0);
    }

    private static void CenterPanelInSubViewport(Control visualPanel, Vector2 panelSize)
    {
        Vector2 viewportSize = new(_subViewportSize.X, _subViewportSize.Y);

        Vector2 anchored = (viewportSize - panelSize) * (Vector2.One - VisualAnchorSettings.NormalizedAnchor);
        visualPanel.Position = new Vector2(Mathf.Max(0, anchored.X), Mathf.Max(0, anchored.Y));
    }

    private static Vector2 GetPanelSize(Control visualPanel)
    {
        Vector2 panelSize = visualPanel.GetCombinedMinimumSize();
        if (panelSize == Vector2.Zero)
        {
            panelSize = visualPanel.Size;
        }

        return panelSize;
    }
}
#endif
