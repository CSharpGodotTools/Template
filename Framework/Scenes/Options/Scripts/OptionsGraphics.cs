using Godot;
using System;

namespace __TEMPLATE__.UI;

public class OptionsGraphics
{
    #region Events
    public event Action<int> AntialiasingChanged;
    #endregion

    #region Fields
    private ResourceOptions _options;
    private OptionButton _antialiasing;
    private readonly Options options;
    #endregion

    public OptionsGraphics(Options options, Button graphicsBtn)
    {
        this.options = options;

        GetOptions();
        SetupQualityPreset(graphicsBtn);
        SetupAntialiasing(graphicsBtn);
    }

    private void GetOptions()
    {
        _options = Game.Options.GetOptions();
    }

    private void SetupQualityPreset(Button graphicsBtn)
    {
        OptionButton optionBtnQualityPreset = options.GetNode<OptionButton>("%QualityMode");
        optionBtnQualityPreset.FocusNeighborLeft = graphicsBtn.GetPath();
        optionBtnQualityPreset.Select((int)_options.QualityPreset);
        optionBtnQualityPreset.ItemSelected += OnQualityModeItemSelected;
    }

    private void SetupAntialiasing(Button graphicsBtn)
    {
        _antialiasing = options.GetNode<OptionButton>("%Antialiasing");
        _antialiasing.FocusNeighborLeft = graphicsBtn.GetPath();
        _antialiasing.Select(_options.Antialiasing);
        _antialiasing.ItemSelected += OnAntialiasingItemSelected;
    }

    private void OnQualityModeItemSelected(long index)
    {
        _options.QualityPreset = (QualityPreset)index;
    }

    private void OnAntialiasingItemSelected(long index)
    {
        _options.Antialiasing = (int)index;
        AntialiasingChanged?.Invoke((int)index);
    }
}

public enum QualityPreset
{
    Low,
    Medium,
    High
}
