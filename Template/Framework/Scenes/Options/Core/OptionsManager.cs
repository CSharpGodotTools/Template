using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

// Autoload
/// <summary>
/// Coordinates options systems and exposes a stable API to the rest of the project.
/// </summary>
public class OptionsManager : IDisposable, IOptionsService
{
    private readonly OptionsHotkeysService _hotkeysService;
    private readonly OptionsSettingDispatcherComponent _settingDispatcher;
    private readonly OptionsDisplaySettingsComponent _displaySettings;
    private readonly OptionsRegistrationComponent _registration;
    private readonly OptionsRightControlRegistryComponent _rightControls;
    private readonly OptionsLifecycleComponent _lifecycle;
    private readonly OptionsSettings _settings;

    private string _currentOptionsTab = FrameworkOptionsTabs.Input;

    internal OptionsManager(
        OptionsHotkeysService hotkeysService,
        OptionsSettingDispatcherComponent settingDispatcher,
        OptionsDisplaySettingsComponent displaySettings,
        OptionsRegistrationComponent registration,
        OptionsRightControlRegistryComponent rightControls,
        OptionsLifecycleComponent lifecycle)
    {
        _hotkeysService = hotkeysService;
        _settingDispatcher = settingDispatcher;
        _displaySettings = displaySettings;
        _registration = registration;
        _rightControls = rightControls;
        _lifecycle = lifecycle;

        _settings = new OptionsSettings(
            _settingDispatcher.ReadOptionInt,
            _settingDispatcher.ReadOptionFloat,
            _settingDispatcher.ReadOptionString,
            _settingDispatcher.ReadOptionBool,
            _settingDispatcher.SetIntSetting,
            _settingDispatcher.SetFloatSetting,
            _settingDispatcher.SetStringSetting,
            _settingDispatcher.SetBoolSetting);

        _hotkeysService.Initialize();
        _displaySettings.ApplyStartupSettings();
    }

    public event Action<WindowMode> WindowModeChanged
    {
        add => _displaySettings.WindowModeChanged += value;
        remove => _displaySettings.WindowModeChanged -= value;
    }

    internal event Action<RegisteredSliderOption> SliderOptionRegistered
    {
        add => _registration.SliderOptionRegistered += value;
        remove => _registration.SliderOptionRegistered -= value;
    }

    internal event Action<RegisteredDropdownOption> DropdownOptionRegistered
    {
        add => _registration.DropdownOptionRegistered += value;
        remove => _registration.DropdownOptionRegistered -= value;
    }

    internal event Action<RegisteredLineEditOption> LineEditOptionRegistered
    {
        add => _registration.LineEditOptionRegistered += value;
        remove => _registration.LineEditOptionRegistered -= value;
    }

    internal event Action<RegisteredToggleOption> ToggleOptionRegistered
    {
        add => _registration.ToggleOptionRegistered += value;
        remove => _registration.ToggleOptionRegistered -= value;
    }

    internal event Action<RegisteredRightControl> RightControlRegistered
    {
        add => _rightControls.RightControlRegistered += value;
        remove => _rightControls.RightControlRegistered -= value;
    }

    public void Update()
    {
        if (Input.IsActionJustPressed(InputActions.Fullscreen))
            _displaySettings.ToggleFullscreen();
    }

    public void Dispose()
    {
        _lifecycle.Dispose();
    }

    public string GetCurrentTab()
    {
        return _currentOptionsTab;
    }

    public void SetCurrentTab(string tab)
    {
        if (!string.IsNullOrWhiteSpace(tab))
            _currentOptionsTab = tab;
    }

    public OptionsSettings Settings => _settings;

    public ResourceHotkeys GetHotkeys()
    {
        return _hotkeysService.Hotkeys;
    }

    public void ResetHotkeys()
    {
        _hotkeysService.ResetToDefaults();
    }

    public void AddOption(OptionDefinition option)
    {
        _registration.AddOption(option);
    }

    public void AddRightControl(OptionRightControlDefinition definition)
    {
        _rightControls.AddRightControl(definition);
    }

    internal IEnumerable<RegisteredSliderOption> GetSliderOptions()
    {
        return _registration.GetSliderOptions();
    }

    internal IEnumerable<RegisteredDropdownOption> GetDropdownOptions()
    {
        return _registration.GetDropdownOptions();
    }

    internal IEnumerable<RegisteredLineEditOption> GetLineEditOptions()
    {
        return _registration.GetLineEditOptions();
    }

    internal IEnumerable<RegisteredToggleOption> GetToggleOptions()
    {
        return _registration.GetToggleOptions();
    }

    internal IEnumerable<RegisteredRightControl> GetRightControls()
    {
        return _rightControls.GetRightControls();
    }
}
