namespace __TEMPLATE__.Ui;

internal static class OptionsManagerFactory
{
    public static OptionsManager Create(AutoloadsFramework autoloads)
    {
        OptionsSettingsStore settingsStore = new();
        OptionsValueStoreComponent valueStore = new(settingsStore);
        OptionsHotkeysService hotkeysService = new();
        OptionsAudioSettingsComponent audioSettings = new(autoloads, valueStore);
        OptionsVisualSettingsComponent visualSettings = new(valueStore);
        OptionsDisplaySettingsComponent displaySettings = new(autoloads, valueStore, visualSettings);
        OptionsSettingDispatcherComponent settingDispatcher = new(valueStore, displaySettings, audioSettings);
        OptionsRegistrationComponent registration = new(valueStore);
        OptionsRightControlRegistryComponent rightControls = new();
        OptionsLifecycleComponent lifecycle = new(autoloads, valueStore, hotkeysService, displaySettings);

        return new OptionsManager(
            hotkeysService,
            settingDispatcher,
            displaySettings,
            registration,
            rightControls,
            lifecycle);
    }
}
