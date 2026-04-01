using Godot;
using System;

namespace __TEMPLATE__.Ui;

public partial class OptionsInput
{
    /// <summary>
    /// UI button that represents a single hotkey binding slot.
    /// </summary>
    public partial class HotkeyButton : Button
    {
        /// <summary>
        /// Raised when the hotkey button is pressed.
        /// </summary>
        public event Action<HotkeyButtonInfo> HotkeyPressed = null!;

        /// <summary>
        /// Gets or sets metadata describing the represented hotkey binding.
        /// </summary>
        public HotkeyButtonInfo Info { get; set; } = null!;

        public override void _Ready()
        {
            Pressed += OnPressedLocal;
            TreeExited += OnTreeExited;
        }

        /// <summary>
        /// Unsubscribes local handlers when this button exits the tree.
        /// </summary>
        private void OnTreeExited()
        {
            Pressed -= OnPressedLocal;
            TreeExited -= OnTreeExited;
        }

        /// <summary>
        /// Forwards button presses to external listeners with current metadata.
        /// </summary>
        private void OnPressedLocal()
        {
            HotkeyPressed?.Invoke(Info);
        }
    }
}
