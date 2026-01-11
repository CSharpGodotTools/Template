using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.UI;

public class OptionsGameplay : IDisposable
{
    #region Events
    public event Action<float> OnMouseSensitivityChanged;
    #endregion

    #region Fields
    private ResourceOptions _options;
    private Button _gameplayBtn;
    private readonly Options options;
    private readonly OptionButton _difficultyBtn;
    private readonly HSlider _sensitivitySlider;
    #endregion

    public OptionsGameplay(Options options, Button gameplayButton)
    {
        this.options = options;
        _gameplayBtn = gameplayButton;
        _difficultyBtn = options.GetNode<OptionButton>("%Difficulty");
        _sensitivitySlider = options.GetNode<HSlider>("%Sensitivity");

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
        _difficultyBtn.FocusNeighborLeft = _gameplayBtn.GetPath();
        _difficultyBtn.Select((int)_options.Difficulty);
        _difficultyBtn.ItemSelected += OnDifficultyItemSelected;
    }

    private void SetupSensitivity()
    {
        _sensitivitySlider.FocusNeighborLeft = _gameplayBtn.GetPath();
        _sensitivitySlider.Value = _options.MouseSensitivity;
        _sensitivitySlider.ValueChanged += OnSensitivityValueChanged;
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

    public void Dispose()
    {
        _difficultyBtn.ItemSelected -= OnDifficultyItemSelected;
        _sensitivitySlider.ValueChanged -= OnSensitivityValueChanged;
    }
}

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}
