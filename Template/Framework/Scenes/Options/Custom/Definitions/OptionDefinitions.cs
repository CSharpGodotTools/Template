using Godot;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Convenience factory for creating option definitions with delegates instead of one class per option.
/// </summary>
public static class OptionDefinitions
{
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

        public override float GetValue() => getValue();
        public override void SetValue(float value) => setValue(value);
    }

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

        public override int GetValue() => getValue();
        public override void SetValue(int value) => setValue(value);
    }

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

        public override string GetValue() => getValue() ?? string.Empty;
        public override void SetValue(string value) => setValue(value ?? string.Empty);
    }

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

        public override bool GetValue() => getValue();
        public override void SetValue(bool value) => setValue(value);
    }
}
