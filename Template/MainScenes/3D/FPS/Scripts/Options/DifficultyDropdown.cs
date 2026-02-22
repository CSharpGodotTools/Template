using Framework;
using Framework.UI;
using System.Collections.Generic;

namespace __TEMPLATE__.FPS;

public sealed class DifficultyDropdown : DropdownOptionDefinition
{
    public override OptionsTab Tab => OptionsTab.Gameplay;
    public override string Label => "DIFFICULTY";
    public override int DefaultValue => (int)Difficulty.Normal;
    public override IReadOnlyList<string> Items => ["EASY", "NORMAL", "HARD"];

    public override int GetValue()
    {
        return (int)GameFramework.Settings.Difficulty;
    }

    public override void SetValue(int value)
    {
        GameFramework.Settings.Difficulty = (Difficulty)value;
    }
}
