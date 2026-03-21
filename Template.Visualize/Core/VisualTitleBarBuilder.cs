#if DEBUG
using Godot;
using System;
using System.Linq;
using static Godot.Control;

namespace GodotUtils.Debugging;

internal static class VisualTitleBarBuilder
{

    public static VBoxContainer Build(string name, Control mutableMembersVbox, Control readonlyMembersVbox, VisualData visualData, string[] readonlyMembers)
    {
        VBoxContainer vboxParent = new();

        HBoxContainer hbox = new()
        {
            Name = "Title Bar",
            Alignment = BoxContainer.AlignmentMode.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };

        Label title = new()
        {
            Name = "Title",
            Text = name,
            Visible = true,
            LabelSettings = new LabelSettings
            {
                FontSize = VisualUiLayout.TitleFontSize,
                FontColor = Colors.LightSkyBlue,
                OutlineColor = Colors.Black,
                OutlineSize = VisualUiLayout.FontOutlineSize,
            }
        };

        hbox.AddChild(title);

        PopupPanel anchorPopup = CreateAnchorPopup(out Action refreshAnchorSelection);
        hbox.AddChild(anchorPopup);

        Button anchorBtn = new()
        {
            Name = "Anchor",
            Text = "A",
            SelfModulate = Colors.White,
            CustomMinimumSize = Vector2.One * VisualUiLayout.MinButtonSize,
            Flat = true
        };

        void OnAnchorPressed()
        {
            if (anchorBtn.Disabled)
            {
                return;
            }

            refreshAnchorSelection();
            Vector2 popupSize = anchorPopup.GetContentsMinimumSize();
            Vector2 anchorScreenPosition = anchorBtn.GetScreenPosition();

            Vector2 popupPosition = new(
                anchorScreenPosition.X + (anchorBtn.Size.X - popupSize.X) * 0.5f,
                anchorScreenPosition.Y - popupSize.Y - 4);

            Vector2 screenSize = DisplayServer.ScreenGetSize();
            popupPosition.X = Mathf.Clamp(popupPosition.X, 0, Mathf.Max(0, screenSize.X - popupSize.X));
            popupPosition.Y = Mathf.Clamp(popupPosition.Y, 0, Mathf.Max(0, screenSize.Y - popupSize.Y));

            anchorPopup.Popup(new Rect2I((Vector2I)popupPosition, (Vector2I)popupSize));
        }

        void OnAnchorPopupAboutToPopup()
        {
            anchorBtn.Disabled = true;
        }

        void OnAnchorPopupHide()
        {
            anchorBtn.Disabled = false;
        }

        void OnAnchorExitedTree()
        {
            anchorBtn.Pressed -= OnAnchorPressed;
            anchorPopup.AboutToPopup -= OnAnchorPopupAboutToPopup;
            anchorPopup.PopupHide -= OnAnchorPopupHide;
            anchorBtn.TreeExited -= OnAnchorExitedTree;
        }

        anchorBtn.Pressed += OnAnchorPressed;
        anchorPopup.AboutToPopup += OnAnchorPopupAboutToPopup;
        anchorPopup.PopupHide += OnAnchorPopupHide;
        anchorBtn.TreeExited += OnAnchorExitedTree;
        hbox.AddChild(anchorBtn);

        Button? readonlyBtn = null;
        Button? mutableBtn = null;

        if (visualData.Properties.Any() || visualData.Fields.Any())
        {
            mutableBtn = VisualUiElementFactory.CreateVisibilityButton("W", Colors.White);
            mutableBtn.ButtonPressed = true;
            hbox.AddChild(mutableBtn);
        }

        if (readonlyMembers.Length > 0)
        {
            readonlyBtn = VisualUiElementFactory.CreateVisibilityButton("R", Colors.White);
            readonlyBtn.ButtonPressed = true;
            hbox.AddChild(readonlyBtn);
        }

        if (readonlyBtn != null)
        {
            void OnReadonlyPressed()
            {
                UpdateVisibility();
            }

            void OnReadonlyExitedTree()
            {
                readonlyBtn.Pressed -= OnReadonlyPressed;
                readonlyBtn.TreeExited -= OnReadonlyExitedTree;
            }

            readonlyBtn.Pressed += OnReadonlyPressed;
            readonlyBtn.TreeExited += OnReadonlyExitedTree;
        }

        if (mutableBtn != null)
        {
            void OnMutablePressed()
            {
                UpdateVisibility();
            }

            void OnMutableExitedTree()
            {
                mutableBtn.Pressed -= OnMutablePressed;
                mutableBtn.TreeExited -= OnMutableExitedTree;
            }

            mutableBtn.Pressed += OnMutablePressed;
            mutableBtn.TreeExited += OnMutableExitedTree;
        }

        vboxParent.AddChild(hbox);
        VisualUiElementFactory.SetButtonsToReleaseFocusOnPress(vboxParent);

        UpdateVisibility();

        return vboxParent;

        void UpdateVisibility()
        {
            bool mutableVisible = mutableBtn?.ButtonPressed ?? false;
            bool readonlyVisible = readonlyBtn?.ButtonPressed ?? false;

            if (mutableBtn != null)
            {
                mutableBtn.SelfModulate = mutableVisible ? Colors.White : Colors.White * 0.65f;
            }

            if (readonlyBtn != null)
            {
                readonlyBtn.SelfModulate = readonlyVisible ? Colors.White : Colors.White * 0.65f;
            }

            readonlyMembersVbox.Visible = readonlyVisible;

            if (mutableVisible)
            {
                mutableMembersVbox.Visible = true;
                mutableMembersVbox.Modulate = VisualUiResources.MutableMembersColor;
                SetReadonlyLabelsVisible(readonlyMembersVbox, false);
            }
            else if (readonlyVisible)
            {
                // In pink-only mode, labels should be pink and aligned with pink controls.
                mutableMembersVbox.Visible = false;
                SetReadonlyLabelsVisible(readonlyMembersVbox, true);
            }
            else
            {
                mutableMembersVbox.Visible = false;
                mutableMembersVbox.Modulate = VisualUiResources.MutableMembersColor;
                SetReadonlyLabelsVisible(readonlyMembersVbox, false);
            }

            title.Visible = true;
        }
    }

    private static PopupPanel CreateAnchorPopup(out Action refreshSelection)
    {
        PopupPanel popup = new();
        MarginContainer marginContainer = new();
        marginContainer.AddThemeConstantOverride("margin_left", 6);
        marginContainer.AddThemeConstantOverride("margin_top", 6);
        marginContainer.AddThemeConstantOverride("margin_right", 6);
        marginContainer.AddThemeConstantOverride("margin_bottom", 6);

        GridContainer grid = new() { Columns = 3 };
        grid.AddThemeConstantOverride("h_separation", 4);
        grid.AddThemeConstantOverride("v_separation", 4);
        marginContainer.AddChild(grid);
        popup.AddChild(marginContainer);

        System.Collections.Generic.List<(Button Button, Vector2 Anchor)> anchorButtons = [];

        AddAnchorButton(grid, popup, anchorButtons, "TL", new Vector2(0.0f, 0.0f));
        AddAnchorButton(grid, popup, anchorButtons, "T",  new Vector2(0.5f, 0.0f));
        AddAnchorButton(grid, popup, anchorButtons, "TR", new Vector2(1.0f, 0.0f));
        AddAnchorButton(grid, popup, anchorButtons, "L",  new Vector2(0.0f, 0.5f));
        AddAnchorButton(grid, popup, anchorButtons, "C",  new Vector2(0.5f, 0.5f));
        AddAnchorButton(grid, popup, anchorButtons, "R",  new Vector2(1.0f, 0.5f));
        AddAnchorButton(grid, popup, anchorButtons, "BL", new Vector2(0.0f, 1.0f));
        AddAnchorButton(grid, popup, anchorButtons, "B",  new Vector2(0.5f, 1.0f));
        AddAnchorButton(grid, popup, anchorButtons, "BR", new Vector2(1.0f, 1.0f));

        refreshSelection = () =>
        {
            foreach ((Button button, Vector2 anchor) in anchorButtons)
            {
                bool isSelected = anchor == VisualAnchorSettings.NormalizedAnchor;
                Color textColor = isSelected ? Colors.White : new Color(0.45f, 0.45f, 0.45f);

                button.RemoveThemeStyleboxOverride("normal");
                button.RemoveThemeStyleboxOverride("hover");
                button.RemoveThemeStyleboxOverride("pressed");
                button.RemoveThemeStyleboxOverride("focus");

                button.AddThemeColorOverride("font_color", textColor);
                button.AddThemeColorOverride("font_hover_color", textColor);
                button.AddThemeColorOverride("font_pressed_color", textColor);
                button.AddThemeColorOverride("font_focus_color", textColor);
            }
        };

        refreshSelection();

        return popup;
    }

    private static void AddAnchorButton(
        GridContainer grid,
        PopupPanel popup,
        System.Collections.Generic.List<(Button Button, Vector2 Anchor)> anchorButtons,
        string text,
        Vector2 anchor)
    {
        Button button = new() { Text = text, CustomMinimumSize = new Vector2(48, 36) };

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

    private static void SetReadonlyLabelsVisible(Control readonlyMembersVbox, bool visible)
    {
        foreach (Node node in readonlyMembersVbox.GetChildren())
        {
            if (node is not HBoxContainer row || row.GetChildCount() == 0)
            {
                continue;
            }

            if (row.GetChild(0) is Label label)
            {
                label.Visible = visible;
            }
        }
    }
}
#endif
