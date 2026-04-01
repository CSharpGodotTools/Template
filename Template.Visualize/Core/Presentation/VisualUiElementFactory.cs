#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Factory helpers for common visualization UI controls and containers.
/// </summary>
internal static class VisualUiElementFactory
{
    private const string ToggleVisibilityButtonName = "Toggle Visibility";
    private const string StyleNormal = "normal";
    private const string StyleHover = "hover";
    private const string StylePressed = "pressed";
    private const string StyleHoverPressed = "hover_pressed";
    private const string StyleDisabled = "disabled";
    private const string StyleFocus = "focus";

    /// <summary>
    /// Creates the root panel container for a visualization panel.
    /// </summary>
    /// <param name="name">Panel node name.</param>
    /// <returns>Configured panel container.</returns>
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

    /// <summary>
    /// Creates a vertically stacked container tinted with the supplied color.
    /// </summary>
    /// <param name="color">Tint color applied to the VBox container.</param>
    /// <returns>Configured VBox container.</returns>
    public static VBoxContainer CreateColoredVBox(Color color)
    {
        return new VBoxContainer
        {
            Modulate = color
        };
    }

    /// <summary>
    /// Creates a canvas layer used to host 2D visualization panels.
    /// </summary>
    /// <param name="name">Anchor node name.</param>
    /// <param name="instanceId">Anchor node instance ID.</param>
    /// <returns>Configured canvas layer.</returns>
    public static CanvasLayer CreateCanvasLayer(string name, ulong instanceId)
    {
        CanvasLayer canvasLayer = new()
        {
            FollowViewportEnabled = true,
            Name = $"Visualizing {name} {instanceId}"
        };
        return canvasLayer;
    }

    /// <summary>
    /// Creates a flat toggle button used for title-bar visibility controls.
    /// </summary>
    /// <param name="text">Button label text.</param>
    /// <param name="color">Button tint color.</param>
    /// <returns>Configured toggle button.</returns>
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

    /// <summary>
    /// Wires buttons to release focus shortly after press.
    /// </summary>
    /// <param name="vboxParent">Container whose direct button children are wired.</param>
    public static void SetButtonsToReleaseFocusOnPress(VBoxContainer vboxParent)
    {
        foreach (BaseButton baseButton in vboxParent.GetChildren<BaseButton>())
        {
            void OnPressed()
            {
                // Delay focus release so button press visuals can complete first.
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
