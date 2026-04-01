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

    /// <summary>
    /// Initializes the custom option registry.
    /// </summary>
    /// <param name="options">Options resource used for persistence.</param>
    public OptionsCustomRegistry(ResourceOptions options)
    {
        _persistence = new OptionPersistence(options);
    }

    // -- Getters --

    /// <summary>
    /// Gets all registered slider options.
    /// </summary>
    /// <returns>Registered slider options.</returns>
    public IEnumerable<RegisteredSliderOption> GetSliderOptions() => _sliders.Values;

    /// <summary>
    /// Gets all registered dropdown options.
    /// </summary>
    /// <returns>Registered dropdown options.</returns>
    public IEnumerable<RegisteredDropdownOption> GetDropdownOptions() => _dropdowns.Values;

    /// <summary>
    /// Gets all registered line-edit options.
    /// </summary>
    /// <returns>Registered line-edit options.</returns>
    public IEnumerable<RegisteredLineEditOption> GetLineEditOptions() => _lineEdits.Values;

    /// <summary>
    /// Gets all registered toggle options.
    /// </summary>
    /// <returns>Registered toggle options.</returns>
    public IEnumerable<RegisteredToggleOption> GetToggleOptions() => _toggles.Values;

    /// <summary>
    /// Reads an integer option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when missing.</param>
    /// <returns>Resolved integer value.</returns>
    public int GetOptionInt(string key, int defaultValue) => _persistence.GetDropdownValue(key, defaultValue);

    /// <summary>
    /// Reads a float option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when missing.</param>
    /// <returns>Resolved float value.</returns>
    public float GetOptionFloat(string key, float defaultValue) => _persistence.GetSliderValue(key, defaultValue);

    /// <summary>
    /// Reads a string option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when missing.</param>
    /// <returns>Resolved string value.</returns>
    public string GetOptionString(string key, string defaultValue) => _persistence.GetLineEditValue(key, defaultValue);

    /// <summary>
    /// Reads a boolean option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when missing.</param>
    /// <returns>Resolved boolean value.</returns>
    public bool GetOptionBool(string key, bool defaultValue) => _persistence.GetToggleValue(key, defaultValue);

    /// <summary>
    /// Stores an integer option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    public void SetOptionInt(string key, int value) => _persistence.SetDropdownValue(key, value);

    /// <summary>
    /// Stores a float option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    public void SetOptionFloat(string key, float value) => _persistence.SetSliderValue(key, value);

    /// <summary>
    /// Stores a string option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    public void SetOptionString(string key, string value) => _persistence.SetLineEditValue(key, value);

    /// <summary>
    /// Stores a boolean option value.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    public void SetOptionBool(string key, bool value) => _persistence.SetToggleValue(key, value);

    // -- Registration --

    /// <summary>
    /// Registers a slider option definition.
    /// </summary>
    /// <param name="option">Slider option definition.</param>
    /// <returns>Registered slider option wrapper.</returns>
    public RegisteredSliderOption AddSlider(SliderOptionDefinition option)
    {
        OptionValidator.ValidateTab(option.Tab);
        OptionValidator.ValidateLabel(option.Label, "Slider");
        OptionValidator.ValidateSliderRange(option.MinValue, option.MaxValue, option.Step);
        return Register(option, _sliders, OptionRegistrar.CreateSlider);
    }

    /// <summary>
    /// Registers a dropdown option definition.
    /// </summary>
    /// <param name="option">Dropdown option definition.</param>
    /// <returns>Registered dropdown option wrapper.</returns>
    public RegisteredDropdownOption AddDropdown(DropdownOptionDefinition option)
    {
        OptionValidator.ValidateTab(option.Tab);
        OptionValidator.ValidateLabel(option.Label, "Dropdown");
        OptionValidator.ValidateDropdownItems(option.Items);
        return Register(option, _dropdowns, OptionRegistrar.CreateDropdown);
    }

    /// <summary>
    /// Registers a line-edit option definition.
    /// </summary>
    /// <param name="option">Line-edit option definition.</param>
    /// <returns>Registered line-edit option wrapper.</returns>
    public RegisteredLineEditOption AddLineEdit(LineEditOptionDefinition option)
    {
        OptionValidator.ValidateTab(option.Tab);
        OptionValidator.ValidateLabel(option.Label, "LineEdit");
        return Register(option, _lineEdits, OptionRegistrar.CreateLineEdit);
    }

    /// <summary>
    /// Registers a toggle option definition.
    /// </summary>
    /// <param name="option">Toggle option definition.</param>
    /// <returns>Registered toggle option wrapper.</returns>
    public RegisteredToggleOption AddToggle(ToggleOptionDefinition option)
    {
        OptionValidator.ValidateTab(option.Tab);
        OptionValidator.ValidateLabel(option.Label, "Toggle");
        return Register(option, _toggles, OptionRegistrar.CreateToggle);
    }

    // -- Helpers --

    /// <summary>Shared flow: null‑check, assign ID, create, replace, store.</summary>
    /// <typeparam name="TDef">Option definition type.</typeparam>
    /// <typeparam name="TReg">Registered wrapper type.</typeparam>
    /// <param name="option">Option definition to register.</param>
    /// <param name="storage">Destination storage map.</param>
    /// <param name="factory">Factory used to create wrapper from definition.</param>
    /// <returns>Registered wrapper instance.</returns>
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

    /// <summary>
    /// Gets or creates a stable id for a tab/label pair.
    /// </summary>
    /// <param name="tab">Option tab name.</param>
    /// <param name="label">Option label key.</param>
    /// <returns>Stable option id.</returns>
    private int GetOrCreateId(string tab, string label)
    {
        string key = CreateOptionKey(tab, label);

        // Reuse existing stable id when tab/label key was previously registered.
        if (_optionIds.TryGetValue(key, out int id))
            return id;

        id = ++_nextId;
        _optionIds[key] = id;
        return id;
    }

    /// <summary>
    /// Builds normalized dictionary key for a tab/label combination.
    /// </summary>
    /// <param name="tab">Option tab name.</param>
    /// <param name="label">Option label key.</param>
    /// <returns>Composite key.</returns>
    private static string CreateOptionKey(string tab, string label)
    {
        return $"{tab.Trim()}::{label.Trim()}";
    }

    /// <summary>Clears any existing registration for the given ID across all types.</summary>
    /// <param name="id">Stable option registration id to clear.</param>
    private void ReplaceExisting(int id)
    {
        _sliders.Remove(id);
        _dropdowns.Remove(id);
        _lineEdits.Remove(id);
        _toggles.Remove(id);
    }
}
