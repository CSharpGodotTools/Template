using Godot;
using GodotUtils;
using System;

namespace Framework.Ui;

public partial class OptionsInput
{
    public sealed class HotkeyRow
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

        public void AddBindings(Godot.Collections.Array<InputEvent> events)
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
}


