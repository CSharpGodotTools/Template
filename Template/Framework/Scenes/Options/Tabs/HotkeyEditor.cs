using Godot;

namespace __TEMPLATE__.Ui;

public partial class OptionsInput
{
    public sealed class HotkeyEditor
    {
        private readonly HotkeyStore _store;
        private readonly HotkeyListView _view;
        private readonly string _removeHotkeyAction;
        private readonly string _fullscreenAction;

        private HotkeyButtonInfo? _current;
        private bool _listeningOnPlus;
        private bool _actionSuppressed;
        private StringName _suppressedAction = null!;

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
            StringName action = _current!.Action;

            _store.RemoveEvent(action, _current!.InputEvent);
            HotkeyListView.RemoveButton(_current!);
            _view.FocusPlusButton(action);

            Clear();
        }

        private void CancelListening()
        {
            if (_current!.IsPlus)
                HotkeyListView.RemoveButton(_current!);
            else
                HotkeyListView.RestoreListening(_current!);

            Clear();
        }

        private void ApplyNewInput(InputEvent @event)
        {
            StringName action = _current!.Action;

            if (action == _fullscreenAction && @event is InputEventMouseButton)
                return;

            Game.FocusOutline.ClearFocus();

            InputEvent persistentEvent = (InputEvent)@event.Duplicate();

            if (_store.HasDuplicate(action, persistentEvent))
            {
                HandleDuplicate(action);
                return;
            }

            _view.ReplaceButton(_current!, persistentEvent);
            _store.ReplaceEvent(action, _current!.InputEvent, persistentEvent);
            _view.FocusPlusButton(action);

            Clear();
        }

        private void HandleDuplicate(StringName action)
        {
            if (_current!.IsPlus)
                HotkeyListView.RemoveButton(_current!);
            else
                HotkeyListView.RestoreListening(_current!);

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
}


