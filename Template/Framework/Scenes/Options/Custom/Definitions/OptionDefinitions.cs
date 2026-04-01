using Godot;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Convenience factory for creating option definitions with delegates instead of one class per option.
/// </summary>
public static class OptionDefinitions
{
    /// <summary>
    /// Creates a right-control definition using delegate callbacks.
    /// </summary>
    /// <param name="tab">Target tab name.</param>
    /// <param name="targetLabel">Target option label in that tab.</param>
    /// <param name="name">Right control name.</param>
    /// <param name="createControl">Factory that creates the right-side control.</param>
    /// <param name="onAttached">Optional callback invoked after attachment.</param>
    /// <param name="onDetaching">Optional callback invoked before detaching.</param>
    /// <returns>Configured right-control definition.</returns>
    public static OptionRightControlDefinition RightControl(
        string tab,
        string targetLabel,
        string name,
        Func<Control, Control> createControl,
        Action<Control, Control>? onAttached = null,
        Action<Control, Control>? onDetaching = null)
    {
        ArgumentNullException.ThrowIfNull(createControl);
        return new OptionRightControlDefinition(tab, targetLabel, name, createControl, onAttached, onDetaching);
    }

    /// <summary>
    /// Creates a slider option definition backed by delegates.
    /// </summary>
    /// <param name="tab">Target tab name.</param>
    /// <param name="label">Display label key.</param>
    /// <param name="minValue">Minimum slider value.</param>
    /// <param name="maxValue">Maximum slider value.</param>
    /// <param name="getValue">Getter delegate for current value.</param>
    /// <param name="setValue">Setter delegate for updated value.</param>
    /// <param name="step">Slider increment step.</param>
    /// <param name="saveKey">Optional explicit persistence key.</param>
    /// <param name="defaultValue">Optional explicit default value.</param>
    /// <returns>Configured slider option definition.</returns>
    public static SliderOptionDefinition Slider(
        string tab,
        string label,
        double minValue,
        double maxValue,
        Func<float> getValue,
        Action<float> setValue,
        double step = 1.0,
        string? saveKey = null,
        float? defaultValue = null)
    {
        ArgumentNullException.ThrowIfNull(getValue);
        ArgumentNullException.ThrowIfNull(setValue);

        float resolvedDefault = defaultValue ?? getValue();
        return new DelegateSliderOptionDefinition(tab, label, minValue, maxValue, step, saveKey, resolvedDefault, getValue, setValue);
    }

    /// <summary>
    /// Creates a dropdown option definition backed by delegates.
    /// </summary>
    /// <param name="tab">Target tab name.</param>
    /// <param name="label">Display label key.</param>
    /// <param name="items">Dropdown items in index order.</param>
    /// <param name="getValue">Getter delegate for selected index.</param>
    /// <param name="setValue">Setter delegate for selected index.</param>
    /// <param name="controlMinWidth">Minimum dropdown control width.</param>
    /// <param name="saveKey">Optional explicit persistence key.</param>
    /// <param name="defaultValue">Optional explicit default selected index.</param>
    /// <returns>Configured dropdown option definition.</returns>
    public static DropdownOptionDefinition Dropdown(
        string tab,
        string label,
        IReadOnlyList<string> items,
        Func<int> getValue,
        Action<int> setValue,
        float controlMinWidth = DropdownOptionDefinition.DefaultControlMinWidth,
        string? saveKey = null,
        int? defaultValue = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(getValue);
        ArgumentNullException.ThrowIfNull(setValue);

        int resolvedDefault = defaultValue ?? getValue();
        return new DelegateDropdownOptionDefinition(tab, label, [.. items], controlMinWidth, saveKey, resolvedDefault, getValue, setValue);
    }

    /// <summary>
    /// Creates a line-edit option definition backed by delegates.
    /// </summary>
    /// <param name="tab">Target tab name.</param>
    /// <param name="label">Display label key.</param>
    /// <param name="getValue">Getter delegate for current text.</param>
    /// <param name="setValue">Setter delegate for current text.</param>
    /// <param name="placeholder">Placeholder text shown when empty.</param>
    /// <param name="saveKey">Optional explicit persistence key.</param>
    /// <param name="defaultValue">Optional explicit default text.</param>
    /// <returns>Configured line-edit option definition.</returns>
    public static LineEditOptionDefinition LineEdit(
        string tab,
        string label,
        Func<string> getValue,
        Action<string> setValue,
        string placeholder = "",
        string? saveKey = null,
        string? defaultValue = null)
    {
        ArgumentNullException.ThrowIfNull(getValue);
        ArgumentNullException.ThrowIfNull(setValue);

        string resolvedDefault = defaultValue ?? getValue() ?? string.Empty;
        return new DelegateLineEditOptionDefinition(tab, label, placeholder, saveKey, resolvedDefault, getValue, setValue);
    }

    /// <summary>
    /// Creates a toggle option definition backed by delegates.
    /// </summary>
    /// <param name="tab">Target tab name.</param>
    /// <param name="label">Display label key.</param>
    /// <param name="getValue">Getter delegate for current state.</param>
    /// <param name="setValue">Setter delegate for current state.</param>
    /// <param name="saveKey">Optional explicit persistence key.</param>
    /// <param name="defaultValue">Optional explicit default state.</param>
    /// <returns>Configured toggle option definition.</returns>
    public static ToggleOptionDefinition Toggle(
        string tab,
        string label,
        Func<bool> getValue,
        Action<bool> setValue,
        string? saveKey = null,
        bool? defaultValue = null)
    {
        ArgumentNullException.ThrowIfNull(getValue);
        ArgumentNullException.ThrowIfNull(setValue);

        bool resolvedDefault = defaultValue ?? getValue();
        return new DelegateToggleOptionDefinition(tab, label, saveKey, resolvedDefault, getValue, setValue);
    }

    /// <summary>
    /// Delegate-backed slider option definition.
    /// </summary>
    /// <param name="tab">Target tab name.</param>
    /// <param name="label">Display label key.</param>
    /// <param name="minValue">Minimum slider value.</param>
    /// <param name="maxValue">Maximum slider value.</param>
    /// <param name="step">Slider step increment.</param>
    /// <param name="saveKey">Optional explicit persistence key.</param>
    /// <param name="defaultValue">Default slider value used when none is stored.</param>
    /// <param name="getValue">Delegate that reads current slider value.</param>
    /// <param name="setValue">Delegate that persists slider value.</param>
    private sealed class DelegateSliderOptionDefinition(
        string tab,
        string label,
        double minValue,
        double maxValue,
        double step,
        string? saveKey,
        float defaultValue,
        Func<float> getValue,
        Action<float> setValue) : SliderOptionDefinition
    {
        public override string Tab { get; } = tab;
        public override string Label { get; } = label;
        public override string? SaveKey { get; } = saveKey;
        public override double MinValue { get; } = minValue;
        public override double MaxValue { get; } = maxValue;
        public override double Step { get; } = step;
        public override float DefaultValue { get; } = defaultValue;

        /// <summary>
        /// Reads current slider value from delegate.
        /// </summary>
        /// <returns>Current slider value.</returns>
        public override float GetValue() => getValue();

        /// <summary>
        /// Writes slider value using delegate.
        /// </summary>
        /// <param name="value">Slider value to persist.</param>
        public override void SetValue(float value) => setValue(value);
    }

    /// <summary>
    /// Delegate-backed dropdown option definition.
    /// </summary>
    /// <param name="tab">Target tab name.</param>
    /// <param name="label">Display label key.</param>
    /// <param name="items">Dropdown item labels.</param>
    /// <param name="controlMinWidth">Minimum control width in pixels.</param>
    /// <param name="saveKey">Optional explicit persistence key.</param>
    /// <param name="defaultValue">Default selected index used when none is stored.</param>
    /// <param name="getValue">Delegate that reads current selected index.</param>
    /// <param name="setValue">Delegate that persists selected index.</param>
    private sealed class DelegateDropdownOptionDefinition(
        string tab,
        string label,
        IReadOnlyList<string> items,
        float controlMinWidth,
        string? saveKey,
        int defaultValue,
        Func<int> getValue,
        Action<int> setValue) : DropdownOptionDefinition
    {
        public override string Tab { get; } = tab;
        public override string Label { get; } = label;
        public override string? SaveKey { get; } = saveKey;
        public override IReadOnlyList<string> Items { get; } = items;
        public override float ControlMinWidth { get; } = controlMinWidth;
        public override int DefaultValue { get; } = defaultValue;

        /// <summary>
        /// Reads current dropdown index from delegate.
        /// </summary>
        /// <returns>Current selected index.</returns>
        public override int GetValue() => getValue();

        /// <summary>
        /// Writes dropdown index using delegate.
        /// </summary>
        /// <param name="value">Selected index to persist.</param>
        public override void SetValue(int value) => setValue(value);
    }

    /// <summary>
    /// Delegate-backed line-edit option definition.
    /// </summary>
    /// <param name="tab">Target tab name.</param>
    /// <param name="label">Display label key.</param>
    /// <param name="placeholder">Placeholder text displayed when empty.</param>
    /// <param name="saveKey">Optional explicit persistence key.</param>
    /// <param name="defaultValue">Default text used when none is stored.</param>
    /// <param name="getValue">Delegate that reads current text value.</param>
    /// <param name="setValue">Delegate that persists text value.</param>
    private sealed class DelegateLineEditOptionDefinition(
        string tab,
        string label,
        string placeholder,
        string? saveKey,
        string defaultValue,
        Func<string> getValue,
        Action<string> setValue) : LineEditOptionDefinition
    {
        public override string Tab { get; } = tab;
        public override string Label { get; } = label;
        public override string? SaveKey { get; } = saveKey;
        public override string Placeholder { get; } = placeholder ?? string.Empty;
        public override string DefaultValue { get; } = defaultValue ?? string.Empty;

        /// <summary>
        /// Reads current text from delegate.
        /// </summary>
        /// <returns>Current text value.</returns>
        public override string GetValue() => getValue() ?? string.Empty;

        /// <summary>
        /// Writes text value using delegate.
        /// </summary>
        /// <param name="value">Text value to persist.</param>
        public override void SetValue(string value) => setValue(value ?? string.Empty);
    }

    /// <summary>
    /// Delegate-backed toggle option definition.
    /// </summary>
    /// <param name="tab">Target tab name.</param>
    /// <param name="label">Display label key.</param>
    /// <param name="saveKey">Optional explicit persistence key.</param>
    /// <param name="defaultValue">Default toggle value used when none is stored.</param>
    /// <param name="getValue">Delegate that reads current toggle value.</param>
    /// <param name="setValue">Delegate that persists toggle value.</param>
    private sealed class DelegateToggleOptionDefinition(
        string tab,
        string label,
        string? saveKey,
        bool defaultValue,
        Func<bool> getValue,
        Action<bool> setValue) : ToggleOptionDefinition
    {
        public override string Tab { get; } = tab;
        public override string Label { get; } = label;
        public override string? SaveKey { get; } = saveKey;
        public override bool DefaultValue { get; } = defaultValue;

        /// <summary>
        /// Reads current toggle value from delegate.
        /// </summary>
        /// <returns>Current toggle state.</returns>
        public override bool GetValue() => getValue();

        /// <summary>
        /// Writes toggle value using delegate.
        /// </summary>
        /// <param name="value">Toggle state to persist.</param>
        public override void SetValue(bool value) => setValue(value);
    }
}
