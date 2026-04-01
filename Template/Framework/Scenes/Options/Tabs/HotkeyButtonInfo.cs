using Godot;

namespace __TEMPLATE__.Ui;

public partial class OptionsInput
{
    /// <summary>
    /// Captures context for a hotkey button interaction.
    /// </summary>
    public sealed class HotkeyButtonInfo
    {
        /// <summary>
        /// Gets original button text shown before listening state.
        /// </summary>
        public required string OriginalText { get; init; }

        /// <summary>
        /// Gets input action name represented by this button.
        /// </summary>
        public required StringName Action { get; init; }

        /// <summary>
        /// Gets owning hotkey row.
        /// </summary>
        public required HotkeyRow Row { get; init; }

        /// <summary>
        /// Gets button control associated with this metadata.
        /// </summary>
        public required Button Button { get; init; }

        /// <summary>
        /// Gets current bound input event.
        /// </summary>
        public required InputEvent InputEvent { get; init; }

        /// <summary>
        /// Gets whether this info refers to a plus/add button.
        /// </summary>
        public bool IsPlus { get; init; }
    }
}
