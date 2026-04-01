using Godot;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Factory that creates registered option wrappers with
/// persistence‑backed getters and setters.
/// </summary>
internal static class OptionRegistrar
{
    /// <summary>
    /// Creates a registered slider option backed by persistence wrappers.
    /// </summary>
    /// <param name="id">Stable option id.</param>
    /// <param name="option">Slider option definition.</param>
    /// <param name="persistence">Persistence service.</param>
    /// <returns>Registered slider option.</returns>
    internal static RegisteredSliderOption CreateSlider(
        int id, SliderOptionDefinition option, OptionPersistence persistence)
    {
        string key = OptionPersistence.GetSaveKey(option.Tab, option.Label, option.SaveKey);
        string legacyKey = OptionPersistence.GetLegacySaveKey(option.Label);
        float min = (float)option.MinValue;
        float max = (float)option.MaxValue;
        float def = Mathf.Clamp(option.DefaultValue, min, max);

        // Initialise from persisted storage, clamped to the valid range
        float initial = Mathf.Clamp(persistence.GetSliderValue(key, def, legacyKey), min, max);
        persistence.SetSliderValue(key, initial);
        option.SetValue(initial);

        return new RegisteredSliderOption(id, option,
            () => { float v = Mathf.Clamp(persistence.GetSliderValue(key, def, legacyKey), min, max); persistence.SetSliderValue(key, v); return v; },
            v => { float c = Mathf.Clamp(v, min, max); persistence.SetSliderValue(key, c); option.SetValue(c); });
    }

    /// <summary>
    /// Creates a registered dropdown option backed by persistence wrappers.
    /// </summary>
    /// <param name="id">Stable option id.</param>
    /// <param name="option">Dropdown option definition.</param>
    /// <param name="persistence">Persistence service.</param>
    /// <returns>Registered dropdown option.</returns>
    internal static RegisteredDropdownOption CreateDropdown(
        int id, DropdownOptionDefinition option, OptionPersistence persistence)
    {
        string key = OptionPersistence.GetSaveKey(option.Tab, option.Label, option.SaveKey);
        string legacyKey = OptionPersistence.GetLegacySaveKey(option.Label);
        int maxIndex = option.Items.Count - 1;
        int def = Mathf.Clamp(option.DefaultValue, 0, maxIndex);

        int initial = Mathf.Clamp(persistence.GetDropdownValue(key, def, legacyKey), 0, maxIndex);
        persistence.SetDropdownValue(key, initial);
        option.SetValue(initial);

        return new RegisteredDropdownOption(id, option,
            () => { int v = Mathf.Clamp(persistence.GetDropdownValue(key, def, legacyKey), 0, maxIndex); persistence.SetDropdownValue(key, v); return v; },
            v => { int c = Mathf.Clamp(v, 0, maxIndex); persistence.SetDropdownValue(key, c); option.SetValue(c); });
    }

    /// <summary>
    /// Creates a registered line-edit option backed by persistence wrappers.
    /// </summary>
    /// <param name="id">Stable option id.</param>
    /// <param name="option">Line-edit option definition.</param>
    /// <param name="persistence">Persistence service.</param>
    /// <returns>Registered line-edit option.</returns>
    internal static RegisteredLineEditOption CreateLineEdit(
        int id, LineEditOptionDefinition option, OptionPersistence persistence)
    {
        string key = OptionPersistence.GetSaveKey(option.Tab, option.Label, option.SaveKey);
        string legacyKey = OptionPersistence.GetLegacySaveKey(option.Label);
        string def = option.DefaultValue ?? string.Empty;

        string initial = persistence.GetLineEditValue(key, def, legacyKey);
        persistence.SetLineEditValue(key, initial);
        option.SetValue(initial);

        return new RegisteredLineEditOption(id, option,
            () => persistence.GetLineEditValue(key, def, legacyKey),
            v => { string s = v ?? string.Empty; persistence.SetLineEditValue(key, s); option.SetValue(s); });
    }

    /// <summary>
    /// Creates a registered toggle option backed by persistence wrappers.
    /// </summary>
    /// <param name="id">Stable option id.</param>
    /// <param name="option">Toggle option definition.</param>
    /// <param name="persistence">Persistence service.</param>
    /// <returns>Registered toggle option.</returns>
    internal static RegisteredToggleOption CreateToggle(
        int id, ToggleOptionDefinition option, OptionPersistence persistence)
    {
        string key = OptionPersistence.GetSaveKey(option.Tab, option.Label, option.SaveKey);
        string legacyKey = OptionPersistence.GetLegacySaveKey(option.Label);

        bool initial = persistence.GetToggleValue(key, option.DefaultValue, legacyKey);
        persistence.SetToggleValue(key, initial);
        option.SetValue(initial);

        return new RegisteredToggleOption(id, option,
            () => { bool v = persistence.GetToggleValue(key, option.DefaultValue, legacyKey); persistence.SetToggleValue(key, v); return v; },
            v => { persistence.SetToggleValue(key, v); option.SetValue(v); });
    }
}
