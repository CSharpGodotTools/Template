using Godot;
using System;

namespace GodotUtils.UI;

public class OptionsGraphics(Options options)
{
    public event Action<int> AntialiasingChanged;

    private ResourceOptions _options;
    private OptionButton _antialiasing;

    public void Initialize()
    {
        _options = OptionsManager.GetOptions();

        SetupQualityPreset();
        SetupAntialiasing();
    }

    private void SetupQualityPreset()
    {
        OptionButton optionBtnQualityPreset = options.GetNode<OptionButton>("%QualityMode");
        optionBtnQualityPreset.Select((int)_options.QualityPreset);
        optionBtnQualityPreset.ItemSelected += OnQualityModeItemSelected;
    }

    private void SetupAntialiasing()
    {
        _antialiasing = options.GetNode<OptionButton>("%Antialiasing");
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
