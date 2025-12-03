using Godot;
using System;

namespace __TEMPLATE__.UI;

public class OptionsGameplay(Options options)
{
    public event Action<float> OnMouseSensitivityChanged;

    private ResourceOptions _options;

    public void Initialize()
    {
        GetOptions();
        SetupDifficulty();
        SetupSensitivity();
    }

    private void GetOptions()
    {
        _options = Game.Options.GetOptions();
    }

    private void SetupDifficulty()
    {
        OptionButton difficultyBtn = options.GetNode<OptionButton>("%Difficulty");
        difficultyBtn.Select((int)_options.Difficulty);
        difficultyBtn.ItemSelected += OnDifficultyItemSelected;
    }

    private void SetupSensitivity()
    {
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
