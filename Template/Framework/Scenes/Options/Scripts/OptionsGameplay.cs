using Godot;
using GodotUtils;
using System;

namespace Framework.UI;

public class OptionsGameplay : IDisposable
{
    // Events
    public event Action<float> OnMouseSensitivityChanged;

    // Fields
    private ResourceOptions _resourceOptions;
    private readonly Button _gameplayBtn;
    private readonly Options _options;
    private readonly OptionButton _difficultyBtn;
    private readonly HSlider _sensitivitySlider;

    public OptionsGameplay(Options options, Button gameplayButton)
    {
        this._options = options;
        _gameplayBtn = gameplayButton;
        _difficultyBtn = options.GetNode<OptionButton>("%Difficulty");
        _sensitivitySlider = options.GetNode<HSlider>("%Sensitivity");

        GetOptions();
        SetupDifficulty();
        SetupSensitivity();
    }

    private void GetOptions()
    {
        _resourceOptions = GameFramework.Options.GetOptions();
    }

    private void SetupDifficulty()
    {
        _difficultyBtn.FocusNeighborLeft = _gameplayBtn.GetPath();
        _difficultyBtn.Select((int)_resourceOptions.Difficulty);
        _difficultyBtn.ItemSelected += OnDifficultyItemSelected;
    }

    private void SetupSensitivity()
    {
        _sensitivitySlider.FocusNeighborLeft = _gameplayBtn.GetPath();
        _sensitivitySlider.Value = _resourceOptions.MouseSensitivity;
        _sensitivitySlider.ValueChanged += OnSensitivityValueChanged;
    }

    private void OnDifficultyItemSelected(long index)
    {
        _resourceOptions.Difficulty = (Difficulty)index;
    }

    private void OnSensitivityValueChanged(double v)
    {
        float value = (float)v;
        _resourceOptions.MouseSensitivity = value;
        OnMouseSensitivityChanged?.Invoke(value);
    }

    public void Dispose()
    {
        _difficultyBtn.ItemSelected -= OnDifficultyItemSelected;
        _sensitivitySlider.ValueChanged -= OnSensitivityValueChanged;
        GC.SuppressFinalize(this);
    }
}

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}
