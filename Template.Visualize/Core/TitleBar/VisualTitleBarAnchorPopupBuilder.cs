#if DEBUG
using Godot;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Builds the anchor-selection popup used by visual title-bar controls.
/// </summary>
internal sealed class VisualTitleBarAnchorPopupBuilder : IVisualTitleBarAnchorPopupBuilder
{
    private const int AnchorPopupMargin = 6;
    private const int AnchorPopupGridHSeparation = 4;
    private const int AnchorPopupGridVSeparation = 4;
    private const int AnchorButtonMinWidth = 48;
    private const int AnchorButtonMinHeight = 36;
    private static readonly Color _unselectedAnchorColor = new(0.45f, 0.45f, 0.45f);

    /// <summary>
    /// Creates the popup panel with a 3x3 anchor grid and refresh callback.
    /// </summary>
    /// <returns>Anchor popup record containing panel and selection refresh action.</returns>
    public VisualTitleBarAnchorPopup Build()
    {
        PopupPanel popup = new();
        MarginContainer marginContainer = new();
        marginContainer.AddThemeConstantOverride("margin_left", AnchorPopupMargin);
        marginContainer.AddThemeConstantOverride("margin_top", AnchorPopupMargin);
        marginContainer.AddThemeConstantOverride("margin_right", AnchorPopupMargin);
        marginContainer.AddThemeConstantOverride("margin_bottom", AnchorPopupMargin);

        GridContainer grid = new() { Columns = 3 };
        grid.AddThemeConstantOverride("h_separation", AnchorPopupGridHSeparation);
        grid.AddThemeConstantOverride("v_separation", AnchorPopupGridVSeparation);
        marginContainer.AddChild(grid);
        popup.AddChild(marginContainer);

        List<(Button Button, Vector2 Anchor)> anchorButtons = [];
        AddAnchorButton(grid, popup, anchorButtons, "TL", new Vector2(0.0f, 0.0f));
        AddAnchorButton(grid, popup, anchorButtons, "T", new Vector2(0.5f, 0.0f));
        AddAnchorButton(grid, popup, anchorButtons, "TR", new Vector2(1.0f, 0.0f));
        AddAnchorButton(grid, popup, anchorButtons, "L", new Vector2(0.0f, 0.5f));
        AddAnchorButton(grid, popup, anchorButtons, "C", new Vector2(0.5f, 0.5f));
        AddAnchorButton(grid, popup, anchorButtons, "R", new Vector2(1.0f, 0.5f));
        AddAnchorButton(grid, popup, anchorButtons, "BL", new Vector2(0.0f, 1.0f));
        AddAnchorButton(grid, popup, anchorButtons, "B", new Vector2(0.5f, 1.0f));
        AddAnchorButton(grid, popup, anchorButtons, "BR", new Vector2(1.0f, 1.0f));

        void refreshSelection()
        {
            // Repaint all buttons so only the current anchor appears highlighted.
            foreach ((Button button, Vector2 anchor) in anchorButtons)
            {
                bool isSelected = anchor == VisualAnchorSettings.NormalizedAnchor;
                Color textColor = isSelected ? Colors.White : _unselectedAnchorColor;

                button.RemoveThemeStyleboxOverride("normal");
                button.RemoveThemeStyleboxOverride("hover");
                button.RemoveThemeStyleboxOverride("pressed");
                button.RemoveThemeStyleboxOverride("focus");
                button.AddThemeColorOverride("font_color", textColor);
                button.AddThemeColorOverride("font_hover_color", textColor);
                button.AddThemeColorOverride("font_pressed_color", textColor);
                button.AddThemeColorOverride("font_focus_color", textColor);
            }
        }

        refreshSelection();
        return new VisualTitleBarAnchorPopup(popup, refreshSelection);
    }

    /// <summary>
    /// Adds a single anchor-selection button to the popup grid.
    /// </summary>
    /// <param name="grid">Grid container that hosts anchor buttons.</param>
    /// <param name="popup">Popup panel to hide after selection.</param>
    /// <param name="anchorButtons">List used to track button/anchor associations.</param>
    /// <param name="text">Button label text.</param>
    /// <param name="anchor">Normalized anchor value represented by the button.</param>
    private static void AddAnchorButton(
        GridContainer grid,
        PopupPanel popup,
        List<(Button Button, Vector2 Anchor)> anchorButtons,
        string text,
        Vector2 anchor)
    {
        Button button = new() { Text = text, CustomMinimumSize = new Vector2(AnchorButtonMinWidth, AnchorButtonMinHeight) };

        void OnPressed()
        {
            VisualAnchorSettings.NormalizedAnchor = anchor;
            popup.Hide();
        }

        void OnExitedTree()
        {
            button.Pressed -= OnPressed;
            button.TreeExited -= OnExitedTree;
        }

        button.Pressed += OnPressed;
        button.TreeExited += OnExitedTree;
        anchorButtons.Add((button, anchor));
        grid.AddChild(button);
    }
}
#endif
