using Godot;

namespace __TEMPLATE__.Ui;

public partial class OptionsInput
{
    /// <summary>
    /// Handles hotkey edit/listen lifecycle and applies captured inputs.
    /// </summary>
    public sealed class HotkeyEditor
    {
        private readonly HotkeyStore _store;
        private readonly HotkeyListView _view;
        private readonly string _removeHotkeyAction;
        private readonly string _fullscreenAction;
        private readonly FocusOutlineManager _focusOutline;

        private HotkeyButtonInfo? _current;
        private bool _listeningOnPlus;
        private bool _actionSuppressed;
        private StringName _suppressedAction = null!;

        /// <summary>
        /// Initializes hotkey editor dependencies.
        /// </summary>
        /// <param name="store">Hotkey storage backend.</param>
        /// <param name="view">Hotkey list view adapter.</param>
        /// <param name="removeHotkeyAction">Action name used to remove bindings.</param>
        /// <param name="fullscreenAction">Action name restricted from mouse binding.</param>
        /// <param name="focusOutline">Focus outline manager.</param>
        public HotkeyEditor(
            HotkeyStore store,
            HotkeyListView view,
            string removeHotkeyAction,
            string fullscreenAction,
            FocusOutlineManager focusOutline)
        {
            _store = store;
            _view = view;
            _removeHotkeyAction = removeHotkeyAction;
            _fullscreenAction = fullscreenAction;
            _focusOutline = focusOutline;
        }

        /// <summary>
        /// Gets whether editor is currently listening for new input.
        /// </summary>
        public bool IsListening => _current != null;

        /// <summary>
        /// Starts listening mode for a selected hotkey button.
        /// </summary>
        /// <param name="info">Selected hotkey button metadata.</param>
        /// <param name="fromPlus">Whether listening started from plus button.</param>
        public void StartListening(HotkeyButtonInfo info, bool fromPlus)
        {
            _current = info;
            _listeningOnPlus = fromPlus;
            _actionSuppressed = true;
            _suppressedAction = info.Action;

            _view.ShowListening(info);
            HotkeyStore.SuppressAction(info.Action);
        }

        /// <summary>
        /// Processes input while listening and applies/remove/cancel operations.
        /// </summary>
        /// <param name="event">Incoming input event.</param>
        public void HandleInput(InputEvent @event)
        {
            // Ignore input when editor is not in listening mode.
            if (_current == null)
                return;

            // Remove existing binding when remove action is pressed for non-plus edits.
            if (Input.IsActionJustPressed(_removeHotkeyAction) && !_listeningOnPlus)
            {
                RemoveCurrentHotkey();
                return;
            }

            // Cancel listening and restore prior state on cancel action.
            if (Input.IsActionJustPressed(InputActions.UICancel))
            {
                CancelListening();
                return;
            }

            // Apply captured key or mouse release events as new bindings.
            if (ShouldCaptureInput(@event))
                ApplyNewInput(@event);
        }

        /// <summary>
        /// Clears listening state and restores suppressed action sync.
        /// </summary>
        public void Clear()
        {
            // Restore runtime InputMap bindings after temporary suppression.
            if (_actionSuppressed)
            {
                _store.SyncAction(_suppressedAction);
                _actionSuppressed = false;
            }

            _current = null;
            _listeningOnPlus = false;
        }

        /// <summary>
        /// Removes the currently edited hotkey binding.
        /// </summary>
        private void RemoveCurrentHotkey()
        {
            StringName action = _current!.Action;

            _store.RemoveEvent(action, _current!.InputEvent);
            HotkeyListView.RemoveButton(_current!);
            _view.FocusPlusButton(action);

            Clear();
        }

        /// <summary>
        /// Cancels listening and restores previous UI/binding state.
        /// </summary>
        private void CancelListening()
        {
            // Remove placeholder plus button when canceling add-binding mode.
            if (_current!.IsPlus)
                HotkeyListView.RemoveButton(_current!);
            else
                HotkeyListView.RestoreListening(_current!);

            Clear();
        }

        /// <summary>
        /// Applies a newly captured input event to the current action.
        /// </summary>
        /// <param name="event">Captured input event.</param>
        private void ApplyNewInput(InputEvent @event)
        {
            StringName action = _current!.Action;

            // Prevent mouse-button bindings for fullscreen action.
            if (action == _fullscreenAction && @event is InputEventMouseButton)
                return;

            _focusOutline.ClearFocus();

            InputEvent persistentEvent = (InputEvent)@event.Duplicate();

            // Reject bindings that duplicate an existing event for this action.
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

        /// <summary>
        /// Handles duplicate binding attempts by restoring UI and focus.
        /// </summary>
        /// <param name="action">Action currently being edited.</param>
        private void HandleDuplicate(StringName action)
        {
            // Remove temporary plus button when duplicate was captured in add mode.
            if (_current!.IsPlus)
                HotkeyListView.RemoveButton(_current!);
            else
                HotkeyListView.RestoreListening(_current!);

            _view.FocusPlusButton(action);
            Clear();
        }

        /// <summary>
        /// Determines whether an input event should be captured as a binding.
        /// </summary>
        /// <param name="event">Input event to evaluate.</param>
        /// <returns><see langword="true"/> when event represents a key/mouse release.</returns>
        private static bool ShouldCaptureInput(InputEvent @event)
        {
            // Capture mouse button release events as bindings.
            if (@event is InputEventMouseButton mb && !mb.Pressed)
                return true;

            return @event is InputEventKey { Echo: false, Pressed: false };
        }
    }
}
