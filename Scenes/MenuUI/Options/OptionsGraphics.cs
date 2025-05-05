using Godot;
using System;
using Environment = Godot.Environment;

namespace Template.UI;

public partial class OptionsGraphics : Control
{
    public event Action<int> OnAntialiasingChanged;

    [Export] private OptionsManager _optionsManager;
    private ResourceOptions _options;
    private OptionButton _antialiasing;

    public override void _Ready()
    {
        _options = _optionsManager.Options;
        SetupQualityPreset();
        SetupAntialiasing();
        SetupWorldEnvironmentSettings();
    }

    private void SetupWorldEnvironmentSettings()
    {
        AddNewSetting("GLOW", 
            (checkbox) =>
                checkbox.ButtonPressed = _options.Glow,
            (pressed) =>
                _options.Glow = pressed, 
            (environment, pressed) =>
                environment.GlowEnabled = pressed);

        AddNewSetting("AMBIENT_OCCLUSION",
            (checkbox) =>
                checkbox.ButtonPressed = _options.AmbientOcclusion,
            (pressed) =>
                _options.AmbientOcclusion = pressed,
            (environment, pressed) =>
                environment.SsaoEnabled = pressed);

        AddNewSetting("INDIRECT_LIGHTING",
            (checkbox) =>
                checkbox.ButtonPressed = _options.IndirectLighting,
            (pressed) =>
                _options.IndirectLighting = pressed,
            (environment, pressed) =>
                environment.SsilEnabled = pressed);

        AddNewSetting("REFLECTIONS",
            (checkbox) =>
                checkbox.ButtonPressed = _options.Reflections,
            (pressed) =>
                _options.Reflections = pressed,
            (environment, pressed) =>
                environment.SsrEnabled = pressed);
    }

    private void AddNewSetting(string name, Action<CheckBox> setPressed, Action<bool> saveOption, Action<Environment, bool> applyInGame)
    {
        HBoxContainer hbox = new();

        hbox.AddChild(new Label
        {
            Text = name,
            CustomMinimumSize = new Vector2(200, 0)
        });

        CheckBox checkBox = new();
        setPressed(checkBox);

        checkBox.Pressed += () =>
        {
            saveOption(checkBox.ButtonPressed);

            UIPopupMenu popupMenu = Services.Get<UIPopupMenu>();

            if (popupMenu == null)
            {
                return;
            }

            WorldEnvironment worldEnvironment = popupMenu.WorldEnvironment;

            if (worldEnvironment == null)
            {
                return;
            }

            applyInGame(worldEnvironment.Environment, checkBox.ButtonPressed);
        };

        hbox.AddChild(checkBox);

        AddChild(hbox);
    }

    private void SetupQualityPreset()
    {
        OptionButton optionBtnQualityPreset = GetNode<OptionButton>("%QualityMode");
        optionBtnQualityPreset.Select((int)_options.QualityPreset);
    }

    private void SetupAntialiasing()
    {
        _antialiasing = GetNode<OptionButton>("%Antialiasing");
        _antialiasing.Select(_options.Antialiasing);
    }

    private void _on_quality_mode_item_selected(int index)
    {
        // todo: setup quality preset and change other settings

        _options.QualityPreset = (QualityPreset)index;
    }

    private void _on_antialiasing_item_selected(int index)
    {
        _options.Antialiasing = index;
        OnAntialiasingChanged?.Invoke(index);
    }
}

public enum QualityPreset
{
    Low,
    Medium,
    High
}

