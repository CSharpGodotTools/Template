#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal static class VisualUiElementFactory
{
    private const string ToggleVisibilityButtonName = "Toggle Visibility";
    private const string StyleNormal = "normal";
    private const string StyleHover = "hover";
    private const string StylePressed = "pressed";
    private const string StyleHoverPressed = "hover_pressed";
    private const string StyleDisabled = "disabled";
    private const string StyleFocus = "focus";

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

    public static Button CreateVisibilityButton(string text, Color color)
    {
        Button btn = new()
        {
            Name = ToggleVisibilityButtonName,
            ToggleMode = true,
            Text = text,
            Flat = true,
            SelfModulate = color,
            CustomMinimumSize = Vector2.One * VisualUiLayout.MinButtonSize,
        };

        StyleBoxEmpty flatStyle = new();
        // Fix bug causing Visualize icons to turn into 1 pixel when clicked
        btn.AddThemeStyleboxOverride(StyleNormal, flatStyle);
        btn.AddThemeStyleboxOverride(StyleHover, flatStyle);
        btn.AddThemeStyleboxOverride(StylePressed, flatStyle);
        btn.AddThemeStyleboxOverride(StyleHoverPressed, flatStyle);
        btn.AddThemeStyleboxOverride(StyleDisabled, flatStyle);
        btn.AddThemeStyleboxOverride(StyleFocus, flatStyle);

        return btn;
    }

    public static void SetButtonsToReleaseFocusOnPress(VBoxContainer vboxParent)
    {
        foreach (BaseButton baseButton in vboxParent.GetChildren<BaseButton>())
        {
            void OnPressed()
            {
                Tweens.Animate(baseButton)
                    .Delay(VisualUiLayout.ReleaseFocusOnPressDelay)
                    .Then(baseButton.ReleaseFocus);
            }

            void OnExitedTree()
            {
                baseButton.Pressed -= OnPressed;
                baseButton.TreeExited -= OnExitedTree;
            }

            baseButton.Pressed += OnPressed;
            baseButton.TreeExited += OnExitedTree;
        }
    }
}
#endif
