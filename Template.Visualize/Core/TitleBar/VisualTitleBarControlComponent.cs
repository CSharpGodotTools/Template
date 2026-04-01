#if DEBUG
using Godot;
using System;
using System.Linq;
using static Godot.Control;

namespace GodotUtils.Debugging;

/// <summary>
/// Builds and wires title-bar controls for visual panels, including visibility toggles and anchor popup behavior.
/// </summary>
/// <param name="anchorPopupBuilder">Builder that creates and configures the anchor popup UI.</param>
/// <param name="columnVisibilityController">Service that applies mutable/readonly/method visibility rules.</param>
/// <param name="valueSyncService">Service that syncs readonly values into mutable controls.</param>
internal sealed class VisualTitleBarControlComponent(
    IVisualTitleBarAnchorPopupBuilder anchorPopupBuilder,
    IVisualTitleBarColumnVisibilityController columnVisibilityController,
    IVisualTitleBarValueSyncService valueSyncService)
{
    private const int PopupOffsetY = 4;

    private readonly IVisualTitleBarAnchorPopupBuilder _anchorPopupBuilder = anchorPopupBuilder;
    private readonly IVisualTitleBarColumnVisibilityController _columnVisibilityController = columnVisibilityController;
    private readonly IVisualTitleBarValueSyncService _valueSyncService = valueSyncService;

    /// <summary>
    /// Creates the title-bar container and wires all visibility, sync, and anchor popup interactions.
    /// </summary>
    /// <param name="request">Build request containing visual data and column containers.</param>
    /// <returns>Configured title-bar container.</returns>
    public VBoxContainer Build(VisualTitleBarBuildRequest request)
    {
        // Root container that will host the assembled title-row controls.
        VBoxContainer container = new();

        // Horizontal row containing title text and all action/toggle buttons.
        HBoxContainer titleRow = new()
        {
            Name = "Title Bar",
            Alignment = BoxContainer.AlignmentMode.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };

        // Title reflects the visualized object/member group name.
        Label title = CreateTitleLabel(request.Name);
        titleRow.AddChild(title);

        // Anchor popup allows selecting where panel visuals are attached in the scene.
        VisualTitleBarAnchorPopup anchorPopup = _anchorPopupBuilder.Build();
        titleRow.AddChild(anchorPopup.Popup);

        // Anchor button toggles and positions the popup near the title row.
        Button anchorButton = CreateAnchorButton();
        WireAnchorButton(anchorButton, anchorPopup);
        titleRow.AddChild(anchorButton);

        // Detect which columns exist so optional buttons are only created when needed.
        bool hasMutableMembers = request.VisualData.Properties.Any() || request.VisualData.Fields.Any();
        bool hasReadonlyMembers = request.ReadonlyMembers.Length > 0;
        bool hasMethods = request.VisualData.Methods.Any();

        // Create visibility toggles for each available column.
        Button? mutableButton = hasMutableMembers ? VisualUiElementFactory.CreateVisibilityButton("W", Colors.White) : null;
        Button? readonlyButton = hasReadonlyMembers ? VisualUiElementFactory.CreateVisibilityButton("R", Colors.White) : null;
        Button? methodsButton = hasMethods ? VisualUiElementFactory.CreateVisibilityButton("M", Colors.White) : null;

        // Sync action is only meaningful when both mutable and readonly columns exist.
        Button? syncButton = (hasMutableMembers && hasReadonlyMembers) ? CreateSyncButton() : null;

        // Attach whichever optional controls were created for this panel shape.
        AddIfNotNull(titleRow, mutableButton, readonlyButton, methodsButton, syncButton);

        // Default all visibility toggles to pressed (visible) without firing handlers during setup.
        mutableButton?.SetPressedNoSignal(true);
        readonlyButton?.SetPressedNoSignal(true);
        methodsButton?.SetPressedNoSignal(true);

        // Centralized visibility update keeps title/columns consistent across all toggle interactions.
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

        // Wire each toggle to the shared visibility update routine.
        RegisterPressedHandler(mutableButton, UpdateVisibility);
        RegisterPressedHandler(readonlyButton, UpdateVisibility);
        RegisterPressedHandler(methodsButton, UpdateVisibility);

        // Wire sync action to copy readonly values back into mutable controls.
        RegisterPressedHandler(syncButton, () => _valueSyncService.SyncMutableFromReadonly(request.MutableMembersVbox, request.ReadonlyMembersVbox));

        // Finalize tree attachment and standard button focus behavior.
        container.AddChild(titleRow);
        VisualUiElementFactory.SetButtonsToReleaseFocusOnPress(container);

        // Apply initial visibility once all controls and handlers are connected.
        UpdateVisibility();

        return container;
    }

    /// <summary>
    /// Creates the title label used in the title-bar row.
    /// </summary>
    /// <param name="name">Display name shown in the title label.</param>
    /// <returns>Configured title label control.</returns>
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

    /// <summary>
    /// Creates the anchor popup toggle button.
    /// </summary>
    /// <returns>Configured anchor button.</returns>
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

    /// <summary>
    /// Creates the value-sync button shown when mutable and readonly columns are both present.
    /// </summary>
    /// <returns>Configured sync button.</returns>
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

    /// <summary>
    /// Adds non-null controls to a row in argument order.
    /// </summary>
    /// <param name="row">Row container that receives controls.</param>
    /// <param name="controls">Controls to add when non-null.</param>
    private static void AddIfNotNull(HBoxContainer row, params Control?[] controls)
    {
        foreach (Control? control in controls)
        {
            // Optional controls are added only when they were created for this panel configuration.
            if (control != null)
            {
                row.AddChild(control);
            }
        }
    }

    /// <summary>
    /// Subscribes a pressed handler and automatically detaches it when the button exits the tree.
    /// </summary>
    /// <param name="button">Button to wire.</param>
    /// <param name="handler">Action to invoke on press.</param>
    private static void RegisterPressedHandler(Button? button, Action handler)
    {
        // Null buttons represent disabled feature paths for the current panel.
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

    /// <summary>
    /// Wires anchor button events to popup show/hide behavior and screen-clamped positioning.
    /// </summary>
    /// <param name="anchorButton">Anchor toggle button.</param>
    /// <param name="anchorPopup">Popup component shown by the button.</param>
    private static void WireAnchorButton(Button anchorButton, VisualTitleBarAnchorPopup anchorPopup)
    {
        void OnAnchorPressed()
        {
            // Ignore presses while popup lifecycle callbacks have the button temporarily disabled.
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
