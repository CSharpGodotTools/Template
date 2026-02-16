using Godot;
using Godot.Collections;
using GodotUtils;
using System;
using System.Linq;

namespace Framework.UI;

public partial class OptionsInput : IDisposable
{
    // Constants
    private const string OptionsSceneName = "Options";
    private const string UiPrefix = "ui";
    private const string Ellipsis = "...";

    // Fields
    private readonly Button _resetInputToDefaultsBtn;
    private readonly SceneManager _scene;
    private readonly HotkeyStore _store;
    private readonly HotkeyListView _view;
    private readonly HotkeyEditor _editor;

    public OptionsInput(Options options, Button inputNavBtn)
    {
        _scene = GameFramework.Scene;

        VBoxContainer content = options.GetNode<VBoxContainer>("%InputContent");

        _store = new HotkeyStore(GameFramework.Options);
        _view = new HotkeyListView(content, inputNavBtn, _store, InputActions.RemoveHotkey, UiPrefix, Ellipsis);
        _view.HotkeyPressed += OnHotkeyButtonPressed;
        _view.PlusPressed += OnPlusButtonPressed;
        _view.Build();

        _editor = new HotkeyEditor(_store, _view, InputActions.RemoveHotkey, InputActions.Fullscreen);

        _resetInputToDefaultsBtn = options.GetNode<Button>("%ResetInputToDefaults");
        _resetInputToDefaultsBtn.Pressed += OnResetToDefaultsPressed;
    }

    public void Dispose()
    {
        _resetInputToDefaultsBtn.Pressed -= OnResetToDefaultsPressed;
        _view.HotkeyPressed -= OnHotkeyButtonPressed;
        _view.PlusPressed -= OnPlusButtonPressed;
        _editor.Clear();
        GC.SuppressFinalize(this);
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

    private sealed class HotkeyEditor
    {
        private readonly HotkeyStore _store;
        private readonly HotkeyListView _view;
        private readonly string _removeHotkeyAction;
        private readonly string _fullscreenAction;

        private HotkeyButtonInfo _current;
        private bool _listeningOnPlus;
        private bool _actionSuppressed;
        private StringName _suppressedAction;

        public HotkeyEditor(HotkeyStore store, HotkeyListView view, string removeHotkeyAction, string fullscreenAction)
        {
            _store = store;
            _view = view;
            _removeHotkeyAction = removeHotkeyAction;
            _fullscreenAction = fullscreenAction;
        }

        public bool IsListening => _current != null;

        public void StartListening(HotkeyButtonInfo info, bool fromPlus)
        {
            _current = info;
            _listeningOnPlus = fromPlus;
            _actionSuppressed = true;
            _suppressedAction = info.Action;

            _view.ShowListening(info);
            HotkeyStore.SuppressAction(info.Action);
        }

        public void HandleInput(InputEvent @event)
        {
            if (_current == null)
                return;

            if (Input.IsActionJustPressed(_removeHotkeyAction) && !_listeningOnPlus)
            {
                RemoveCurrentHotkey();
                return;
            }

            if (Input.IsActionJustPressed(InputActions.UICancel))
            {
                CancelListening();
                return;
            }

            if (ShouldCaptureInput(@event))
                ApplyNewInput(@event);
        }

        public void Clear()
        {
            if (_actionSuppressed)
            {
                _store.SyncAction(_suppressedAction);
                _actionSuppressed = false;
            }

            _current = null;
            _listeningOnPlus = false;
        }

        private void RemoveCurrentHotkey()
        {
            StringName action = _current.Action;

            _store.RemoveEvent(action, _current.InputEvent);
            HotkeyListView.RemoveButton(_current);
            _view.FocusPlusButton(action);

            Clear();
        }

        private void CancelListening()
        {
            if (_current.IsPlus)
                HotkeyListView.RemoveButton(_current);
            else
                HotkeyListView.RestoreListening(_current);

            Clear();
        }

        private void ApplyNewInput(InputEvent @event)
        {
            StringName action = _current.Action;

            if (action == _fullscreenAction && @event is InputEventMouseButton)
                return;

            GameFramework.FocusOutline.ClearFocus();

            InputEvent persistentEvent = (InputEvent)@event.Duplicate();

            if (_store.HasDuplicate(action, persistentEvent))
            {
                HandleDuplicate(action);
                return;
            }

            _view.ReplaceButton(_current, persistentEvent);
            _store.ReplaceEvent(action, _current.InputEvent, persistentEvent);
            _view.FocusPlusButton(action);

            Clear();
        }

        private void HandleDuplicate(StringName action)
        {
            if (_current.IsPlus)
                HotkeyListView.RemoveButton(_current);
            else
                HotkeyListView.RestoreListening(_current);

            _view.FocusPlusButton(action);
            Clear();
        }

        private static bool ShouldCaptureInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && !mb.Pressed)
                return true;

            return @event is InputEventKey { Echo: false, Pressed: false };
        }
    }

    private sealed class HotkeyStore
    {
        private readonly OptionsManager _options;

        public HotkeyStore(OptionsManager options)
        {
            _options = options;
        }

        public Dictionary<StringName, Array<InputEvent>> Actions => _options.GetHotkeys().Actions;

        public Array<InputEvent> GetEvents(StringName action)
        {
            return Actions[action];
        }

        public System.Collections.Generic.IEnumerable<StringName> GetOrderedActions()
        {
            return Actions.Keys.OrderBy(x => x.ToString());
        }

        public void RemoveEvent(StringName action, InputEvent @event)
        {
            if (@event == null)
                return;

            Actions[action].Remove(@event);
        }

        public void ReplaceEvent(StringName action, InputEvent oldEvent, InputEvent newEvent)
        {
            Actions[action].Remove(oldEvent);
            Actions[action].Add(newEvent);
        }

        public bool HasDuplicate(StringName action, InputEvent candidate)
        {
            Array<InputEvent> events = Actions[action];
            for (int i = 0; i < events.Count; i++)
            {
                if (EventsMatch(events[i], candidate))
                    return true;
            }

            return false;
        }

        public void ResetToDefaults()
        {
            _options.ResetHotkeys();
        }

        public static void SuppressAction(StringName action)
        {
            Array<InputEvent> events = InputMap.ActionGetEvents(action);
            for (int i = 0; i < events.Count; i++)
            {
                InputMap.ActionEraseEvent(action, events[i]);
            }
        }

        public void SyncAction(StringName action)
        {
            Array<InputEvent> existing = InputMap.ActionGetEvents(action);
            for (int i = 0; i < existing.Count; i++)
            {
                InputMap.ActionEraseEvent(action, existing[i]);
            }

            Array<InputEvent> stored = Actions[action];
            for (int i = 0; i < stored.Count; i++)
            {
                InputMap.ActionAddEvent(action, stored[i]);
            }
        }

        private static bool EventsMatch(InputEvent left, InputEvent right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            if (left is InputEventKey leftKey && right is InputEventKey rightKey)
            {
                if (!ModifiersMatch(leftKey, rightKey))
                    return false;

                return KeyMatch(leftKey.Keycode, rightKey.Keycode)
                    || KeyMatch(leftKey.PhysicalKeycode, rightKey.PhysicalKeycode)
                    || KeyMatch(leftKey.Keycode, rightKey.PhysicalKeycode)
                    || KeyMatch(leftKey.PhysicalKeycode, rightKey.Keycode);
            }

            if (left is InputEventMouseButton leftMouse && right is InputEventMouseButton rightMouse)
            {
                return leftMouse.ButtonIndex == rightMouse.ButtonIndex
                    && ModifiersMatch(leftMouse, rightMouse);
            }

            return false;
        }

        private static bool ModifiersMatch(InputEventWithModifiers left, InputEventWithModifiers right)
        {
            return left.ShiftPressed == right.ShiftPressed
                && left.AltPressed == right.AltPressed
                && left.CtrlPressed == right.CtrlPressed
                && left.MetaPressed == right.MetaPressed;
        }

        private static bool KeyMatch(Key left, Key right)
        {
            int leftValue = (int)left;
            int rightValue = (int)right;

            return leftValue != 0 && rightValue != 0 && leftValue == rightValue;
        }
    }

    private sealed class HotkeyListView
    {
        private readonly VBoxContainer _content;
        private readonly Button _inputNavBtn;
        private readonly HotkeyStore _store;
        private readonly string _removeHotkeyAction;
        private readonly string _uiPrefix;
        private readonly string _ellipsis;

        private readonly System.Collections.Generic.Dictionary<StringName, HotkeyRow> _rows = [];

        public event Action<HotkeyButtonInfo> HotkeyPressed;
        public event Action<HotkeyButtonInfo> PlusPressed;

        public HotkeyListView(
            VBoxContainer content,
            Button inputNavBtn,
            HotkeyStore store,
            string removeHotkeyAction,
            string uiPrefix,
            string ellipsis)
        {
            _content = content;
            _inputNavBtn = inputNavBtn;
            _store = store;
            _removeHotkeyAction = removeHotkeyAction;
            _uiPrefix = uiPrefix;
            _ellipsis = ellipsis;
        }

        public void Build()
        {
            foreach (StringName action in _store.GetOrderedActions())
            {
                if (!ShouldDisplayAction(action))
                    continue;

                HotkeyRow row = new(action, _inputNavBtn, GetDisplayName(action), HandleHotkeyPressed, HandlePlusPressed);
                _rows.Add(action, row);

                _content.AddChild(row.RowRoot);

                row.AddBindings(_store.GetEvents(action));
                row.AddPlusButton();
            }
        }

        public void Clear()
        {
            var children = _content.GetChildren();
            for (int i = 0; i < children.Count; i++)
            {
                _content.GetChild(i).QueueFree();
            }

            _rows.Clear();
        }

        public void ShowListening(HotkeyButtonInfo info)
        {
            info.Button.Text = _ellipsis;
            info.Button.Disabled = true;
        }

        public static void RestoreListening(HotkeyButtonInfo info)
        {
            info.Button.Text = info.OriginalText;
            info.Button.Disabled = false;
        }

        public static void RemoveButton(HotkeyButtonInfo info)
        {
            info.Button.QueueFree();
        }

        public void ReplaceButton(HotkeyButtonInfo info, InputEvent newEvent)
        {
            if (!_rows.TryGetValue(info.Action, out HotkeyRow row))
                return;

            row.ReplaceButton(info, newEvent);
        }

        public void AddPlusButton(StringName action)
        {
            if (_rows.TryGetValue(action, out HotkeyRow row))
                row.AddPlusButton();
        }

        public void FocusPlusButton(StringName action)
        {
            if (_rows.TryGetValue(action, out HotkeyRow row))
                row.FocusPlusButton();
        }

        private void HandleHotkeyPressed(HotkeyButtonInfo info)
        {
            HotkeyPressed?.Invoke(info);
        }

        private void HandlePlusPressed(HotkeyButtonInfo info)
        {
            PlusPressed?.Invoke(info);
        }

        private bool ShouldDisplayAction(StringName action)
        {
            string actionStr = action.ToString();
            return actionStr != _removeHotkeyAction && !actionStr.StartsWith(_uiPrefix);
        }

        private static string GetDisplayName(StringName action)
        {
            return action.ToString().Replace('_', ' ').ToTitleCase();
        }
    }

    private sealed class HotkeyRow
    {
        private readonly StringName _action;
        private readonly Button _inputNavBtn;
        private readonly Action<HotkeyButtonInfo> _onHotkeyPressed;
        private readonly Action<HotkeyButtonInfo> _onPlusPressed;

        private readonly HBoxContainer _rowRoot;
        private readonly HBoxContainer _events;

        public HotkeyRow(
            StringName action,
            Button inputNavBtn,
            string displayName,
            Action<HotkeyButtonInfo> onHotkeyPressed,
            Action<HotkeyButtonInfo> onPlusPressed)
        {
            _action = action;
            _inputNavBtn = inputNavBtn;
            _onHotkeyPressed = onHotkeyPressed;
            _onPlusPressed = onPlusPressed;

            _rowRoot = new HBoxContainer();

            Label label = LabelFactory.Create(displayName);
            _rowRoot.AddChild(label);

            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.CustomMinimumSize = new Vector2(200, 0);

            _events = new HBoxContainer();
            _rowRoot.AddChild(_events);
        }

        public HBoxContainer RowRoot => _rowRoot;

        public void AddBindings(Array<InputEvent> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                bool isFirst = i == 0;

                InputEvent @event = events[i];
                if (@event is InputEventKey || @event is InputEventMouseButton)
                    CreateBindingButton(@event, isFirst);
            }
        }

        public void AddPlusButton()
        {
            HotkeyButton btn = new() { Text = "+" };

            _events.AddChild(btn);

            HotkeyButtonInfo info = new()
            {
                OriginalText = btn.Text,
                Action = _action,
                Row = this,
                Button = btn,
                InputEvent = null,
                IsPlus = true
            };

            AttachHandlers(btn, info, _onPlusPressed);
        }

        public void ReplaceButton(HotkeyButtonInfo info, InputEvent newEvent)
        {
            bool wasFirst = info.Button.FocusNeighborLeft != null;
            int index = info.Button.GetIndex();

            info.Button.QueueFree();

            HotkeyButton btn = CreateBindingButton(newEvent, wasFirst);
            _events.MoveChild(btn, index);
        }

        public void FocusPlusButton()
        {
            if (_events.GetChildCount() == 0)
                return;

            Button plusBtn = _events.GetChild<Button>(_events.GetChildCount() - 1);
            GameFramework.FocusOutline.Focus(plusBtn);
        }

        private HotkeyButton CreateBindingButton(InputEvent inputEvent, bool isFirst)
        {
            string readable = GetReadableForInput(inputEvent);

            HotkeyButton btn = new()
            {
                Text = readable
            };

            if (isFirst)
                btn.FocusNeighborLeft = _inputNavBtn.GetPath();

            _events.AddChild(btn);

            HotkeyButtonInfo info = new()
            {
                OriginalText = btn.Text,
                Action = _action,
                Row = this,
                Button = btn,
                InputEvent = inputEvent,
                IsPlus = false
            };

            AttachHandlers(btn, info, _onHotkeyPressed);

            return btn;
        }

        private static void AttachHandlers(HotkeyButton btn, HotkeyButtonInfo info, Action<HotkeyButtonInfo> handler)
        {
            btn.Info = info;
            btn.HotkeyPressed += handler;
            btn.TreeExited += ExitTree;

            void ExitTree()
            {
                btn.HotkeyPressed -= handler;
                btn.TreeExited -= ExitTree;
            }
        }
    }

    private partial class HotkeyButton : Button
    {
        public event Action<HotkeyButtonInfo> HotkeyPressed;

        public HotkeyButtonInfo Info { get; set; }

        public override void _Ready()
        {
            Pressed += OnPressedLocal;
            TreeExited += OnTreeExited;
        }

        private void OnTreeExited()
        {
            Pressed -= OnPressedLocal;
            TreeExited -= OnTreeExited;
        }

        private void OnPressedLocal()
        {
            HotkeyPressed?.Invoke(Info);
        }
    }

    private sealed class HotkeyButtonInfo
    {
        public required string OriginalText { get; init; }
        public required StringName Action { get; init; }
        public required HotkeyRow Row { get; init; }
        public required Button Button { get; init; }
        public InputEvent InputEvent { get; init; }
        public bool IsPlus { get; init; }
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
