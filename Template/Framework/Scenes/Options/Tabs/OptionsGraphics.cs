using Godot;
using System;

namespace __TEMPLATE__.Ui;

public class OptionsGraphics : IDisposable
{
    // Events
    public event Action<int> AntialiasingChanged = null!;

    // Fields
    private readonly OptionsManager _optionsManager;
    private OptionButton _antialiasing = null!;
    private readonly Options _options;
    private readonly OptionButton _optionBtnQualityPreset;

    public OptionsGraphics(Options options, Button graphicsBtn, OptionsManager optionsManager)
    {
        _options = options;
        _optionBtnQualityPreset = options.GetNode<OptionButton>("%QualityMode");
        _optionsManager = optionsManager;

        SetupQualityPreset(graphicsBtn);
        SetupAntialiasing(graphicsBtn);
    }

    public void Dispose()
    {
        _optionBtnQualityPreset.ItemSelected -= OnQualityModeItemSelected;
        _antialiasing.ItemSelected -= OnAntialiasingItemSelected;
    }

    private void SetupQualityPreset(Button graphicsBtn)
    {
        _optionBtnQualityPreset.FocusNeighborLeft = graphicsBtn.GetPath();
        _optionBtnQualityPreset.Select((int)_optionsManager.Settings.QualityPreset);
        _optionBtnQualityPreset.ItemSelected += OnQualityModeItemSelected;
    }

    private void SetupAntialiasing(Button graphicsBtn)
    {
        _antialiasing = _options.GetNode<OptionButton>("%Antialiasing");
        _antialiasing.FocusNeighborLeft = graphicsBtn.GetPath();
        _antialiasing.Select(_optionsManager.Settings.Antialiasing);
        _antialiasing.ItemSelected += OnAntialiasingItemSelected;
    }

    private void OnQualityModeItemSelected(long index)
    {
        _optionsManager.SetQualityPreset((QualityPreset)index);
    }

    private void OnAntialiasingItemSelected(long index)
    {
        _optionsManager.SetAntialiasing((int)index);
        AntialiasingChanged?.Invoke((int)index);
    }
}

public enum QualityPreset
{
    Low,
    Medium,
    High
}
