using Godot;
using System;

namespace Framework.UI;

public class OptionsGraphics : IDisposable
{
    // Events
    public event Action<int> AntialiasingChanged;

    // Fields
    private ResourceOptions _resourceOptions;
    private OptionButton _antialiasing;
    private readonly Options _options;
    private readonly OptionButton _optionBtnQualityPreset;

    public OptionsGraphics(Options options, Button graphicsBtn)
    {
        _options = options;
        _optionBtnQualityPreset = options.GetNode<OptionButton>("%QualityMode");

        GetOptions();
        SetupQualityPreset(graphicsBtn);
        SetupAntialiasing(graphicsBtn);
    }

    public void Dispose()
    {
        _optionBtnQualityPreset.ItemSelected -= OnQualityModeItemSelected;
        _antialiasing.ItemSelected -= OnAntialiasingItemSelected;
        GC.SuppressFinalize(this);
    }

    private void GetOptions()
    {
        _resourceOptions = GameFramework.Options.GetOptions();
    }

    private void SetupQualityPreset(Button graphicsBtn)
    {
        _optionBtnQualityPreset.FocusNeighborLeft = graphicsBtn.GetPath();
        _optionBtnQualityPreset.Select((int)_resourceOptions.QualityPreset);
        _optionBtnQualityPreset.ItemSelected += OnQualityModeItemSelected;
    }

    private void SetupAntialiasing(Button graphicsBtn)
    {
        _antialiasing = _options.GetNode<OptionButton>("%Antialiasing");
        _antialiasing.FocusNeighborLeft = graphicsBtn.GetPath();
        _antialiasing.Select(_resourceOptions.Antialiasing);
        _antialiasing.ItemSelected += OnAntialiasingItemSelected;
    }

    private void OnQualityModeItemSelected(long index)
    {
        _resourceOptions.QualityPreset = (QualityPreset)index;
    }

    private void OnAntialiasingItemSelected(long index)
    {
        _resourceOptions.Antialiasing = (int)index;
        AntialiasingChanged?.Invoke((int)index);
    }
}

public enum QualityPreset
{
    Low,
    Medium,
    High
}
