#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static class VisualUiElementFactory
{
    public static PanelContainer CreatePanelContainer(string name)
    {
        PanelContainer panelContainer = new()
        {
            // Ensure this info is rendered above all game elements
            Name = name,
            ZIndex = (int)RenderingServer.CanvasItemZMax
        };

        panelContainer.AddThemeStyleboxOverride("panel", new StyleBoxEmpty());

        return panelContainer;
    }

    public static VBoxContainer CreateColoredVBox(Color color)
    {
        return new VBoxContainer
        {
            Modulate = color
        };
    }

    public static CanvasLayer CreateCanvasLayer(string name, ulong instanceId)
    {
        CanvasLayer canvasLayer = new()
        {
            FollowViewportEnabled = true,
            Name = $"Visualizing {name} {instanceId}"
        };
        return canvasLayer;
    }

    public static Button CreateVisibilityButton(Texture2D icon, Color color)
    {
        Button btn = new()
        {
            Name = "Toggle Visibility",
            ToggleMode = true,
            Icon = icon,
            Flat = true,
            ExpandIcon = true,
            SelfModulate = color,
            CustomMinimumSize = Vector2.One * VisualUiLayout.MinButtonSize,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };

        StyleBoxEmpty flatStyle = new();
        // Fix bug causing Visualize icons to turn into 1 pixel when clicked
        btn.AddThemeStyleboxOverride("normal", flatStyle);
        btn.AddThemeStyleboxOverride("hover", flatStyle);
        btn.AddThemeStyleboxOverride("pressed", flatStyle);
        btn.AddThemeStyleboxOverride("hover_pressed", flatStyle);
        btn.AddThemeStyleboxOverride("disabled", flatStyle);
        btn.AddThemeStyleboxOverride("focus", flatStyle);

        return btn;
    }

    public static void SetButtonsToReleaseFocusOnPress(VBoxContainer vboxParent)
    {
        foreach (BaseButton baseButton in vboxParent.GetChildren<BaseButton>())
        {
            baseButton.Pressed += () =>
            {
                Tweens.Animate(baseButton)
                    .Delay(VisualUiLayout.ReleaseFocusOnPressDelay)
                    .Then(baseButton.ReleaseFocus);
            };
        }
    }
}
#endif
