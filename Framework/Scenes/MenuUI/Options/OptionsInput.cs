using Godot;
using GodotUtils;
using System.Linq;
using Godot.Collections;
using GodotUtils.UI;

namespace __TEMPLATE__.UI;

public partial class OptionsInput : Control
{
    private Dictionary<StringName, Array<InputEvent>> _defaultActions;
    private BtnInfo _btnNewInput; // the btn waiting for new input
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
                OptionsManager.GetHotkeys().Actions[action].Remove(_btnNewInput.InputEvent);

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
                if (SceneManager.GetCurrentScene().Name == "Options")
                {
                    if (_btnNewInput == null)
                    {
                        SceneManager.SwitchScene(Scene.MainMenu);
                    }
                }
            }
        }
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
        Button btn = CreateButton(action, @event, _btnNewInput.HBox);
        btn.Disabled = false;

        // Move the button to where it was originally at
        _btnNewInput.HBox.MoveChild(btn, index);

        Dictionary<StringName, Array<InputEvent>> actions = OptionsManager.GetHotkeys().Actions;

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

    private Button CreateButton(string action, InputEvent inputEvent, HBoxContainer hbox)
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
        Button btn = new()
        {
            Text = readable
        };

        hbox.AddChild(btn);

        btn.Pressed += () =>
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
                Btn = btn,
                HBox = hbox,
                InputEvent = inputEvent,
                OriginalText = btn.Text
            };

            // Give feedback to the user saying we are waiting for new input
            btn.Disabled = true;
            btn.Text = "...";
        };

        return btn;
    }

    private void CreateButtonPlus(string action, HBoxContainer hbox)
    {
        // Create the button
        Button btn = new()
        {
            Text = "+"
        };

        hbox.AddChild(btn);

        btn.Pressed += () =>
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
                Btn = btn,
                HBox = hbox,
                OriginalText = btn.Text,
                Plus = true
            };

            // Give feedback to the user saying we are waiting for new input
            btn.Disabled = true;
            btn.Text = "...";

            CreateButtonPlus(action, hbox);
        };
    }

    private void CreateHotkeys()
    {
        // Loop through the actions in alphabetical order
        foreach (StringName action in OptionsManager.GetHotkeys().Actions.Keys.OrderBy(x => x.ToString()))
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
            Label label = LabelFactory.Create(name);
            hbox.AddChild(label);

            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.CustomMinimumSize = new Vector2(200, 0);

            // Add all the events after the action label
            HBoxContainer hboxEvents = new();

            Array<InputEvent> events = OptionsManager.GetHotkeys().Actions[action];

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
    }
}

public class BtnInfo
{
    public required string        OriginalText { get; init; }
    public required StringName    Action       { get; init; }
    public required HBoxContainer HBox         { get; init; }
    public required Button        Btn          { get; init; }
    public InputEvent             InputEvent   { get; init; }
    public bool                   Plus         { get; init; }
}
