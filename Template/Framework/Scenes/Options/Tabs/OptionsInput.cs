using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.Ui;

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

    public void Dispose()
    {
        _resetInputToDefaultsBtn.Pressed -= OnResetToDefaultsPressed;
        _view.HotkeyPressed -= OnHotkeyButtonPressed;
        _view.PlusPressed -= OnPlusButtonPressed;
        _editor.Clear();
    }

    public void HandleInput(InputEvent @event)
    {
        if (_editor.IsListening)
        {
            _editor.HandleInput(@event);
            return;
        }

        HandleNonListeningInput();
    }

    private void OnHotkeyButtonPressed(HotkeyButtonInfo info)
    {
        if (_editor.IsListening)
            return;

        _editor.StartListening(info, fromPlus: false);
    }

    private void OnPlusButtonPressed(HotkeyButtonInfo info)
    {
        if (_editor.IsListening)
            return;

        _editor.StartListening(info, fromPlus: true);
        _view.AddPlusButton(info.Action);
    }

    private void HandleNonListeningInput()
    {
        if (!Input.IsActionJustPressed(InputActions.UICancel))
            return;

        if (_scene.CurrentScene.Name != OptionsSceneName)
            return;

        _scene.SwitchToMainMenu();
    }

    private void OnResetToDefaultsPressed()
    {
        _view.Clear();
        _editor.Clear();

        _store.ResetToDefaults();
        _view.Build();
    }

    private static string GetReadableForInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey key)
            return key.Readable();

        if (inputEvent is InputEventMouseButton mb)
            return $"Mouse {mb.ButtonIndex}";

        return string.Empty;
    }
}
