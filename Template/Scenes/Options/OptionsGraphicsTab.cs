namespace __TEMPLATE__.Ui;

public sealed class OptionsGraphicsTab : IOptionsTabRegistrar
{
    public string TabName => OptionsTabs.Graphics;

    public void Register(IOptionsService optionsService)
    {
        optionsService.AddOption(
            OptionDefinitions.Dropdown(
                tab: TabName,
                label: "QUALITY_PRESET",
                items: ["LOW", "MEDIUM", "HIGH"],
                getValue: () => optionsService.Settings.QualityPreset,
                setValue: value => optionsService.Settings.QualityPreset = value,
                saveKey: OptionsSaveKeys.QualityPreset,
                defaultValue: (int)QualityPreset.High));

        optionsService.AddOption(
            OptionDefinitions.Dropdown(
                tab: TabName,
                label: "ANTIALIASING",
                items: ["DISABLED", "2x", "4x", "8x"],
                getValue: () => optionsService.Settings.Antialiasing,
                setValue: value => optionsService.Settings.Antialiasing = value,
                saveKey: OptionsSaveKeys.Antialiasing,
                defaultValue: 3));
    }
}

public enum QualityPreset
{
    Low,
    Medium,
    High
}
