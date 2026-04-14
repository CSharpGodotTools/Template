using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.Ui;

public partial class OptionsInput
{
    /// <summary>
    /// Owns one action row in the hotkey list, including existing bindings and the plus button.
    /// </summary>
    public sealed class HotkeyRow
    {
        private readonly StringName _action;
        private readonly Button _inputNavBtn;
        private readonly Action<HotkeyButtonInfo> _onHotkeyPressed;
        private readonly Action<HotkeyButtonInfo> _onPlusPressed;
        private readonly FocusOutlineManager _focusOutline;
        private readonly HBoxContainer _events;

        /// <summary>
        /// Creates a UI row for one input action.
        /// </summary>
        /// <param name="action">Input action represented by this row.</param>
        /// <param name="inputNavBtn">Navigation button used as left-focus neighbor for the first binding.</param>
        /// <param name="displayName">Localized or formatted label shown for the action.</param>
        /// <param name="onHotkeyPressed">Callback invoked when a binding button is pressed.</param>
        /// <param name="onPlusPressed">Callback invoked when the plus button is pressed.</param>
        /// <param name="focusOutline">Focus helper used for keyboard navigation.</param>
        public HotkeyRow(
            StringName action,
            Button inputNavBtn,
            string displayName,
            Action<HotkeyButtonInfo> onHotkeyPressed,
            Action<HotkeyButtonInfo> onPlusPressed,
            FocusOutlineManager focusOutline)
        {
            _action = action;
            _inputNavBtn = inputNavBtn;
            _onHotkeyPressed = onHotkeyPressed;
            _onPlusPressed = onPlusPressed;
            _focusOutline = focusOutline;

            RowRoot = new HBoxContainer();

            Label label = LabelFactory.Create(displayName);
            RowRoot.AddChild(label);

            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.CustomMinimumSize = new Vector2(200, 0);

            _events = new HBoxContainer();
            RowRoot.AddChild(_events);
        }

        /// <summary>
        /// Root control for this action row.
        /// </summary>
        public HBoxContainer RowRoot { get; }

        /// <summary>
        /// Adds binding buttons for keyboard and mouse events currently assigned to the action.
        /// </summary>
        /// <param name="events">Existing input events bound to the action.</param>
        public void AddBindings(Godot.Collections.Array<InputEvent> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                bool isFirst = i == 0;

                InputEvent @event = events[i];
                // Render buttons only for keyboard and mouse-button bindings.
                if (@event is InputEventKey || @event is InputEventMouseButton)
                    CreateBindingButton(@event, isFirst);
            }
        }

        /// <summary>
        /// Adds the trailing plus button used to append a new binding.
        /// </summary>
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
                InputEvent = null!,
                IsPlus = true
            };

            AttachHandlers(btn, info, _onPlusPressed);
        }

        /// <summary>
        /// Replaces a single binding button while preserving its position in the row.
        /// </summary>
        /// <param name="info">Metadata for the binding button being replaced.</param>
        /// <param name="newEvent">New input event that should be represented.</param>
        public void ReplaceButton(HotkeyButtonInfo info, InputEvent newEvent)
        {
            bool wasFirst = info.Button.FocusNeighborLeft != null;
            int index = info.Button.GetIndex();

            info.Button.QueueFree();

            HotkeyButton btn = CreateBindingButton(newEvent, wasFirst);
            _events.MoveChild(btn, index);
        }

        /// <summary>
        /// Focuses the plus button in this row when available.
        /// </summary>
        public void FocusPlusButton()
        {
            // Abort when this row has no child buttons to focus.
            if (_events.GetChildCount() == 0)
                return;

            Button plusBtn = _events.GetChild<Button>(_events.GetChildCount() - 1);
            _focusOutline.Focus(plusBtn);
        }

        /// <summary>
        /// Creates and wires one binding button for the provided input event.
        /// </summary>
        /// <param name="inputEvent">Input event represented by the new button.</param>
        /// <param name="isFirst"><see langword="true"/> when this is the first binding button in the row.</param>
        /// <returns>The newly created binding button.</returns>
        private HotkeyButton CreateBindingButton(InputEvent inputEvent, bool isFirst)
        {
            string readable = GetReadableForInput(inputEvent);

            HotkeyButton btn = new()
            {
                Text = readable
            };

            // Wire left-navigation from the first binding to the input tab button.
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

        /// <summary>
        /// Hooks press and cleanup handlers to a hotkey button.
        /// </summary>
        /// <param name="btn">Button to wire.</param>
        /// <param name="info">Metadata assigned to the button.</param>
        /// <param name="handler">Press callback to invoke when triggered.</param>
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
}
