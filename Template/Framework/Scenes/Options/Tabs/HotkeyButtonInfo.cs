using Godot;

namespace Framework.UI;

public partial class OptionsInput
{
    public sealed class HotkeyButtonInfo
    {
        public required string OriginalText { get; init; }
        public required StringName Action { get; init; }
        public required HotkeyRow Row { get; init; }
        public required Button Button { get; init; }
        public InputEvent InputEvent { get; init; }
        public bool IsPlus { get; init; }
    }
}


