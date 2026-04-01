using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

public partial class OptionsInput
{
    /// <summary>
    /// Builds and updates the visual list of input-action hotkey rows.
    /// </summary>
    public sealed class HotkeyListView
    {
        private readonly VBoxContainer _content;
        private readonly Button _inputNavBtn;
        private readonly HotkeyStore _store;
        private readonly string _removeHotkeyAction;
        private readonly string _uiPrefix;
        private readonly string _ellipsis;
        private readonly FocusOutlineManager _focusOutline;

        private readonly Dictionary<StringName, HotkeyRow> _rows = [];

        /// <summary>
        /// Raised when an existing hotkey binding button is pressed.
        /// </summary>
        public event Action<HotkeyButtonInfo> HotkeyPressed = null!;

        /// <summary>
        /// Raised when the add-binding (+) button is pressed.
        /// </summary>
        public event Action<HotkeyButtonInfo> PlusPressed = null!;

        /// <summary>
        /// Creates a view controller for the hotkey list in the options input tab.
        /// </summary>
        /// <param name="content">Container that receives generated row controls.</param>
        /// <param name="inputNavBtn">Navigation anchor used for keyboard focus wiring.</param>
        /// <param name="store">Source of action names and bound input events.</param>
        /// <param name="removeHotkeyAction">Special action name reserved for removal behavior.</param>
        /// <param name="uiPrefix">Prefix used by internal UI actions that should stay hidden.</param>
        /// <param name="ellipsis">Text displayed while waiting for a replacement binding.</param>
        /// <param name="focusOutline">Focus helper used when restoring selection.</param>
        public HotkeyListView(
            VBoxContainer content,
            Button inputNavBtn,
            HotkeyStore store,
            string removeHotkeyAction,
            string uiPrefix,
            string ellipsis,
            FocusOutlineManager focusOutline)
        {
            _content = content;
            _inputNavBtn = inputNavBtn;
            _store = store;
            _removeHotkeyAction = removeHotkeyAction;
            _uiPrefix = uiPrefix;
            _ellipsis = ellipsis;
            _focusOutline = focusOutline;
        }

        /// <summary>
        /// Rebuilds every visible action row from the current hotkey store.
        /// </summary>
        public void Build()
        {
            foreach (StringName action in _store.GetOrderedActions())
            {
                // Skip internal actions that should not appear in the user-facing list.
                if (!ShouldDisplayAction(action))
                    continue;

                HotkeyRow row = new(action, _inputNavBtn, GetDisplayName(action), HandleHotkeyPressed, HandlePlusPressed, _focusOutline);
                _rows.Add(action, row);

                _content.AddChild(row.RowRoot);

                row.AddBindings(_store.GetEvents(action));
                row.AddPlusButton();
            }
        }

        /// <summary>
        /// Removes all generated rows from the scene tree and clears internal row tracking.
        /// </summary>
        public void Clear()
        {
            Godot.Collections.Array<Node> children = _content.GetChildren();
            for (int i = 0; i < children.Count; i++)
            {
                _content.GetChild(i).QueueFree();
            }

            _rows.Clear();
        }

        /// <summary>
        /// Marks a button as actively listening for new input.
        /// </summary>
        /// <param name="info">Button metadata for the row being edited.</param>
        public void ShowListening(HotkeyButtonInfo info)
        {
            info.Button.Text = _ellipsis;
            info.Button.Disabled = true;
        }

        /// <summary>
        /// Restores a listening button to its original label and enabled state.
        /// </summary>
        /// <param name="info">Button metadata for the row being edited.</param>
        public static void RestoreListening(HotkeyButtonInfo info)
        {
            info.Button.Text = info.OriginalText;
            info.Button.Disabled = false;
        }

        /// <summary>
        /// Removes a hotkey button from the UI after a binding was deleted.
        /// </summary>
        /// <param name="info">Button metadata for the removed binding.</param>
        public static void RemoveButton(HotkeyButtonInfo info)
        {
            info.Button.QueueFree();
        }

        /// <summary>
        /// Replaces a binding button with one that represents a newly assigned input event.
        /// </summary>
        /// <param name="info">Metadata for the button being replaced.</param>
        /// <param name="newEvent">New input event to display and bind.</param>
        public void ReplaceButton(HotkeyButtonInfo info, InputEvent newEvent)
        {
            // Ignore replacement requests when the row no longer exists.
            if (!_rows.TryGetValue(info.Action, out HotkeyRow? row))
                return;

            row.ReplaceButton(info, newEvent);
        }

        /// <summary>
        /// Ensures a row has an add-binding (+) button after editing completes.
        /// </summary>
        /// <param name="action">Action row that should receive the plus button.</param>
        public void AddPlusButton(StringName action)
        {
            // Add a plus button only when the action row is still present.
            if (_rows.TryGetValue(action, out HotkeyRow? row))
                row.AddPlusButton();
        }

        /// <summary>
        /// Moves keyboard focus to the plus button in the specified action row.
        /// </summary>
        /// <param name="action">Action row whose plus button should be focused.</param>
        public void FocusPlusButton(StringName action)
        {
            // Focus only when the requested action row exists.
            if (_rows.TryGetValue(action, out HotkeyRow? row))
                row.FocusPlusButton();
        }

        /// <summary>
        /// Relays row-level hotkey button presses to external listeners.
        /// </summary>
        /// <param name="info">Button metadata describing the pressed binding button.</param>
        private void HandleHotkeyPressed(HotkeyButtonInfo info)
        {
            HotkeyPressed?.Invoke(info);
        }

        /// <summary>
        /// Relays row-level plus button presses to external listeners.
        /// </summary>
        /// <param name="info">Button metadata describing the pressed plus button.</param>
        private void HandlePlusPressed(HotkeyButtonInfo info)
        {
            PlusPressed?.Invoke(info);
        }

        /// <summary>
        /// Filters out framework-only actions that are not user-editable.
        /// </summary>
        /// <param name="action">Input action name to evaluate.</param>
        /// <returns><see langword="true"/> when the action should be shown.</returns>
        private bool ShouldDisplayAction(StringName action)
        {
            string actionStr = action.ToString();
            return actionStr != _removeHotkeyAction && !actionStr.StartsWith(_uiPrefix);
        }

        /// <summary>
        /// Converts an action identifier into the title-cased label shown in the UI.
        /// </summary>
        /// <param name="action">Input action to convert.</param>
        /// <returns>Human-friendly display name.</returns>
        private static string GetDisplayName(StringName action)
        {
            return action.ToString().Replace('_', ' ').ToTitleCase();
        }
    }
}
