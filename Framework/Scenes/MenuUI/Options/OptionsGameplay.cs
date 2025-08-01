using Godot;
using System;

namespace __TEMPLATE__.UI;

public partial class OptionsGameplay : Control
{
    public event Action<float> OnMouseSensitivityChanged;

    private ResourceOptions _options;

    public override void _Ready()
    {
        _options = GetNode<OptionsManager>(AutoloadPaths.OptionsManager).Options;
        SetupDifficulty();
        SetupMouseSensitivity();
    }

    private void SetupDifficulty()
    {
        GetNode<OptionButton>("%Difficulty").Select((int)_options.Difficulty);
    }

    private void SetupMouseSensitivity()
    {
        GetNode<HSlider>("%Sensitivity").Value = _options.MouseSensitivity;
    }

    private void _OnDifficultyItemSelected(int index)
    {
        _options.Difficulty = (Difficulty)index;
    }

    private void _OnSensitivityValueChanged(float value)
    {
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
