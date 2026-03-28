namespace __TEMPLATE__.Ui;

public sealed class OptionsGeneralTab : IOptionsTabRegistrar
{
    public string TabName => OptionsTabs.General;

    public void Register(IOptionsService optionsService)
    {
        optionsService.AddOption(
            OptionDefinitions.Dropdown(
                tab: TabName,
                label: "LANGUAGE",
                items: ["English", "French", "Japanese"],
                getValue: () => optionsService.Settings.Language,
                setValue: value => optionsService.Settings.Language = value,
                saveKey: OptionsSaveKeys.Language,
                defaultValue: (int)Language.English));
    }
}

public enum Language
{
    English,
    French,
    Japanese
}
