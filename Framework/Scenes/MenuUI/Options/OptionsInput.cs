using Godot;
using GodotUtils;
using System;
using System.Linq;
using Godot.Collections;

namespace __TEMPLATE__.UI;

public partial class OptionsInput : Control
{
    private static BtnInfo _btnNewInput; // the btn waiting for new input
    private Dictionary<StringName, Array<InputEvent>> _defaultActions;
    private VBoxContainer _content;

    public override void _Ready()
    {
        _content = GetNode<VBoxContainer>("Scroll/VBox");
        CreateHotkeys();
    }

    public override void _Input(InputEvent @event)
    {
        if (_btnNewInput != null)
        {
            if (Input.IsActionJustPressed(InputActions.RemoveHotkey))
            {
                StringName action = _btnNewInput.Action;

                // Update input map
                InputMap.ActionEraseEvent(action, _btnNewInput.InputEvent);

                // Update options
                OptionsManager.Hotkeys.Actions[action].Remove(_btnNewInput.InputEvent);

                // Update UI
                _btnNewInput.Btn.QueueFree();
                _btnNewInput = null;
            }

            if (Input.IsActionJustPressed(InputActions.UICancel))
            {
                _btnNewInput.Btn.Text = _btnNewInput.OriginalText;
                _btnNewInput.Btn.Disabled = false;

                if (_btnNewInput.Plus)
                {
                    _btnNewInput.Btn.QueueFree();
                }

                _btnNewInput = null;
                @event.Dispose(); // Object count was increasing a lot when this function was executed
                return;
            }

            switch (@event)
            {
                case InputEventMouseButton eventMouseBtn:
                {
                    if (!eventMouseBtn.Pressed)
                        HandleInput(eventMouseBtn);
                    
                    break;
                }
                case InputEventKey { Echo: false, Pressed: false } eventKey: // Only check when the last key was released from the keyboard
                    HandleInput(eventKey);
                    break;
            }   
        }
        else
        {
            if (Input.IsActionJustPressed(InputActions.UICancel))
            {
                if (SceneManager.CurrentScene.Name == "Options")
                {
                    if (_btnNewInput == null)
                    {
                        Game.SwitchScene(Scene.MainMenu);
                    }
                }
            }
        }

        @event.Dispose(); // Object count was increasing a lot when this function was executed
    }

    private void HandleInput(InputEvent @event)
    {
        StringName action = _btnNewInput.Action;

        // Prevent something very evil from happening!
        if (action == "fullscreen" && @event is InputEventMouseButton)
            return;

        // Re-create the button

        // Preserve the index the button was originally at
        int index = _btnNewInput.Btn.GetIndex();

        // Destroy the button
        _btnNewInput.Btn.QueueFree();

        // Create the button
        GButton btn = CreateButton(action, @event, _btnNewInput.HBox);
        btn.Internal.Disabled = false;

        // Move the button to where it was originally at
        _btnNewInput.HBox.MoveChild(btn.Internal, index);

        Dictionary<StringName, Array<InputEvent>> actions = OptionsManager.Hotkeys.Actions;

        // Clear the specific action event
        actions[action].Remove(_btnNewInput.InputEvent);

        // Update the specific action event
        actions[action].Add(@event);

        // Update input map
        if (_btnNewInput.InputEvent != null)
        {
            InputMap.ActionEraseEvent(action, _btnNewInput.InputEvent);
        }

        InputMap.ActionAddEvent(action, @event);

        // No longer waiting for new input
        _btnNewInput = null;
    }

    private static GButton CreateButton(string action, InputEvent inputEvent, HBoxContainer hbox)
    {
        string readable = "";

        if (inputEvent is InputEventKey key)
        {
            readable = key.Readable();
        }
        else if (inputEvent is InputEventMouseButton button)
        {
            readable = $"Mouse {button.ButtonIndex}";
        }

        // Create the button
        GButton btn = new(hbox, readable);
        btn.Internal.Pressed += () =>
        {
            // Do not do anything if listening for new input
            if (_btnNewInput != null)
            {
                return;
            }

            // Listening for new hotkey to replace old with...
            _btnNewInput = new BtnInfo
            {
                Action = action,
                Btn = btn.Internal,
                HBox = hbox,
                InputEvent = inputEvent,
                OriginalText = btn.Internal.Text
            };

            // Give feedback to the user saying we are waiting for new input
            btn.Internal.Disabled = true;
            btn.Internal.Text = "...";
        };

        return btn;
    }

    private static void CreateButtonPlus(string action, HBoxContainer hbox)
    {
        // Create the button
        GButton btn = new(hbox, "+");
        btn.Internal.Pressed += () =>
        {
            // Do not do anything if listening for new input
            if (_btnNewInput != null)
            {
                return;
            }

            // Listening for new hotkey to replace old with...
            _btnNewInput = new BtnInfo
            {
                Action = action,
                Btn = btn.Internal,
                HBox = hbox,
                OriginalText = btn.Internal.Text,
                Plus = true
            };

            // Give feedback to the user saying we are waiting for new input
            btn.Internal.Disabled = true;
            btn.Internal.Text = "...";

            CreateButtonPlus(action, hbox);
        };
    }

    private void CreateHotkeys()
    {
        // Loop through the actions in alphabetical order
        foreach (StringName action in OptionsManager.Hotkeys.Actions.Keys.OrderBy(x => x.ToString()))
        {
            string actionStr = action.ToString();

            // Exclude "remove_hotkey" action and all built-in actions
            if (actionStr == "remove_hotkey" || actionStr.StartsWith("ui"))
            {
                continue;
            }

            HBoxContainer hbox = new();

            // For example convert move_left to Move Left
            string name = action.ToString().Replace('_', ' ').ToTitleCase();

            // Add the action label
            GLabel label = new(hbox, name);

            label.Internal.HorizontalAlignment = HorizontalAlignment.Left;
            label.Internal.CustomMinimumSize = new Vector2(200, 0);

            // Add all the events after the action label
            HBoxContainer hboxEvents = new();

            Array<InputEvent> events = OptionsManager.Hotkeys.Actions[action];

            foreach (InputEvent @event in events)
            {
                // Handle keys
                if (@event is InputEventKey eventKey)
                {
                    CreateButton(action, eventKey, hboxEvents);
                }

                // Handle mouse buttons
                if (@event is InputEventMouseButton eventMouseBtn)
                {
                    CreateButton(action, eventMouseBtn, hboxEvents);
                }
            }

            CreateButtonPlus(action, hboxEvents);

            hbox.AddChild(hboxEvents);
            _content.AddChild(hbox);
        }
    }

    private void _OnResetToDefaultsPressed()
    {
        for (int i = 0; i < _content.GetChildren().Count; i++)
        {
            if (_content.GetChild(i) != this)
            {
                _content.GetChild(i).QueueFree();
            }
        }

        _btnNewInput = null;
        OptionsManager.ResetHotkeys();
        CreateHotkeys();
        GC.Collect(); // Object count was increasing a lot when this function was executed
    }
}

public class BtnInfo
{
    public InputEvent InputEvent { get; init; }
    public string OriginalText { get; init; }
    public StringName Action { get; init; }
    public HBoxContainer HBox { get; init; }
    public Button Btn { get; init; }
    public bool Plus { get; init; }
}

