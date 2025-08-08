using Godot;
using GodotUtils;
using System;
using System.Linq;
using Godot.Collections;
using GodotUtils.UI;

namespace __TEMPLATE__.UI;

public partial class OptionsInput : Control
{
    private const string RemoveHotkeyAction = "remove_hotkey";
    private const string FullscreenAction = "fullscreen";
    private const string OptionsSceneName = "Options";
    private const string UiPrefix = "ui";
    private const string Ellipsis = "...";

    private Dictionary<StringName, Array<InputEvent>> _defaultActions;
    private VBoxContainer _content;
    private BtnInfo _btnNewInput; // The button currently waiting for new input

    public override void _Ready()
    {
        // Cache the content container used for dynamically adding rows.
        _content = GetNode<VBoxContainer>("Scroll/VBox");

        // Build the UI for all hotkeys from saved options.
        CreateHotkeys();
    }

    public override void _Input(InputEvent @event)
    {
        // If we are currently listening for a replacement binding, handle those inputs first.
        if (_btnNewInput != null)
        {
            HandleListeningInput(@event);
            return;
        }

        // Otherwise, handle global non-listening input (e.g. escape to go back).
        HandleNonListeningInput();
    }

    private void HandleListeningInput(InputEvent @event)
    {
        // If the user pressed the dedicated remove-hotkey action, remove the binding.
        if (Input.IsActionJustPressed(RemoveHotkeyAction))
        {
            HandleRemoveHotkey();
            return;
        }

        // If the user pressed UI cancel while listening, cancel listening mode.
        if (Input.IsActionJustPressed(InputActions.UICancel))
        {
            CancelListeningForInput();
            return;
        }

        // Only process input when a key is released or mouse button is released.
        if (@event is InputEventMouseButton mb && !mb.Pressed)
            ProcessCapturedInput(mb);
        else if (@event is InputEventKey { Echo: false, Pressed: false } key)
            ProcessCapturedInput(key);
    }

    private void HandleRemoveHotkey()
    {
        // Remove the currently selected input event from input map and options storage.
        StringName action = _btnNewInput.Action;

        InputMap.ActionEraseEvent(action, _btnNewInput.InputEvent);
        OptionsManager.GetHotkeys().Actions[action].Remove(_btnNewInput.InputEvent);

        // Remove the UI button representing that binding and stop listening.
        _btnNewInput.Btn.QueueFree();
        _btnNewInput = null;
    }

    private void CancelListeningForInput()
    {
        // Restore the original button text and enabled state.
        _btnNewInput.Btn.Text = _btnNewInput.OriginalText;
        _btnNewInput.Btn.Disabled = false;

        // If the listening was started from a plus-button, remove that placeholder.
        if (_btnNewInput.Plus)
            _btnNewInput.Btn.QueueFree();

        // Stop listening for input.
        _btnNewInput = null;
    }

    private void HandleNonListeningInput()
    {
        // Only act if the user pressed the UI cancel action.
        if (!Input.IsActionJustPressed(InputActions.UICancel))
            return;

        // If we are in the Options scene, going back should return to the main menu.
        if (SceneManager.GetCurrentScene().Name != OptionsSceneName)
            return;

        SceneManager.SwitchScene(Scene.MainMenu);
    }

    private void ProcessCapturedInput(InputEvent @event)
    {
        // Identify the action we are editing.
        StringName action = _btnNewInput.Action;

        // Prevent binding mouse buttons to the fullscreen toggle (intentional safeguard).
        if (action == FullscreenAction && @event is InputEventMouseButton)
            return;

        // Remember where the button was in the HBox so we can recreate it in the same spot.
        int index = _btnNewInput.Btn.GetIndex();

        // Recreate the UI button and position it back where the old one was.
        RecreateButtonAtIndex(action, @event, index);

        // Update both the options storage and the input map to reflect the new binding.
        UpdateOptionStorageAndInputMap(action, @event);

        // Done listening.
        _btnNewInput = null;
    }

    private void RecreateButtonAtIndex(StringName action, InputEvent @event, int index)
    {
        // Remove the old button from the UI (we recreate a fresh instance).
        _btnNewInput.Btn.QueueFree();

        // Create the new button representing this binding and enable it.
        Button btn = CreateButton(action, @event, _btnNewInput.HBox);
        btn.Disabled = false;

        // Move the new button to the original index so ordering remains unchanged.
        _btnNewInput.HBox.MoveChild(btn, index);
    }

    private void UpdateOptionStorageAndInputMap(StringName action, InputEvent @event)
    {
        // Load the dictionary of actions from the saved hotkeys options.
        Dictionary<StringName, Array<InputEvent>> actions = OptionsManager.GetHotkeys().Actions;

        // Remove the previous InputEvent entry for this action in the saved options.
        actions[action].Remove(_btnNewInput.InputEvent);

        // Add the new InputEvent to the saved options for this action.
        actions[action].Add(@event);

        // If the previous InputEvent existed, remove it from the engine input map.
        if (_btnNewInput.InputEvent != null)
            InputMap.ActionEraseEvent(action, _btnNewInput.InputEvent);

        // Add the new InputEvent to the input map.
        InputMap.ActionAddEvent(action, @event);
    }

    private HotkeyButton CreateButton(StringName action, InputEvent inputEvent, HBoxContainer hbox)
    {
        // Create a readable label for the input (e.g. "A" or "Mouse 1").
        string readable = GetReadableForInput(inputEvent);

        var btn = new HotkeyButton()
        {
            Text = readable
        };

        // Add the created button to the row's events container.
        hbox.AddChild(btn);

        // Build BtnInfo to describe this button and its associated metadata.
        BtnInfo info = new()
        {
            OriginalText = btn.Text,
            Action = action,
            HBox = hbox,
            Btn = btn,
            InputEvent = inputEvent,
            Plus = false
        };

        // Handle hotkey pressed events
        btn.Info = info;
        btn.HotkeyPressed += OnHotkeyButtonPressed;

        return btn;
    }

    private void CreateButtonPlus(StringName action, HBoxContainer hbox)
    {
        // Create a plus button used to add another binding for the same action.
        HotkeyButton btn = new() { Text = "+" };

        hbox.AddChild(btn);

        BtnInfo info = new()
        {
            OriginalText = btn.Text,
            Action = action,
            HBox = hbox,
            Btn = btn,
            Plus = true
        };

        btn.Info = info;
        btn.HotkeyPressed += OnPlusButtonPressed;
    }

    private void OnHotkeyButtonPressed(BtnInfo info)
    {
        // Ignore presses while already listening for another replacement.
        if (_btnNewInput != null)
            return;

        // Start listening for a new input to replace this binding.
        StartListening(info);
    }

    private void OnPlusButtonPressed(BtnInfo info)
    {
        // Ignore presses while already listening for another replacement.
        if (_btnNewInput != null)
            return;

        // Start listening and immediately create a new plus button for chaining.
        StartListening(info);
        CreateButtonPlus(info.Action, info.HBox);
    }

    private void StartListening(BtnInfo info)
    {
        // Store which button we are waiting input for and give visual feedback.
        _btnNewInput = info;
        _btnNewInput.Btn.Disabled = true;
        _btnNewInput.Btn.Text = Ellipsis;
    }

    private string GetReadableForInput(InputEvent inputEvent)
    {
        // Convert keyboard events to their human readable representation.
        if (inputEvent is InputEventKey key)
            return key.Readable();

        // Convert mouse button events to a simple "Mouse N" label.
        if (inputEvent is InputEventMouseButton mb)
            return $"Mouse {mb.ButtonIndex}";

        return string.Empty;
    }

    private void CreateHotkeys()
    {
        // Iterate actions sorted alphabetically so the UI is deterministic.
        foreach (StringName action in OptionsManager.GetHotkeys().Actions.Keys.OrderBy(x => x.ToString()))
        {
            string actionStr = action.ToString();

            // Skip the internal remove-hotkey action and engine UI actions.
            if (actionStr == RemoveHotkeyAction || actionStr.StartsWith(UiPrefix))
                continue;

            // Create the full row (label + event buttons + plus button) and add it to the content.
            HBoxContainer row = CreateActionRowFor(action);
            _content.AddChild(row);
        }
    }

    // Create an HBox row for a single action, populate its label and event buttons, and return it.
    private HBoxContainer CreateActionRowFor(StringName action)
    {
        HBoxContainer row = new();

        // Convert snake_case action name to human readable Title Case (e.g. move_left -> Move Left).
        string name = action.ToString().Replace('_', ' ').ToTitleCase();

        Label label = LabelFactory.Create(name);
        row.AddChild(label);

        // Align and size the label so layout matches other options screens.
        label.HorizontalAlignment = HorizontalAlignment.Left;
        label.CustomMinimumSize = new Vector2(200, 0);

        // Container for event buttons (the actual key/mouse bindings).
        HBoxContainer hboxEvents = new();

        // Populate hboxEvents with existing bindings for this action.
        AddEventButtonsForAction(action, hboxEvents);

        // Add the plus button used to add more bindings.
        CreateButtonPlus(action, hboxEvents);

        // Attach the events container to the row and return the fully constructed row.
        row.AddChild(hboxEvents);
        return row;
    }

    private void AddEventButtonsForAction(StringName action, HBoxContainer hboxEvents)
    {
        // Fetch the saved events for this action from the options.
        Array<InputEvent> events = OptionsManager.GetHotkeys().Actions[action];

        // Create a button for each keyboard and mouse binding.
        foreach (InputEvent @event in events)
        {
            if (@event is InputEventKey eventKey)
            {
                CreateButton(action, eventKey, hboxEvents);
            }

            if (@event is InputEventMouseButton eventMouseBtn)
            {
                CreateButton(action, eventMouseBtn, hboxEvents);
            }
        }
    }

    private void _OnResetToDefaultsPressed()
    {
        // Clear the currently generated UI rows.
        ClearContentChildren();

        _btnNewInput = null;

        // Reset saved hotkeys to defaults and rebuild the UI.
        OptionsManager.ResetHotkeys();
        CreateHotkeys();
    }

    private void ClearContentChildren()
    {
        for (int i = 0; i < _content.GetChildren().Count; i++)
        {
            _content.GetChild(i).QueueFree();
        }
    }

    private partial class HotkeyButton : Button
    {
        public event Action<BtnInfo> HotkeyPressed;

        public BtnInfo Info { get; set; }

        public HotkeyButton()
        {
            // Use Godot's Pressed event internally to raise a C# event for external code.
            Pressed += OnPressedLocal;
        }

        private void OnPressedLocal()
        {
            // Invoke the C# event with the stored BtnInfo payload.
            HotkeyPressed?.Invoke(Info);
        }
    }

    private class BtnInfo
    {
        public required string        OriginalText { get; init; }
        public required StringName    Action       { get; init; }
        public required HBoxContainer HBox         { get; init; }
        public required Button        Btn          { get; init; }
        public InputEvent             InputEvent   { get; init; }
        public bool                   Plus         { get; init; }
    }
}
