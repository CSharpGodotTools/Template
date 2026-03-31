#if DEBUG
using Godot;
using System;
using System.Linq;
using static Godot.Control;

namespace GodotUtils.Debugging;

internal sealed class VisualTitleBarControlComponent(
    IVisualTitleBarAnchorPopupBuilder anchorPopupBuilder,
    IVisualTitleBarColumnVisibilityController columnVisibilityController,
    IVisualTitleBarValueSyncService valueSyncService)
{
    private const int PopupOffsetY = 4;

    private readonly IVisualTitleBarAnchorPopupBuilder _anchorPopupBuilder = anchorPopupBuilder;
    private readonly IVisualTitleBarColumnVisibilityController _columnVisibilityController = columnVisibilityController;
    private readonly IVisualTitleBarValueSyncService _valueSyncService = valueSyncService;

    public VBoxContainer Build(VisualTitleBarBuildRequest request)
    {
        VBoxContainer container = new();
        HBoxContainer titleRow = new()
        {
            Name = "Title Bar",
            Alignment = BoxContainer.AlignmentMode.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };

        Label title = CreateTitleLabel(request.Name);
        titleRow.AddChild(title);

        VisualTitleBarAnchorPopup anchorPopup = _anchorPopupBuilder.Build();
        titleRow.AddChild(anchorPopup.Popup);

        Button anchorButton = CreateAnchorButton();
        WireAnchorButton(anchorButton, anchorPopup);
        titleRow.AddChild(anchorButton);

        bool hasMutableMembers = request.VisualData.Properties.Any() || request.VisualData.Fields.Any();
        bool hasReadonlyMembers = request.ReadonlyMembers.Length > 0;
        bool hasMethods = request.VisualData.Methods.Any();

        Button? mutableButton = hasMutableMembers ? VisualUiElementFactory.CreateVisibilityButton("W", Colors.White) : null;
        Button? readonlyButton = hasReadonlyMembers ? VisualUiElementFactory.CreateVisibilityButton("R", Colors.White) : null;
        Button? methodsButton = hasMethods ? VisualUiElementFactory.CreateVisibilityButton("M", Colors.White) : null;
        Button? syncButton = (hasMutableMembers && hasReadonlyMembers) ? CreateSyncButton() : null;

        AddIfNotNull(titleRow, mutableButton, readonlyButton, methodsButton, syncButton);

        mutableButton?.SetPressedNoSignal(true);
        readonlyButton?.SetPressedNoSignal(true);
        methodsButton?.SetPressedNoSignal(true);

        void UpdateVisibility()
        {
            _columnVisibilityController.Update(
                request.MutableMembersVbox,
                request.ReadonlyMembersVbox,
                request.MethodsVbox,
                title,
                mutableButton,
                readonlyButton,
                methodsButton);
        }

        RegisterPressedHandler(mutableButton, UpdateVisibility);
        RegisterPressedHandler(readonlyButton, UpdateVisibility);
        RegisterPressedHandler(methodsButton, UpdateVisibility);
        RegisterPressedHandler(syncButton, () => _valueSyncService.SyncMutableFromReadonly(request.MutableMembersVbox, request.ReadonlyMembersVbox));

        container.AddChild(titleRow);
        VisualUiElementFactory.SetButtonsToReleaseFocusOnPress(container);
        UpdateVisibility();

        return container;
    }

    private static Label CreateTitleLabel(string name)
    {
        return new Label
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
    }

    private static Button CreateAnchorButton()
    {
        return new Button
        {
            Name = "Anchor",
            Text = "A",
            SelfModulate = Colors.White,
            CustomMinimumSize = Vector2.One * VisualUiLayout.MinButtonSize,
            Flat = true
        };
    }

    private static Button CreateSyncButton()
    {
        return new Button
        {
            Name = "Sync",
            Text = "S",
            SelfModulate = Colors.White,
            CustomMinimumSize = Vector2.One * VisualUiLayout.MinButtonSize,
            Flat = true
        };
    }

    private static void AddIfNotNull(HBoxContainer row, params Control?[] controls)
    {
        foreach (Control? control in controls)
        {
            if (control != null)
            {
                row.AddChild(control);
            }
        }
    }

    private static void RegisterPressedHandler(Button? button, Action handler)
    {
        if (button == null)
        {
            return;
        }

        void OnPressed() => handler();

        void OnExitedTree()
        {
            button.Pressed -= OnPressed;
            button.TreeExited -= OnExitedTree;
        }

        button.Pressed += OnPressed;
        button.TreeExited += OnExitedTree;
    }

    private static void WireAnchorButton(Button anchorButton, VisualTitleBarAnchorPopup anchorPopup)
    {
        void OnAnchorPressed()
        {
            if (anchorButton.Disabled)
            {
                return;
            }

            anchorPopup.RefreshSelection();
            Vector2 popupSize = anchorPopup.Popup.GetContentsMinimumSize();
            Vector2 anchorScreenPosition = anchorButton.GetScreenPosition();
            Vector2 popupPosition = new(
                anchorScreenPosition.X + (anchorButton.Size.X - popupSize.X) * 0.5f,
                anchorScreenPosition.Y - popupSize.Y - PopupOffsetY);

            Vector2 screenSize = DisplayServer.ScreenGetSize();
            popupPosition.X = Mathf.Clamp(popupPosition.X, 0, Mathf.Max(0, screenSize.X - popupSize.X));
            popupPosition.Y = Mathf.Clamp(popupPosition.Y, 0, Mathf.Max(0, screenSize.Y - popupSize.Y));
            anchorPopup.Popup.Popup(new Rect2I((Vector2I)popupPosition, (Vector2I)popupSize));
        }

        void OnPopupAboutToPopup() => anchorButton.Disabled = true;
        void OnPopupHide() => anchorButton.Disabled = false;

        void OnAnchorExitedTree()
        {
            anchorButton.Pressed -= OnAnchorPressed;
            anchorPopup.Popup.AboutToPopup -= OnPopupAboutToPopup;
            anchorPopup.Popup.PopupHide -= OnPopupHide;
            anchorButton.TreeExited -= OnAnchorExitedTree;
        }

        anchorButton.Pressed += OnAnchorPressed;
        anchorPopup.Popup.AboutToPopup += OnPopupAboutToPopup;
        anchorPopup.Popup.PopupHide += OnPopupHide;
        anchorButton.TreeExited += OnAnchorExitedTree;
    }
}
#endif
