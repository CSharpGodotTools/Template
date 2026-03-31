using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Disposable helper that owns a slider row and its event subscription.
/// </summary>
internal sealed class SliderBinding(HBoxContainer row, HSlider slider, Godot.Range.ValueChangedEventHandler onValueChanged) : IDisposable
{
    private const float ControlMinWidth = 250f;

    private readonly HBoxContainer _row = row;
    private readonly HSlider _slider = slider;
    private readonly Godot.Range.ValueChangedEventHandler _onValueChanged = onValueChanged;

    /// <summary>
    /// Builds the slider control, syncs its initial value, and wires events.
    /// </summary>
    internal static SliderBinding Create(
        VBoxContainer tabContainer, Button navButton,
        RegisteredSliderOption sliderOption)
    {
        SliderOptionDefinition definition = sliderOption.Definition;

        HSlider slider = new()
        {
            CustomMinimumSize = new Vector2(ControlMinWidth, 0),
            MinValue = definition.MinValue,
            MaxValue = definition.MaxValue,
            Step = definition.Step
        };

        string label = string.IsNullOrWhiteSpace(definition.Label)
            ? $"SLIDER_{sliderOption.Id}"
            : definition.Label;

        HBoxContainer row = OptionRowFactory.Create(
            tabContainer, navButton, $"CustomSlider_{sliderOption.Id}", label, slider);

        // Clamp and push the initial value into both the definition and control
        float clamped = Mathf.Clamp(
            sliderOption.GetValue(), (float)definition.MinValue, (float)definition.MaxValue);
        sliderOption.SetValue(clamped);
        slider.Value = clamped;

        void onValueChanged(double v) => sliderOption.SetValue((float)v);
        slider.ValueChanged += onValueChanged;

        return new SliderBinding(row, slider, onValueChanged);
    }

    public void Dispose()
    {
        if (GodotObject.IsInstanceValid(_slider))
            _slider.ValueChanged -= _onValueChanged;

        if (GodotObject.IsInstanceValid(_row))
            _row.QueueFree();
    }
}
