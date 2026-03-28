using Godot;
using System;

namespace __TEMPLATE__.Ui;

internal sealed class MaxFpsFeedbackController : IDisposable
{
    private readonly HSlider _slider;
    private readonly LineEdit _lineEdit;
    private readonly Godot.Range.ValueChangedEventHandler _onValueChanged;

    public MaxFpsFeedbackController(HSlider slider, LineEdit lineEdit)
    {
        _slider = slider ?? throw new ArgumentNullException(nameof(slider));
        _lineEdit = lineEdit ?? throw new ArgumentNullException(nameof(lineEdit));

        _onValueChanged = value => _lineEdit.Text = Mathf.RoundToInt((float)value).ToString();

        _lineEdit.Text = Mathf.RoundToInt((float)_slider.Value).ToString();
        _slider.ValueChanged += _onValueChanged;
    }

    public void Dispose()
    {
        if (GodotObject.IsInstanceValid(_slider))
            _slider.ValueChanged -= _onValueChanged;
    }
}
