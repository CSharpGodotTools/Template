namespace __TEMPLATE__.Ui;

public sealed class OptionsAudioTab : IOptionsTabRegistrar
{
    public string TabName => OptionsTabs.Audio;

    public void Register(IOptionsService optionsService)
    {
        optionsService.AddOption(
            OptionDefinitions.Slider(
                tab: TabName,
                label: "MUSIC",
                minValue: 0,
                maxValue: 100,
                getValue: () => optionsService.Settings.MusicVolume,
                setValue: value => optionsService.Settings.MusicVolume = value,
                step: 1.0,
                saveKey: OptionsSaveKeys.MusicVolume,
                defaultValue: 100));

        optionsService.AddOption(
            OptionDefinitions.Slider(
                tab: TabName,
                label: "SOUNDS",
                minValue: 0,
                maxValue: 100,
                getValue: () => optionsService.Settings.SFXVolume,
                setValue: value => optionsService.Settings.SFXVolume = value,
                step: 1.0,
                saveKey: OptionsSaveKeys.SfxVolume,
                defaultValue: 100));
    }
}
