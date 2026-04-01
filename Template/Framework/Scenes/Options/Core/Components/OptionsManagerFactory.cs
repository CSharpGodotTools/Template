namespace __TEMPLATE__.Ui;

/// <summary>
/// Creates and wires the options management component graph.
/// </summary>
internal static class OptionsManagerFactory
{
    /// <summary>
    /// Creates a fully composed <see cref="OptionsManager"/> instance.
    /// </summary>
    /// <param name="autoloads">Autoload access for runtime dependencies.</param>
    /// <returns>Configured options manager instance.</returns>
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


        // Keep composition centralized so component lifetimes are easy to reason about.
        return new OptionsManager(
            hotkeysService,
            settingDispatcher,
            displaySettings,
            registration,
            rightControls,
            lifecycle);
    }
}
