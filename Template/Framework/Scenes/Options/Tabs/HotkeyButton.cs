using Godot;
using System;

namespace Framework.UI;

public partial class OptionsInput
{
    public partial class HotkeyButton : Button
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
}


