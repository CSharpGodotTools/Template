using Godot;
using System;

namespace GodotUtils.UI;

public class OptionsGameplay(Options options)
{
    public event Action<float> OnMouseSensitivityChanged;

    private ResourceOptions _options;

    public void Initialize()
    {
        _options = OptionsManager.GetOptions();

        OptionButton difficultyBtn = options.GetNode<OptionButton>("%Difficulty");
        difficultyBtn.Select((int)_options.Difficulty);
        difficultyBtn.ItemSelected += OnDifficultyItemSelected;

        HSlider sensitivity = options.GetNode<HSlider>("%Sensitivity");
        sensitivity.Value = _options.MouseSensitivity;
        sensitivity.ValueChanged += OnSensitivityValueChanged;
    }

    private void OnDifficultyItemSelected(long index)
    {
        _options.Difficulty = (Difficulty)index;
    }

    private void OnSensitivityValueChanged(double v)
    {
        float value = (float)v;
        _options.MouseSensitivity = value;
        OnMouseSensitivityChanged?.Invoke(value);
    }
}

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}
