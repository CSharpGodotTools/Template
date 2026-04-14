#if DEBUG
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GodotUtils.Debugging;

/// <summary>
/// Registers visualization panels for 3D anchors by rendering UI into billboarded sprites.
/// </summary>
internal sealed class Visual3DNodeRegistrar
{
    private static readonly Vector2I _subViewportSize = new(1024, 1024);
    private const float Base3DHeightOffset = 0.0f;
    private const float Extra3DHeightPerPanel = 0.8f;
    private const float SpritePixelSize = 0.004f;

    /// <summary>
    /// Creates and registers 3D visualization nodes for an anchor.
    /// </summary>
    /// <param name="context">3D registration context.</param>
    /// <returns>Tracked visual node info for update and cleanup.</returns>
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

    /// <summary>
    /// Computes vertical stacking offset for new 3D panels based on existing sprite trackers.
    /// </summary>
    /// <param name="existingTrackers">Existing visual trackers.</param>
    /// <returns>Vertical offset vector for the new panel.</returns>
    private static Vector3 CalculateVerticalOffset3D(ICollection<VisualNodeInfo> existingTrackers)
    {
        int current3DPanelCount = existingTrackers.Count(tracker => tracker.VisualRoot is Sprite3D);
        return new Vector3(0, Base3DHeightOffset + (current3DPanelCount * Extra3DHeightPerPanel), 0);
    }

    /// <summary>
    /// Positions a panel inside the sub-viewport according to current anchor settings.
    /// </summary>
    /// <param name="visualPanel">Panel to position.</param>
    /// <param name="panelSize">Panel size used for anchor offset calculations.</param>
    private static void CenterPanelInSubViewport(Control visualPanel, Vector2 panelSize)
    {
        Vector2 viewportSize = new(_subViewportSize.X, _subViewportSize.Y);

        Vector2 anchored = (viewportSize - panelSize) * (Vector2.One - VisualAnchorSettings.NormalizedAnchor);
        visualPanel.Position = new Vector2(Mathf.Max(0, anchored.X), Mathf.Max(0, anchored.Y));
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
