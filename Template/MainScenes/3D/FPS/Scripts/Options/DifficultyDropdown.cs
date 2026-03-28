using __TEMPLATE__;
using __TEMPLATE__.Ui;
using System.Collections.Generic;

namespace __TEMPLATE__.FPS;

public sealed class DifficultyDropdown : DropdownOptionDefinition
{
    private readonly IOptionsService _optionsService;

    public DifficultyDropdown(IOptionsService optionsService)
    {
        _optionsService = optionsService;
    }

    public override OptionsTab Tab => OptionsTab.Gameplay;
    public override string Label => "DIFFICULTY";
    public override int DefaultValue => (int)Difficulty.Normal;
    public override IReadOnlyList<string> Items => ["EASY", "NORMAL", "HARD"];

    public override int GetValue()
    {
        return (int)_optionsService.Settings.Difficulty;
    }

    public override void SetValue(int value)
    {
        _optionsService.SetDifficulty((Difficulty)value);
    }
}
