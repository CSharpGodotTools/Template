using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Wraps option persistence and custom option registration operations.
/// </summary>
internal sealed class OptionsValueStoreComponent
{
    private readonly OptionsSettingsStore _settingsStore;
    private readonly OptionsCustomRegistry _customRegistry;
    private readonly ResourceOptions _options;

    /// <summary>
    /// Initializes value storage and normalizes loaded option data.
    /// </summary>
    /// <param name="settingsStore">Persistent settings store implementation.</param>
    public OptionsValueStoreComponent(OptionsSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _options = OptionsSettingsStore.Load();
        _options.Normalize();
        _customRegistry = new OptionsCustomRegistry(_options);
    }

    /// <summary>
    /// Gets an integer option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when key is absent.</param>
    /// <returns>Resolved integer value.</returns>
    public int GetInt(string key, int defaultValue) => _customRegistry.GetOptionInt(key, defaultValue);

    /// <summary>
    /// Gets a float option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when key is absent.</param>
    /// <returns>Resolved float value.</returns>
    public float GetFloat(string key, float defaultValue) => _customRegistry.GetOptionFloat(key, defaultValue);

    /// <summary>
    /// Gets a string option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when key is absent.</param>
    /// <returns>Resolved string value.</returns>
    public string GetString(string key, string defaultValue) => _customRegistry.GetOptionString(key, defaultValue);

    /// <summary>
    /// Gets a boolean option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when key is absent.</param>
    /// <returns>Resolved boolean value.</returns>
    public bool GetBool(string key, bool defaultValue) => _customRegistry.GetOptionBool(key, defaultValue);

    /// <summary>
    /// Stores an integer option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    public void SetInt(string key, int value) => _customRegistry.SetOptionInt(key, value);

    /// <summary>
    /// Stores a float option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    public void SetFloat(string key, float value) => _customRegistry.SetOptionFloat(key, value);

    /// <summary>
    /// Stores a string option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    public void SetString(string key, string value) => _customRegistry.SetOptionString(key, value);

    /// <summary>
    /// Stores a boolean option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    public void SetBool(string key, bool value) => _customRegistry.SetOptionBool(key, value);

    /// <summary>
    /// Registers a slider option definition.
    /// </summary>
    /// <param name="option">Slider definition.</param>
    /// <returns>Registered slider option descriptor.</returns>
    public RegisteredSliderOption AddSlider(SliderOptionDefinition option) => _customRegistry.AddSlider(option);

    /// <summary>
    /// Registers a dropdown option definition.
    /// </summary>
    /// <param name="option">Dropdown definition.</param>
    /// <returns>Registered dropdown option descriptor.</returns>
    public RegisteredDropdownOption AddDropdown(DropdownOptionDefinition option) => _customRegistry.AddDropdown(option);

    /// <summary>
    /// Registers a line-edit option definition.
    /// </summary>
    /// <param name="option">Line-edit definition.</param>
    /// <returns>Registered line-edit option descriptor.</returns>
    public RegisteredLineEditOption AddLineEdit(LineEditOptionDefinition option) => _customRegistry.AddLineEdit(option);

    /// <summary>
    /// Registers a toggle option definition.
    /// </summary>
    /// <param name="option">Toggle definition.</param>
    /// <returns>Registered toggle option descriptor.</returns>
    public RegisteredToggleOption AddToggle(ToggleOptionDefinition option) => _customRegistry.AddToggle(option);

    /// <summary>
    /// Gets registered slider options.
    /// </summary>
    /// <returns>Slider option descriptors.</returns>
    public IEnumerable<RegisteredSliderOption> GetSliderOptions() => _customRegistry.GetSliderOptions();

    /// <summary>
    /// Gets registered dropdown options.
    /// </summary>
    /// <returns>Dropdown option descriptors.</returns>
    public IEnumerable<RegisteredDropdownOption> GetDropdownOptions() => _customRegistry.GetDropdownOptions();

    /// <summary>
    /// Gets registered line-edit options.
    /// </summary>
    /// <returns>Line-edit option descriptors.</returns>
    public IEnumerable<RegisteredLineEditOption> GetLineEditOptions() => _customRegistry.GetLineEditOptions();

    /// <summary>
    /// Gets registered toggle options.
    /// </summary>
    /// <returns>Toggle option descriptors.</returns>
    public IEnumerable<RegisteredToggleOption> GetToggleOptions() => _customRegistry.GetToggleOptions();

    /// <summary>
    /// Persists the current options resource to storage.
    /// </summary>
    public void Save()
    {
        _settingsStore.Save(_options);
    }
}
