#if DEBUG
using Godot;
using System;
using System.Linq;
using static Godot.Control;

namespace GodotUtils.Debugging;

internal static class VisualTitleBarBuilder
{
    private const int PopupOffsetY = 4;
    private const float ToggleInactiveModulateFactor = 0.65f;
    private const int AnchorPopupMargin = 6;
    private const int AnchorPopupGridHSeparation = 4;
    private const int AnchorPopupGridVSeparation = 4;
    private const int AnchorButtonMinWidth = 48;
    private const int AnchorButtonMinHeight = 36;
    private const float HiddenColumnAlpha = 0f;
    private static readonly Color _hiddenColumnColor = new(1, 1, 1, HiddenColumnAlpha);
    private static readonly Color _unselectedAnchorColor = new(0.45f, 0.45f, 0.45f);


    public static VBoxContainer Build(string name, Control mutableMembersVbox, Control readonlyMembersVbox, Control methodsVbox, VisualData visualData, string[] readonlyMembers)
    {
        VBoxContainer vboxParent = new();

        float mutableColumnWidth = 0;
        float readonlyColumnWidth = 0;

        HBoxContainer titleRow = new()
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

        titleRow.AddChild(title);

        PopupPanel anchorPopup = CreateAnchorPopup(out Action refreshAnchorSelection);
        titleRow.AddChild(anchorPopup);

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
                anchorScreenPosition.Y - popupSize.Y - PopupOffsetY);

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
        titleRow.AddChild(anchorBtn);

        Button? readonlyBtn = null;
        Button? mutableBtn = null;
        Button? methodsBtn = null;
        Button? syncBtn = null;

        if (visualData.Properties.Any() || visualData.Fields.Any())
        {
            mutableBtn = VisualUiElementFactory.CreateVisibilityButton("W", Colors.White);
            mutableBtn.ButtonPressed = true;
            titleRow.AddChild(mutableBtn);
        }

        if (readonlyMembers.Length > 0)
        {
            readonlyBtn = VisualUiElementFactory.CreateVisibilityButton("R", Colors.White);
            readonlyBtn.ButtonPressed = true;
            titleRow.AddChild(readonlyBtn);
        }

        if (visualData.Methods.Any())
        {
            methodsBtn = VisualUiElementFactory.CreateVisibilityButton("M", Colors.White);
            methodsBtn.ButtonPressed = true;
            titleRow.AddChild(methodsBtn);
        }

        if ((visualData.Properties.Any() || visualData.Fields.Any()) && readonlyMembers.Length > 0)
        {
            syncBtn = new Button
            {
                Name = "Sync",
                Text = "S",
                SelfModulate = Colors.White,
                CustomMinimumSize = Vector2.One * VisualUiLayout.MinButtonSize,
                Flat = true
            };
            titleRow.AddChild(syncBtn);
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

        if (methodsBtn != null)
        {
            void OnMethodsPressed()
            {
                UpdateVisibility();
            }

            void OnMethodsExitedTree()
            {
                methodsBtn.Pressed -= OnMethodsPressed;
                methodsBtn.TreeExited -= OnMethodsExitedTree;
            }

            methodsBtn.Pressed += OnMethodsPressed;
            methodsBtn.TreeExited += OnMethodsExitedTree;
        }

        if (syncBtn != null)
        {
            void OnSyncPressed()
            {
                SyncMutableFromReadonly(mutableMembersVbox, readonlyMembersVbox);
            }

            void OnSyncExitedTree()
            {
                syncBtn.Pressed -= OnSyncPressed;
                syncBtn.TreeExited -= OnSyncExitedTree;
            }

            syncBtn.Pressed += OnSyncPressed;
            syncBtn.TreeExited += OnSyncExitedTree;
        }

        vboxParent.AddChild(titleRow);
        VisualUiElementFactory.SetButtonsToReleaseFocusOnPress(vboxParent);

        UpdateVisibility();

        return vboxParent;

        void UpdateVisibility()
        {
            bool mutableVisible = mutableBtn?.ButtonPressed ?? false;
            bool readonlyVisible = readonlyBtn?.ButtonPressed ?? false;
            bool methodsVisible = methodsBtn?.ButtonPressed ?? false;

            mutableColumnWidth = Mathf.Max(mutableColumnWidth, mutableMembersVbox.GetCombinedMinimumSize().X);
            readonlyColumnWidth = Mathf.Max(readonlyColumnWidth, readonlyMembersVbox.GetCombinedMinimumSize().X);

            mutableMembersVbox.CustomMinimumSize = new Vector2(mutableColumnWidth, 0);
            readonlyMembersVbox.CustomMinimumSize = new Vector2(readonlyColumnWidth, 0);

            if (mutableBtn != null)
            {
                mutableBtn.SelfModulate = mutableVisible ? Colors.White : Colors.White * ToggleInactiveModulateFactor;
            }

            if (readonlyBtn != null)
            {
                readonlyBtn.SelfModulate = readonlyVisible ? Colors.White : Colors.White * ToggleInactiveModulateFactor;
            }

            if (methodsBtn != null)
            {
                methodsBtn.SelfModulate = methodsVisible ? Colors.White : Colors.White * ToggleInactiveModulateFactor;
            }

            mutableMembersVbox.Visible = true;
            readonlyMembersVbox.Visible = true;

            SetMutableLabelAlignmentMode(mutableMembersVbox, mutableVisible);
            if (mutableVisible)
            {
                mutableMembersVbox.Modulate = VisualUiResources.MutableMembersColor;
                SetMutableLabelsOnlyMode(mutableMembersVbox, false);
                SyncLabelRowHeights(mutableMembersVbox, readonlyMembersVbox, false);
            }
            else if (readonlyVisible)
            {
                // Keep mutable column width reserved and use it to show labels while pink controls stay fixed.
                mutableMembersVbox.Modulate = VisualUiResources.ReadonlyMembersColor;
                SetMutableLabelsOnlyMode(mutableMembersVbox, true);
                SyncLabelRowHeights(mutableMembersVbox, readonlyMembersVbox, true);
            }
            else
            {
                mutableMembersVbox.Modulate = _hiddenColumnColor;
                SetColumnContentVisible(mutableMembersVbox, false);
                SyncLabelRowHeights(mutableMembersVbox, readonlyMembersVbox, false);
            }

            if (readonlyVisible)
            {
                readonlyMembersVbox.Modulate = VisualUiResources.ReadonlyMembersColor;
                SetColumnContentVisible(readonlyMembersVbox, true);
                SetReadonlyLabelsVisible(readonlyMembersVbox, false);
            }
            else
            {
                readonlyMembersVbox.Modulate = _hiddenColumnColor;
                SetColumnContentVisible(readonlyMembersVbox, false);
                SetReadonlyLabelsVisible(readonlyMembersVbox, false);
            }

            methodsVbox.Visible = methodsVisible;
            methodsVbox.Modulate = methodsVisible ? Colors.White : _hiddenColumnColor;

            title.Visible = true;
        }
    }

    private static PopupPanel CreateAnchorPopup(out Action refreshSelection)
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

    private static void SetColumnContentVisible(Control columnRoot, bool visible)
    {
        foreach (Node child in columnRoot.GetChildren())
        {
            if (child is CanvasItem canvasItem)
            {
                canvasItem.Visible = visible;
            }
        }
    }

    private static void SetMutableLabelsOnlyMode(Control mutableMembersVbox, bool labelsOnly)
    {
        foreach (Node node in mutableMembersVbox.GetChildren())
        {
            if (node is not Control row)
            {
                continue;
            }

            if (row.GetChildCount() == 0)
            {
                row.Visible = !labelsOnly;
                continue;
            }

            if (row.GetChild(0) is not Label)
            {
                row.Visible = !labelsOnly;
                continue;
            }

            row.Visible = true;

            for (int i = 1; i < row.GetChildCount(); i++)
            {
                if (row.GetChild(i) is CanvasItem canvasItem)
                {
                    canvasItem.Visible = !labelsOnly;
                }
            }
        }
    }

    private static void SyncLabelRowHeights(Control mutableMembersVbox, Control readonlyMembersVbox, bool matchReadonly)
    {
        int rowCount = Mathf.Min(mutableMembersVbox.GetChildCount(), readonlyMembersVbox.GetChildCount());

        for (int i = 0; i < rowCount; i++)
        {
            if (mutableMembersVbox.GetChild(i) is not Control mutableRow)
            {
                continue;
            }

            if (!matchReadonly)
            {
                mutableRow.CustomMinimumSize = new Vector2(mutableRow.CustomMinimumSize.X, 0);
                continue;
            }

            if (readonlyMembersVbox.GetChild(i) is not Control readonlyRow)
            {
                continue;
            }

            float readonlyHeight = readonlyRow.GetCombinedMinimumSize().Y;
            mutableRow.CustomMinimumSize = new Vector2(mutableRow.CustomMinimumSize.X, readonlyHeight);
        }
    }

    private static void SetMutableLabelAlignmentMode(Control mutableMembersVbox, bool mutableVisible)
    {
        foreach (Node node in mutableMembersVbox.GetChildren())
        {
            if (node is not HBoxContainer row || row.GetChildCount() == 0)
            {
                continue;
            }

            if (row.GetChild(0) is not Label label)
            {
                continue;
            }

            row.Alignment = mutableVisible ? BoxContainer.AlignmentMode.Begin : BoxContainer.AlignmentMode.End;
            label.HorizontalAlignment = HorizontalAlignment.Right;
            label.SizeFlagsHorizontal = SizeFlags.Fill;
            label.CustomMinimumSize = new Vector2(VisualUiLayout.MemberLabelMinWidth, label.CustomMinimumSize.Y);
        }
    }

    private static void SyncMutableFromReadonly(Control mutableMembersVbox, Control readonlyMembersVbox)
    {
        int rowCount = Mathf.Min(mutableMembersVbox.GetChildCount(), readonlyMembersVbox.GetChildCount());

        for (int i = 0; i < rowCount; i++)
        {
            Control? mutableValueControl = ResolveValueControl(mutableMembersVbox.GetChild(i), true);
            Control? readonlyValueControl = ResolveValueControl(readonlyMembersVbox.GetChild(i), false);

            if (mutableValueControl == null || readonlyValueControl == null)
            {
                continue;
            }

            CopyControlValues(readonlyValueControl, mutableValueControl);
        }
    }

    private static Control? ResolveValueControl(Node rowNode, bool isMutable)
    {
        if (rowNode is VBoxContainer vbox && vbox.GetChildCount() >= 2)
        {
            if (vbox.GetChild(1) is HBoxContainer nestedRow && nestedRow.GetChildCount() > 0)
            {
                int controlIndex = isMutable ? nestedRow.GetChildCount() - 1 : 1;
                return nestedRow.GetChild(controlIndex) as Control;
            }
        }

        if (rowNode is HBoxContainer row && row.GetChildCount() > 0)
        {
            int controlIndex = isMutable ? row.GetChildCount() - 1 : 1;
            return row.GetChild(controlIndex) as Control;
        }

        return null;
    }

    private static void CopyControlValues(Control source, Control target)
    {
        if (source is LineEdit sourceLineEdit && target is LineEdit targetLineEdit)
        {
            targetLineEdit.Text = sourceLineEdit.Text;
            return;
        }

        if (source is SpinBox sourceSpinBox && target is SpinBox targetSpinBox)
        {
            targetSpinBox.Value = sourceSpinBox.Value;
            return;
        }

        if (source is CheckBox sourceCheckBox && target is CheckBox targetCheckBox)
        {
            targetCheckBox.ButtonPressed = sourceCheckBox.ButtonPressed;
            return;
        }

        if (source is OptionButton sourceOptionButton && target is OptionButton targetOptionButton)
        {
            targetOptionButton.Select(sourceOptionButton.Selected);
            return;
        }

        if (source is ColorPickerButton sourceColorPicker && target is ColorPickerButton targetColorPicker)
        {
            targetColorPicker.Color = sourceColorPicker.Color;
            return;
        }

        int childCount = Mathf.Min(source.GetChildCount(), target.GetChildCount());
        for (int i = 0; i < childCount; i++)
        {
            if (source.GetChild(i) is Control sourceChild && target.GetChild(i) is Control targetChild)
            {
                CopyControlValues(sourceChild, targetChild);
            }
        }
    }

}
#endif
