using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;

namespace Framework.Ui;

public partial class OptionsInput
{
    public sealed class HotkeyListView
    {
        private readonly VBoxContainer _content;
        private readonly Button _inputNavBtn;
        private readonly HotkeyStore _store;
        private readonly string _removeHotkeyAction;
        private readonly string _uiPrefix;
        private readonly string _ellipsis;

        private readonly Dictionary<StringName, HotkeyRow> _rows = [];

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
            Godot.Collections.Array<Node> children = _content.GetChildren();
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
}


