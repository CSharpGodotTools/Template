using Godot;

namespace Framework.Ui;

/// <summary>
/// Factory that creates registered option wrappers with
/// persistence‑backed getters and setters.
/// </summary>
internal static class OptionRegistrar
{
    internal static RegisteredSliderOption CreateSlider(
        int id, SliderOptionDefinition option, OptionPersistence persistence)
    {
        string key = persistence.GetSaveKey(option.Label);
        float min = (float)option.MinValue;
        float max = (float)option.MaxValue;
        float def = Mathf.Clamp(option.DefaultValue, min, max);

        // Initialise from persisted storage, clamped to the valid range
        float initial = Mathf.Clamp(persistence.GetSliderValue(key, def), min, max);
        persistence.SetSliderValue(key, initial);
        option.SetValue(initial);

        return new RegisteredSliderOption(id, option,
            () => { float v = Mathf.Clamp(persistence.GetSliderValue(key, def), min, max); persistence.SetSliderValue(key, v); return v; },
            v => { float c = Mathf.Clamp(v, min, max); persistence.SetSliderValue(key, c); option.SetValue(c); });
    }

    internal static RegisteredDropdownOption CreateDropdown(
        int id, DropdownOptionDefinition option, OptionPersistence persistence)
    {
        string key = persistence.GetSaveKey(option.Label);
        int maxIndex = option.Items.Count - 1;
        int def = Mathf.Clamp(option.DefaultValue, 0, maxIndex);

        int initial = Mathf.Clamp(persistence.GetDropdownValue(key, def), 0, maxIndex);
        persistence.SetDropdownValue(key, initial);
        option.SetValue(initial);

        return new RegisteredDropdownOption(id, option,
            () => { int v = Mathf.Clamp(persistence.GetDropdownValue(key, def), 0, maxIndex); persistence.SetDropdownValue(key, v); return v; },
            v => { int c = Mathf.Clamp(v, 0, maxIndex); persistence.SetDropdownValue(key, c); option.SetValue(c); });
    }

    internal static RegisteredLineEditOption CreateLineEdit(
        int id, LineEditOptionDefinition option, OptionPersistence persistence)
    {
        string key = persistence.GetSaveKey(option.Label);
        string def = option.DefaultValue ?? string.Empty;

        string initial = persistence.GetLineEditValue(key, def);
        persistence.SetLineEditValue(key, initial);
        option.SetValue(initial);

        return new RegisteredLineEditOption(id, option,
            () => persistence.GetLineEditValue(key, def),
            v => { string s = v ?? string.Empty; persistence.SetLineEditValue(key, s); option.SetValue(s); });
    }

    internal static RegisteredToggleOption CreateToggle(
        int id, ToggleOptionDefinition option, OptionPersistence persistence)
    {
        string key = persistence.GetSaveKey(option.Label);

        bool initial = persistence.GetToggleValue(key, option.DefaultValue);
        persistence.SetToggleValue(key, initial);
        option.SetValue(initial);

        return new RegisteredToggleOption(id, option,
            () => { bool v = persistence.GetToggleValue(key, option.DefaultValue); persistence.SetToggleValue(key, v); return v; },
            v => { persistence.SetToggleValue(key, v); option.SetValue(v); });
    }
}
