using Godot;
using System;

namespace __TEMPLATE__.UI;

public class OptionsGraphics : IDisposable
{
    #region Events
    public event Action<int> AntialiasingChanged;
    #endregion

    #region Fields
    private ResourceOptions _options;
    private OptionButton _antialiasing;
    private readonly Options options;
    private readonly OptionButton _optionBtnQualityPreset;
    #endregion

    public OptionsGraphics(Options options, Button graphicsBtn)
    {
        this.options = options;
        _optionBtnQualityPreset = options.GetNode<OptionButton>("%QualityMode");

        GetOptions();
        SetupQualityPreset(graphicsBtn);
        SetupAntialiasing(graphicsBtn);
    }

    public void Dispose()
    {
        _optionBtnQualityPreset.ItemSelected -= OnQualityModeItemSelected;
        _antialiasing.ItemSelected -= OnAntialiasingItemSelected;
    }

    private void GetOptions()
    {
        _options = Game.Options.GetOptions();
    }

    private void SetupQualityPreset(Button graphicsBtn)
    {
        _optionBtnQualityPreset.FocusNeighborLeft = graphicsBtn.GetPath();
        _optionBtnQualityPreset.Select((int)_options.QualityPreset);
        _optionBtnQualityPreset.ItemSelected += OnQualityModeItemSelected;
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
