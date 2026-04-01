using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Disposable helper that owns a slider row and its event subscription.
/// </summary>
/// <param name="row">Row container that owns the slider control.</param>
/// <param name="slider">Slider control bound to option state.</param>
/// <param name="onValueChanged">Signal handler used to persist slider value changes.</param>
internal sealed class SliderBinding(HBoxContainer row, HSlider slider, Godot.Range.ValueChangedEventHandler onValueChanged) : IDisposable
{
    private const float ControlMinWidth = 250f;

    private readonly HBoxContainer _row = row;
    private readonly HSlider _slider = slider;
    private readonly Godot.Range.ValueChangedEventHandler _onValueChanged = onValueChanged;

    /// <summary>
    /// Builds the slider control, syncs its initial value, and wires events.
    /// </summary>
    /// <param name="tabContainer">Tab container that will own the row.</param>
    /// <param name="navButton">Navigation button used for focus wiring.</param>
    /// <param name="sliderOption">Registered slider option metadata.</param>
    /// <returns>Disposable binding that owns row and signal subscription.</returns>
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

    /// <summary>
    /// Unsubscribes events and frees the generated row.
    /// </summary>
    public void Dispose()
    {
        // Unsubscribe only while the slider instance is still valid.
        if (GodotObject.IsInstanceValid(_slider))
            _slider.ValueChanged -= _onValueChanged;

        // Free row only while the row instance is still valid.
        if (GodotObject.IsInstanceValid(_row))
            _row.QueueFree();
    }
}
