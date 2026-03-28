using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSyncMode = Godot.DisplayServer.VSyncMode;

namespace __TEMPLATE__.Ui;

// Autoload
/// <summary>
/// Coordinates options systems and exposes a stable API to the rest of the project.
/// </summary>
public partial class OptionsManager : IDisposable, IOptionsService
{
    private const int DefaultLanguage = (int)Language.English;
    private const int DefaultAntialiasing = 3;
    private const int DefaultWindowMode = (int)WindowMode.Windowed;
    private const int DefaultVSyncMode = (int)VSyncMode.Enabled;
    private const int DefaultWindowWidth = 0;
    private const int DefaultWindowHeight = 0;
    private const int DefaultMaxFps = 60;

    // Events
    public event Action<WindowMode> WindowModeChanged = null!;
    internal event Action<RegisteredSliderOption> SliderOptionRegistered = null!;
    internal event Action<RegisteredDropdownOption> DropdownOptionRegistered = null!;
    internal event Action<RegisteredLineEditOption> LineEditOptionRegistered = null!;
    internal event Action<RegisteredToggleOption> ToggleOptionRegistered = null!;
    internal event Action<RegisteredRightControl> RightControlRegistered = null!;

    // Fields
    private readonly OptionsSettingsStore _settingsStore = new();
    private readonly OptionsHotkeysService _hotkeysService = new();
    private readonly OptionsCustomRegistry _customRegistry;
    private readonly OptionsSettings _settings;
    private readonly Dictionary<string, int> _rightControlIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, RegisteredRightControl> _rightControls = [];

    private ResourceOptions _options;
    private string _currentOptionsTab = OptionsTabs.General;
    private AutoloadsFramework _autoloads = null!;
    private int _nextRightControlId;

    public OptionsManager(AutoloadsFramework autoloads)
    {
        SetupAutoloads(autoloads);

        _options = _settingsStore.Load();
        _options.Normalize();
        _customRegistry = new OptionsCustomRegistry(_options);
        _settings = new OptionsSettings(
            ReadOptionInt,
            ReadOptionFloat,
            ReadOptionString,
            ReadOptionBool,
            SetIntSetting,
            SetFloatSetting,
            SetStringSetting,
            SetBoolSetting);

        _hotkeysService.Initialize();

        ApplyWindowMode();
        ApplyVSyncMode();
        ApplyWindowSize();
        ApplyMaxFPS();
        ApplyLanguage();
        ApplyAntialiasing();
    }

    private void SetupAutoloads(AutoloadsFramework autoloads)
    {
        _autoloads = autoloads;
        _autoloads.PreQuit += SaveSettingsOnQuit;
        _autoloads.GetTree().Root.SizeChanged += OnWindowResized;
    }

    public void Update()
    {
        if (Input.IsActionJustPressed(InputActions.Fullscreen))
        {
            ToggleFullscreen();
        }
    }

    public void Dispose()
    {
        _autoloads.PreQuit -= SaveSettingsOnQuit;
        _autoloads.GetTree().Root.SizeChanged -= OnWindowResized;
    }

    public string GetCurrentTab()
    {
        return _currentOptionsTab;
    }

    public void SetCurrentTab(string tab)
    {
        _currentOptionsTab = string.IsNullOrWhiteSpace(tab) ? _currentOptionsTab : tab;
    }

    public OptionsSettings Settings => _settings;

    public ResourceHotkeys GetHotkeys()
    {
        return _hotkeysService.Hotkeys;
    }

    private int ReadOptionInt(string key, int defaultValue)
    {
        return _customRegistry.GetOptionInt(key, defaultValue);
    }

    private float ReadOptionFloat(string key, float defaultValue)
    {
        return _customRegistry.GetOptionFloat(key, defaultValue);
    }

    private string ReadOptionString(string key, string defaultValue)
    {
        return _customRegistry.GetOptionString(key, defaultValue);
    }

    private bool ReadOptionBool(string key, bool defaultValue)
    {
        return _customRegistry.GetOptionBool(key, defaultValue);
    }

    private void WriteOptionInt(string key, int value)
    {
        _customRegistry.SetOptionInt(key, value);
    }

    private void WriteOptionFloat(string key, float value)
    {
        _customRegistry.SetOptionFloat(key, value);
    }

    private void WriteOptionString(string key, string value)
    {
        _customRegistry.SetOptionString(key, value);
    }

    private void WriteOptionBool(string key, bool value)
    {
        _customRegistry.SetOptionBool(key, value);
    }

    private void SetIntSetting(string key, int value)
    {
        switch (key)
        {
            case OptionsSaveKeys.Language:
                SetLanguage((Language)value);
                return;
            case OptionsSaveKeys.QualityPreset:
                SetQualityPreset((QualityPreset)value);
                return;
            case OptionsSaveKeys.Antialiasing:
                SetAntialiasing(value);
                return;
            case OptionsSaveKeys.WindowMode:
                SetWindowMode((WindowMode)value);
                return;
            case OptionsSaveKeys.WindowWidth:
                SetWindowSize(value, ReadOptionInt(OptionsSaveKeys.WindowHeight, DefaultWindowHeight));
                return;
            case OptionsSaveKeys.WindowHeight:
                SetWindowSize(ReadOptionInt(OptionsSaveKeys.WindowWidth, DefaultWindowWidth), value);
                return;
            case OptionsSaveKeys.VSyncMode:
                SetVSyncMode((VSyncMode)value);
                return;
            default:
                WriteOptionInt(key, value);
                return;
        }
    }

    private void SetFloatSetting(string key, float value)
    {
        switch (key)
        {
            case OptionsSaveKeys.MusicVolume:
                SetMusicVolume(value);
                return;
            case OptionsSaveKeys.SfxVolume:
                SetSFXVolume(value);
                return;
            case OptionsSaveKeys.MaxFps:
                SetMaxFPS((int)value);
                return;
            default:
                WriteOptionFloat(key, value);
                return;
        }
    }

    private void SetStringSetting(string key, string value)
    {
        WriteOptionString(key, value);
    }

    private void SetBoolSetting(string key, bool value)
    {
        WriteOptionBool(key, value);
    }

    private void SetMusicVolume(float volume)
    {
        float clamped = Math.Clamp(volume, 0f, 100f);
        if (_autoloads.AudioManager is not null)
            _autoloads.AudioManager.ApplyMusicVolumeFromSettings(clamped);

        WriteOptionFloat(OptionsSaveKeys.MusicVolume, clamped);
    }

    private void SetSFXVolume(float volume)
    {
        float clamped = Math.Clamp(volume, 0f, 100f);
        if (_autoloads.AudioManager is not null)
            _autoloads.AudioManager.ApplySfxVolumeFromSettings(clamped);

        WriteOptionFloat(OptionsSaveKeys.SfxVolume, clamped);
    }

    private void SetLanguage(Language language)
    {
        Language clamped = CoerceLanguage((int)language);
        WriteOptionInt(OptionsSaveKeys.Language, (int)clamped);
        ApplyLanguage();
    }

    private void SetQualityPreset(QualityPreset qualityPreset)
    {
        int clamped = Math.Clamp((int)qualityPreset, (int)QualityPreset.Low, (int)QualityPreset.High);
        WriteOptionInt(OptionsSaveKeys.QualityPreset, clamped);
    }

    private void SetAntialiasing(int antialiasing)
    {
        WriteOptionInt(OptionsSaveKeys.Antialiasing, Math.Clamp(antialiasing, 0, 3));
        ApplyAntialiasing();
    }

    private void SetWindowMode(WindowMode windowMode)
    {
        WindowMode clamped = CoerceWindowMode((int)windowMode);
        WriteOptionInt(OptionsSaveKeys.WindowMode, (int)clamped);
        ApplyWindowMode();

        if (clamped == WindowMode.Windowed)
            ApplyWindowSize();

        WindowModeChanged?.Invoke(clamped);
    }

    private void SetWindowSize(int width, int height)
    {
        WriteOptionInt(OptionsSaveKeys.WindowWidth, Math.Max(0, width));
        WriteOptionInt(OptionsSaveKeys.WindowHeight, Math.Max(0, height));

        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed)
            ApplyWindowSize();
    }

    private void SetVSyncMode(VSyncMode vsyncMode)
    {
        VSyncMode clamped = CoerceVSyncMode((int)vsyncMode);
        WriteOptionInt(OptionsSaveKeys.VSyncMode, (int)clamped);
        ApplyVSyncMode();
        ApplyMaxFPS();
    }

    private void SetMaxFPS(int maxFps)
    {
        WriteOptionInt(OptionsSaveKeys.MaxFps, Math.Max(0, maxFps));
        ApplyMaxFPS();
    }

    internal IEnumerable<RegisteredSliderOption> GetSliderOptions()
    {
        return _customRegistry.GetSliderOptions();
    }

    internal IEnumerable<RegisteredDropdownOption> GetDropdownOptions()
    {
        return _customRegistry.GetDropdownOptions();
    }

    internal IEnumerable<RegisteredLineEditOption> GetLineEditOptions()
    {
        return _customRegistry.GetLineEditOptions();
    }

    internal IEnumerable<RegisteredToggleOption> GetToggleOptions()
    {
        return _customRegistry.GetToggleOptions();
    }

    internal IEnumerable<RegisteredRightControl> GetRightControls()
    {
        return _rightControls.Values.OrderBy(control => control.Id);
    }

    /// <summary>
    /// Registers a custom option class.
    /// </summary>
    public void AddOption(OptionDefinition option)
    {
        ArgumentNullException.ThrowIfNull(option);

        switch (option)
        {
            case SliderOptionDefinition slider:
                RegisteredSliderOption registeredSlider = _customRegistry.AddSlider(slider);
                SliderOptionRegistered?.Invoke(registeredSlider);
                break;
            case DropdownOptionDefinition dropdown:
                RegisteredDropdownOption registeredDropdown = _customRegistry.AddDropdown(dropdown);
                DropdownOptionRegistered?.Invoke(registeredDropdown);
                break;
            case LineEditOptionDefinition lineEdit:
                RegisteredLineEditOption registeredLineEdit = _customRegistry.AddLineEdit(lineEdit);
                LineEditOptionRegistered?.Invoke(registeredLineEdit);
                break;
            case ToggleOptionDefinition toggle:
                RegisteredToggleOption registeredToggle = _customRegistry.AddToggle(toggle);
                ToggleOptionRegistered?.Invoke(registeredToggle);
                break;
            default:
                throw new NotSupportedException($"Unsupported option definition type: {option.GetType().Name}");
        }
    }

    public void AddRightControl(OptionRightControlDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        OptionValidator.ValidateTab(definition.Tab);
        OptionValidator.ValidateLabel(definition.TargetLabel, "RightControl target");
        OptionValidator.ValidateLabel(definition.Name, "RightControl name");

        string key = CreateRightControlKey(definition.Tab, definition.TargetLabel, definition.Name);
        if (!_rightControlIds.TryGetValue(key, out int id))
        {
            id = ++_nextRightControlId;
            _rightControlIds[key] = id;
        }

        RegisteredRightControl registered = new(id, definition);
        _rightControls[id] = registered;
        RightControlRegistered?.Invoke(registered);
    }

    private static string CreateRightControlKey(string tab, string targetLabel, string name)
    {
        return $"{tab.Trim()}::{targetLabel.Trim()}::{name.Trim()}";
    }

    private void ToggleFullscreen()
    {
        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed)
        {
            SwitchToFullscreen();
        }
        else
        {
            SwitchToWindow();
        }
    }

    private void SaveOptions()
    {
        _settingsStore.Save(_options);
    }

    private void SaveHotkeys()
    {
        _hotkeysService.Save();
    }

    public void ResetHotkeys()
    {
        _hotkeysService.ResetToDefaults();
    }

    private void ApplyWindowMode()
    {
        if (Engine.IsEmbeddedInEditor())
            return;

        switch (GetWindowMode())
        {
            case WindowMode.Windowed:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                break;
            case WindowMode.Borderless:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                break;
            case WindowMode.Fullscreen:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                break;
        }
    }

    private void SwitchToFullscreen()
    {
        if (Engine.IsEmbeddedInEditor())
            return;

        SetWindowMode(WindowMode.Fullscreen);
    }

    private void SwitchToWindow()
    {
        if (Engine.IsEmbeddedInEditor())
            return;

        SetWindowMode(WindowMode.Windowed);
    }

    private void ApplyVSyncMode()
    {
        DisplayServer.WindowSetVsyncMode(GetVSyncMode());
    }

    private void ApplyWindowSize()
    {
        if (Engine.IsEmbeddedInEditor())
            return;

        Vector2I windowSize = new(GetWindowWidth(), GetWindowHeight());

        if (windowSize != Vector2I.Zero)
        {
            // Use Root.Size to update the RenderingServer viewport synchronously.
            _autoloads.GetTree().Root.Size = windowSize;

            // center window
            Vector2I screenSize = DisplayServer.ScreenGetSize();
            Vector2I winSize = _autoloads.GetTree().Root.Size;
            DisplayServer.WindowSetPosition(screenSize / 2 - winSize / 2);
        }
    }

    private void ApplyMaxFPS()
    {
        if (DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Disabled)
        {
            Engine.MaxFps = GetMaxFps();
        }
    }

    private void ApplyLanguage()
    {
        Language language = GetLanguage();
        TranslationServer.SetLocale(language.ToString()[..2].ToLower());
    }

    private void ApplyAntialiasing()
    {
        int antialiasing = GetAntialiasing();

        // Set both 2D and 3D settings to the same value.
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_2d", antialiasing);
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_3d", antialiasing);
    }

    private void OnWindowResized()
    {
        if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Windowed)
            return;

        Vector2I size = DisplayServer.WindowGetSize();
        WriteOptionInt(OptionsSaveKeys.WindowWidth, Math.Max(0, size.X));
        WriteOptionInt(OptionsSaveKeys.WindowHeight, Math.Max(0, size.Y));
    }

    private Language GetLanguage()
    {
        Language language = CoerceLanguage(ReadOptionInt(OptionsSaveKeys.Language, DefaultLanguage));
        WriteOptionInt(OptionsSaveKeys.Language, (int)language);
        return language;
    }

    private int GetAntialiasing()
    {
        int antialiasing = Math.Clamp(ReadOptionInt(OptionsSaveKeys.Antialiasing, DefaultAntialiasing), 0, 3);
        WriteOptionInt(OptionsSaveKeys.Antialiasing, antialiasing);
        return antialiasing;
    }

    private WindowMode GetWindowMode()
    {
        WindowMode windowMode = CoerceWindowMode(ReadOptionInt(OptionsSaveKeys.WindowMode, DefaultWindowMode));
        WriteOptionInt(OptionsSaveKeys.WindowMode, (int)windowMode);
        return windowMode;
    }

    private VSyncMode GetVSyncMode()
    {
        VSyncMode mode = CoerceVSyncMode(ReadOptionInt(OptionsSaveKeys.VSyncMode, DefaultVSyncMode));
        WriteOptionInt(OptionsSaveKeys.VSyncMode, (int)mode);
        return mode;
    }

    private int GetWindowWidth()
    {
        int width = Math.Max(0, ReadOptionInt(OptionsSaveKeys.WindowWidth, DefaultWindowWidth));
        WriteOptionInt(OptionsSaveKeys.WindowWidth, width);
        return width;
    }

    private int GetWindowHeight()
    {
        int height = Math.Max(0, ReadOptionInt(OptionsSaveKeys.WindowHeight, DefaultWindowHeight));
        WriteOptionInt(OptionsSaveKeys.WindowHeight, height);
        return height;
    }

    private int GetMaxFps()
    {
        int fps = Math.Max(0, ReadOptionInt(OptionsSaveKeys.MaxFps, DefaultMaxFps));
        WriteOptionInt(OptionsSaveKeys.MaxFps, fps);
        return fps;
    }

    private static Language CoerceLanguage(int raw)
    {
        int clamped = Math.Clamp(raw, (int)Language.English, (int)Language.Japanese);
        return (Language)clamped;
    }

    private static WindowMode CoerceWindowMode(int raw)
    {
        return raw switch
        {
            (int)WindowMode.Windowed => WindowMode.Windowed,
            (int)WindowMode.Borderless => WindowMode.Borderless,
            (int)WindowMode.Fullscreen => WindowMode.Fullscreen,
            _ => WindowMode.Windowed,
        };
    }

    private static VSyncMode CoerceVSyncMode(int raw)
    {
        return raw switch
        {
            (int)VSyncMode.Disabled => VSyncMode.Disabled,
            (int)VSyncMode.Enabled => VSyncMode.Enabled,
            (int)VSyncMode.Adaptive => VSyncMode.Adaptive,
            _ => VSyncMode.Enabled,
        };
    }

    private Task SaveSettingsOnQuit()
    {
        SaveOptions();
        SaveHotkeys();

        return Task.CompletedTask;
    }
}
