using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

internal sealed class OptionsValueStoreComponent
{
    private readonly OptionsSettingsStore _settingsStore;
    private readonly OptionsCustomRegistry _customRegistry;
    private readonly ResourceOptions _options;

    public OptionsValueStoreComponent(OptionsSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _options = _settingsStore.Load();
        _options.Normalize();
        _customRegistry = new OptionsCustomRegistry(_options);
    }

    public int GetInt(string key, int defaultValue) => _customRegistry.GetOptionInt(key, defaultValue);
    public float GetFloat(string key, float defaultValue) => _customRegistry.GetOptionFloat(key, defaultValue);
    public string GetString(string key, string defaultValue) => _customRegistry.GetOptionString(key, defaultValue);
    public bool GetBool(string key, bool defaultValue) => _customRegistry.GetOptionBool(key, defaultValue);

    public void SetInt(string key, int value) => _customRegistry.SetOptionInt(key, value);
    public void SetFloat(string key, float value) => _customRegistry.SetOptionFloat(key, value);
    public void SetString(string key, string value) => _customRegistry.SetOptionString(key, value);
    public void SetBool(string key, bool value) => _customRegistry.SetOptionBool(key, value);

    public RegisteredSliderOption AddSlider(SliderOptionDefinition option) => _customRegistry.AddSlider(option);
    public RegisteredDropdownOption AddDropdown(DropdownOptionDefinition option) => _customRegistry.AddDropdown(option);
    public RegisteredLineEditOption AddLineEdit(LineEditOptionDefinition option) => _customRegistry.AddLineEdit(option);
    public RegisteredToggleOption AddToggle(ToggleOptionDefinition option) => _customRegistry.AddToggle(option);

    public IEnumerable<RegisteredSliderOption> GetSliderOptions() => _customRegistry.GetSliderOptions();
    public IEnumerable<RegisteredDropdownOption> GetDropdownOptions() => _customRegistry.GetDropdownOptions();
    public IEnumerable<RegisteredLineEditOption> GetLineEditOptions() => _customRegistry.GetLineEditOptions();
    public IEnumerable<RegisteredToggleOption> GetToggleOptions() => _customRegistry.GetToggleOptions();

    public void Save()
    {
        _settingsStore.Save(_options);
    }
}
