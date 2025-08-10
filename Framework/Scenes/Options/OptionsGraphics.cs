using Godot;
using GodotUtils;
using GodotUtils.UI;
using System;
using Environment = Godot.Environment;

namespace __TEMPLATE__.UI;

public partial class OptionsGraphics : Control
{
    public event Action<int> AntialiasingChanged;

    private GraphicsSetting[] _graphicsSettings = [];
    private ResourceOptions _options;
    private OptionButton _antialiasing;

    public override void _Ready()
    {
        _options = OptionsManager.GetOptions();

        InitializeSettingsMetadata();
        SetupQualityPreset();
        SetupAntialiasing();
        SetupWorldEnvironmentSettings();
    }

    private void _OnQualityModeItemSelected(int index)
    {
        _options.QualityPreset = (QualityPreset)index;
    }

    private void _OnAntialiasingItemSelected(int index)
    {
        _options.Antialiasing = index;
        AntialiasingChanged?.Invoke(index);
    }

    private void InitializeSettingsMetadata()
    {
        _graphicsSettings =
        [
            CreateGlowSetting(),
            CreateAmbientOcclusionSetting(),
            CreateIndirectLightingSetting(),
            CreateReflectionsSetting()
        ];
    }

    private GraphicsSetting CreateGlowSetting()
    {
        return new()
        {
            Name = "GLOW",
            GetOption = () => _options.Glow,
            SetOption = val => _options.Glow = val,
            ApplyInGame = (env, val) => env.GlowEnabled = val
        };
    }

    private GraphicsSetting CreateAmbientOcclusionSetting()
    {
        return new()
        {
            Name = "AMBIENT_OCCLUSION",
            GetOption = () => _options.AmbientOcclusion,
            SetOption = val => _options.AmbientOcclusion = val,
            ApplyInGame = (env, val) => env.SsaoEnabled = val
        };
    }

    private GraphicsSetting CreateIndirectLightingSetting()
    {
        return new()
        {
            Name = "INDIRECT_LIGHTING",
            GetOption = () => _options.IndirectLighting,
            SetOption = val => _options.IndirectLighting = val,
            ApplyInGame = (env, val) => env.SsilEnabled = val
        };
    }

    private GraphicsSetting CreateReflectionsSetting()
    {
        return new()
        {
            Name = "REFLECTIONS",
            GetOption = () => _options.Reflections,
            SetOption = val => _options.Reflections = val,
            ApplyInGame = (env, val) => env.SsrEnabled = val
        };
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

    private void SetupWorldEnvironmentSettings()
    {
        foreach (GraphicsSetting setting in _graphicsSettings)
        {
            AddSettingControl(setting);
        }
    }

    private void AddSettingControl(GraphicsSetting setting)
    {
        HBoxContainer hbox = new();

        Label label = CreateLabel(setting.Name);
        hbox.AddChild(label);

        CheckBox checkBox = CreateCheckBox(setting);
        setting.CheckBoxControl = checkBox;
        checkBox.Pressed += () => OnSettingPressed(setting);
        hbox.AddChild(checkBox);

        AddChild(hbox);
    }

    private void OnSettingPressed(GraphicsSetting setting)
    {
        bool pressed = setting.CheckBoxControl!.ButtonPressed;

        setting.SetOption(pressed);

        GodotUtils.UI.PopupMenu popupMenu = Services.Get<GodotUtils.UI.PopupMenu>();

        if (popupMenu?.WorldEnvironment == null)
            return;

        setting.ApplyInGame(popupMenu.WorldEnvironment.Environment, pressed);
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(200, 0)
        };
    }

    private static CheckBox CreateCheckBox(GraphicsSetting setting)
    {
        return new CheckBox()
        {
            ButtonPressed = setting.GetOption()
        };
    }

    private class GraphicsSetting
    {
        public required string Name { get; init; }
        public required Func<bool> GetOption { get; init; }
        public required Action<bool> SetOption { get; init; }
        public required Action<Environment, bool> ApplyInGame { get; init; }

        public CheckBox CheckBoxControl;
    }
}

public enum QualityPreset
{
    Low,
    Medium,
    High
}
