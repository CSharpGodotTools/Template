using Godot;
using System;

namespace __TEMPLATE__.UI;

public class OptionsGameplay
{
    #region Events
    public event Action<float> OnMouseSensitivityChanged;
    #endregion

    #region Fields
    private ResourceOptions _options;
    private Button _gameplayBtn;
    private readonly Options options;
    #endregion

    public OptionsGameplay(Options options, Button gameplayButton)
    {
        this.options = options;
        _gameplayBtn = gameplayButton;

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
        difficultyBtn.FocusNeighborLeft = _gameplayBtn.GetPath();
        difficultyBtn.Select((int)_options.Difficulty);
        difficultyBtn.ItemSelected += OnDifficultyItemSelected;
    }

    private void SetupSensitivity()
    {
        HSlider sensitivity = options.GetNode<HSlider>("%Sensitivity");
        sensitivity.FocusNeighborLeft = _gameplayBtn.GetPath();
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
