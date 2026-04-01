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

    /// <summary>
    /// Initializes a composed options manager and applies startup settings.
    /// </summary>
    /// <param name="hotkeysService">Hotkeys lifecycle service.</param>
    /// <param name="settingDispatcher">Settings read/write dispatcher.</param>
    /// <param name="displaySettings">Display settings component.</param>
    /// <param name="registration">Option registration component.</param>
    /// <param name="rightControls">Right-control registration component.</param>
    /// <param name="lifecycle">Lifecycle component for persistence hooks.</param>
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

    /// <summary>
    /// Raised when window mode changes.
    /// </summary>
    public event Action<WindowMode> WindowModeChanged
    {
        add => _displaySettings.WindowModeChanged += value;
        remove => _displaySettings.WindowModeChanged -= value;
    }

    /// <summary>
    /// Raised when a slider option is registered.
    /// </summary>
    internal event Action<RegisteredSliderOption> SliderOptionRegistered
    {
        add => _registration.SliderOptionRegistered += value;
        remove => _registration.SliderOptionRegistered -= value;
    }

    /// <summary>
    /// Raised when a dropdown option is registered.
    /// </summary>
    internal event Action<RegisteredDropdownOption> DropdownOptionRegistered
    {
        add => _registration.DropdownOptionRegistered += value;
        remove => _registration.DropdownOptionRegistered -= value;
    }

    /// <summary>
    /// Raised when a line-edit option is registered.
    /// </summary>
    internal event Action<RegisteredLineEditOption> LineEditOptionRegistered
    {
        add => _registration.LineEditOptionRegistered += value;
        remove => _registration.LineEditOptionRegistered -= value;
    }

    /// <summary>
    /// Raised when a toggle option is registered.
    /// </summary>
    internal event Action<RegisteredToggleOption> ToggleOptionRegistered
    {
        add => _registration.ToggleOptionRegistered += value;
        remove => _registration.ToggleOptionRegistered -= value;
    }

    /// <summary>
    /// Raised when a right-side option control is registered.
    /// </summary>
    internal event Action<RegisteredRightControl> RightControlRegistered
    {
        add => _rightControls.RightControlRegistered += value;
        remove => _rightControls.RightControlRegistered -= value;
    }

    /// <summary>
    /// Processes per-frame options updates.
    /// </summary>
    public void Update()
    {
        // Toggle fullscreen when the bound input action is pressed.
        if (Input.IsActionJustPressed(InputActions.Fullscreen))
            _displaySettings.ToggleFullscreen();
    }

    /// <summary>
    /// Disposes owned lifecycle resources.
    /// </summary>
    public void Dispose()
    {
        _lifecycle.Dispose();
    }

    /// <summary>
    /// Gets currently selected options tab.
    /// </summary>
    /// <returns>Current tab name.</returns>
    public string GetCurrentTab()
    {
        return _currentOptionsTab;
    }

    /// <summary>
    /// Sets current options tab when a non-empty value is provided.
    /// </summary>
    /// <param name="tab">Tab name to select.</param>
    public void SetCurrentTab(string tab)
    {
        // Ignore empty tab names to preserve the current selection.
        if (!string.IsNullOrWhiteSpace(tab))
            _currentOptionsTab = tab;
    }

    /// <summary>
    /// Gets strongly typed settings access for this manager.
    /// </summary>
    public OptionsSettings Settings => _settings;

    /// <summary>
    /// Gets the hotkeys resource currently managed by options.
    /// </summary>
    /// <returns>Current hotkeys resource.</returns>
    public ResourceHotkeys GetHotkeys()
    {
        return _hotkeysService.Hotkeys;
    }

    /// <summary>
    /// Restores hotkeys to their default input map values.
    /// </summary>
    public void ResetHotkeys()
    {
        _hotkeysService.ResetToDefaults();
    }

    /// <summary>
    /// Registers a custom option definition.
    /// </summary>
    /// <param name="option">Option definition to add.</param>
    public void AddOption(OptionDefinition option)
    {
        _registration.AddOption(option);
    }

    /// <summary>
    /// Registers a right-side control definition for an option row.
    /// </summary>
    /// <param name="definition">Right control definition.</param>
    public void AddRightControl(OptionRightControlDefinition definition)
    {
        _rightControls.AddRightControl(definition);
    }

    /// <summary>
    /// Gets registered slider options.
    /// </summary>
    /// <returns>Slider option sequence.</returns>
    internal IEnumerable<RegisteredSliderOption> GetSliderOptions()
    {
        return _registration.GetSliderOptions();
    }

    /// <summary>
    /// Gets registered dropdown options.
    /// </summary>
    /// <returns>Dropdown option sequence.</returns>
    internal IEnumerable<RegisteredDropdownOption> GetDropdownOptions()
    {
        return _registration.GetDropdownOptions();
    }

    /// <summary>
    /// Gets registered line-edit options.
    /// </summary>
    /// <returns>Line-edit option sequence.</returns>
    internal IEnumerable<RegisteredLineEditOption> GetLineEditOptions()
    {
        return _registration.GetLineEditOptions();
    }

    /// <summary>
    /// Gets registered toggle options.
    /// </summary>
    /// <returns>Toggle option sequence.</returns>
    internal IEnumerable<RegisteredToggleOption> GetToggleOptions()
    {
        return _registration.GetToggleOptions();
    }

    /// <summary>
    /// Gets registered right-side controls.
    /// </summary>
    /// <returns>Right control sequence.</returns>
    internal IEnumerable<RegisteredRightControl> GetRightControls()
    {
        return _rightControls.GetRightControls();
    }
}
