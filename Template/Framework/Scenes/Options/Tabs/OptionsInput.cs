using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Coordinates input rebinding behavior for the options menu input tab.
/// </summary>
public sealed partial class OptionsInput : IDisposable
{
    // Constants
    private const string OptionsSceneName = "Options";
    private const string UiPrefix = "ui";
    private const string Ellipsis = "...";

    // Fields
    private readonly Button _resetInputToDefaultsBtn;
    private readonly SceneManager _scene;
    private readonly FocusOutlineManager _focusOutline;
    private readonly HotkeyStore _store;
    private readonly HotkeyListView _view;
    private readonly HotkeyEditor _editor;

    /// <summary>
    /// Initializes input-tab controllers and wires UI events.
    /// </summary>
    /// <param name="options">Options scene instance containing tab nodes.</param>
    /// <param name="inputNavButton">Left-side navigation button used for focus neighbors.</param>
    /// <param name="optionsManager">Manager that stores and persists hotkey bindings.</param>
    /// <param name="sceneManager">Scene manager used for cancel-to-main-menu behavior.</param>
    /// <param name="focusOutline">Focus helper used while navigating dynamic controls.</param>
    public OptionsInput(
        Options options,
        Button inputNavButton,
        OptionsManager optionsManager,
        SceneManager sceneManager,
        FocusOutlineManager focusOutline)
    {
        _scene = sceneManager;
        _focusOutline = focusOutline;

        VBoxContainer content = options.GetNode<VBoxContainer>("%InputContent");

        _store = new HotkeyStore(optionsManager);
        _view = new HotkeyListView(content, inputNavButton, _store, InputActions.RemoveHotkey, UiPrefix, Ellipsis, _focusOutline);
        _view.HotkeyPressed += OnHotkeyButtonPressed;
        _view.PlusPressed += OnPlusButtonPressed;
        _view.Build();

        _editor = new HotkeyEditor(_store, _view, InputActions.RemoveHotkey, InputActions.Fullscreen, _focusOutline);

        _resetInputToDefaultsBtn = options.GetNode<Button>("%ResetInputToDefaults");
        _resetInputToDefaultsBtn.Pressed += OnResetToDefaultsPressed;
    }

    /// <summary>
    /// Unsubscribes events and exits listen mode resources.
    /// </summary>
    public void Dispose()
    {
        _resetInputToDefaultsBtn.Pressed -= OnResetToDefaultsPressed;
        _view.HotkeyPressed -= OnHotkeyButtonPressed;
        _view.PlusPressed -= OnPlusButtonPressed;
        _editor.Clear();
    }

    /// <summary>
    /// Handles input for active keybinding capture or normal cancel navigation.
    /// </summary>
    /// <param name="event">Input event received from the owning options scene.</param>
    public void HandleInput(InputEvent @event)
    {
        // Route input to hotkey editor while actively listening for a new binding.
        if (_editor.IsListening)
        {
            _editor.HandleInput(@event);
            return;
        }

        HandleNonListeningInput();
    }

    /// <summary>
    /// Starts capture flow for an existing binding button.
    /// </summary>
    /// <param name="info">Metadata for the pressed binding button.</param>
    private void OnHotkeyButtonPressed(HotkeyButtonInfo info)
    {
        // Ignore presses while another binding capture is in progress.
        if (_editor.IsListening)
            return;

        _editor.StartListening(info, fromPlus: false);
    }

    /// <summary>
    /// Starts capture flow for a new binding and appends a replacement plus button.
    /// </summary>
    /// <param name="info">Metadata for the pressed plus button.</param>
    private void OnPlusButtonPressed(HotkeyButtonInfo info)
    {
        // Ignore presses while another binding capture is in progress.
        if (_editor.IsListening)
            return;

        _editor.StartListening(info, fromPlus: true);
        _view.AddPlusButton(info.Action);
    }

    /// <summary>
    /// Handles cancel behavior when not actively capturing a new binding.
    /// </summary>
    private void HandleNonListeningInput()
    {
        // Handle only cancel input in non-listening mode.
        if (!Input.IsActionJustPressed(InputActions.UICancel))
            return;

        // Ignore cancel handling when current scene is not the options scene.
        if (_scene.CurrentScene.Name != OptionsSceneName)
            return;

        _scene.SwitchToMainMenu();
    }

    /// <summary>
    /// Restores default hotkeys and rebuilds the input list UI.
    /// </summary>
    private void OnResetToDefaultsPressed()
    {
        _view.Clear();
        _editor.Clear();

        _store.ResetToDefaults();
        _view.Build();
    }

    /// <summary>
    /// Converts an input event into a short human-readable label for buttons.
    /// </summary>
    /// <param name="inputEvent">Input event to format.</param>
    /// <returns>Readable binding text, or empty string for unsupported event types.</returns>
    private static string GetReadableForInput(InputEvent inputEvent)
    {
        // Format keyboard input using Godot's readable key helper.
        if (inputEvent is InputEventKey key)
            return key.Readable();

        // Format mouse-button bindings as "Mouse <index>".
        if (inputEvent is InputEventMouseButton mb)
            return $"Mouse {mb.ButtonIndex}";

        return string.Empty;
    }
}
