using System;
using System.Collections.Generic;

#nullable enable

namespace __TEMPLATE__.Ui;

/// <summary>
/// Central registry for custom option definitions. Manages stable IDs,
/// typed storage, and delegates creation to <see cref="OptionRegistrar"/>.
/// </summary>
internal sealed class OptionsCustomRegistry
{
    private readonly OptionPersistence _persistence;
    private readonly Dictionary<int, RegisteredSliderOption> _sliders = [];
    private readonly Dictionary<int, RegisteredDropdownOption> _dropdowns = [];
    private readonly Dictionary<int, RegisteredLineEditOption> _lineEdits = [];
    private readonly Dictionary<int, RegisteredToggleOption> _toggles = [];
    private readonly Dictionary<string, int> _optionIds = new(StringComparer.OrdinalIgnoreCase);
    private int _nextId;

    public OptionsCustomRegistry(ResourceOptions options)
    {
        _persistence = new OptionPersistence(options);
    }

    // -- Getters --

    public IEnumerable<RegisteredSliderOption> GetSliderOptions() => _sliders.Values;
    public IEnumerable<RegisteredDropdownOption> GetDropdownOptions() => _dropdowns.Values;
    public IEnumerable<RegisteredLineEditOption> GetLineEditOptions() => _lineEdits.Values;
    public IEnumerable<RegisteredToggleOption> GetToggleOptions() => _toggles.Values;

    public int GetOptionInt(string key, int defaultValue) => _persistence.GetDropdownValue(key, defaultValue);
    public float GetOptionFloat(string key, float defaultValue) => _persistence.GetSliderValue(key, defaultValue);
    public string GetOptionString(string key, string defaultValue) => _persistence.GetLineEditValue(key, defaultValue);
    public bool GetOptionBool(string key, bool defaultValue) => _persistence.GetToggleValue(key, defaultValue);

    public void SetOptionInt(string key, int value) => _persistence.SetDropdownValue(key, value);
    public void SetOptionFloat(string key, float value) => _persistence.SetSliderValue(key, value);
    public void SetOptionString(string key, string value) => _persistence.SetLineEditValue(key, value);
    public void SetOptionBool(string key, bool value) => _persistence.SetToggleValue(key, value);

    // -- Registration --

    public RegisteredSliderOption AddSlider(SliderOptionDefinition option)
    {
        OptionValidator.ValidateTab(option.Tab);
        OptionValidator.ValidateLabel(option.Label, "Slider");
        OptionValidator.ValidateSliderRange(option.MinValue, option.MaxValue, option.Step);
        return Register(option, _sliders, OptionRegistrar.CreateSlider);
    }

    public RegisteredDropdownOption AddDropdown(DropdownOptionDefinition option)
    {
        OptionValidator.ValidateTab(option.Tab);
        OptionValidator.ValidateLabel(option.Label, "Dropdown");
        OptionValidator.ValidateDropdownItems(option.Items);
        return Register(option, _dropdowns, OptionRegistrar.CreateDropdown);
    }

    public RegisteredLineEditOption AddLineEdit(LineEditOptionDefinition option)
    {
        OptionValidator.ValidateTab(option.Tab);
        OptionValidator.ValidateLabel(option.Label, "LineEdit");
        return Register(option, _lineEdits, OptionRegistrar.CreateLineEdit);
    }

    public RegisteredToggleOption AddToggle(ToggleOptionDefinition option)
    {
        OptionValidator.ValidateTab(option.Tab);
        OptionValidator.ValidateLabel(option.Label, "Toggle");
        return Register(option, _toggles, OptionRegistrar.CreateToggle);
    }

    // -- Helpers --

    /// <summary>Shared flow: null‑check, assign ID, create, replace, store.</summary>
    private TReg Register<TDef, TReg>(
        TDef option, Dictionary<int, TReg> storage,
        Func<int, TDef, OptionPersistence, TReg> factory) where TDef : OptionDefinition
    {
        ArgumentNullException.ThrowIfNull(option);
        int id = GetOrCreateId(option.Tab, option.Label);
        TReg registered = factory(id, option, _persistence);
        ReplaceExisting(id);
        storage[id] = registered;
        return registered;
    }

    private int GetOrCreateId(string tab, string label)
    {
        string key = CreateOptionKey(tab, label);

        if (_optionIds.TryGetValue(key, out int id))
            return id;

        id = ++_nextId;
        _optionIds[key] = id;
        return id;
    }

    private static string CreateOptionKey(string tab, string label)
    {
        return $"{tab.Trim()}::{label.Trim()}";
    }

    /// <summary>Clears any existing registration for the given ID across all types.</summary>
    private void ReplaceExisting(int id)
    {
        _sliders.Remove(id);
        _dropdowns.Remove(id);
        _lineEdits.Remove(id);
        _toggles.Remove(id);
    }
}
