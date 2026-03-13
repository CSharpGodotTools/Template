using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Ui;

// Autoload
/// <summary>
/// Coordinates options systems and exposes a stable API to the rest of the project.
/// </summary>
public partial class OptionsManager : IDisposable
{
    // Events
    public event Action<WindowMode> WindowModeChanged = null!;
    internal event Action<RegisteredSliderOption> SliderOptionRegistered = null!;
    internal event Action<RegisteredDropdownOption> DropdownOptionRegistered = null!;
    internal event Action<RegisteredLineEditOption> LineEditOptionRegistered = null!;
    internal event Action<RegisteredToggleOption> ToggleOptionRegistered = null!;

    // Fields
    private readonly OptionsSettingsStore _settingsStore = new();
    private readonly OptionsHotkeysService _hotkeysService = new();
    private readonly OptionsCustomRegistry _customRegistry;

    private ResourceOptions _options;
    private string _currentOptionsTab = "General";
    private AutoloadsFramework _autoloads = null!;

    public OptionsManager(AutoloadsFramework autoloads)
    {
        SetupAutoloads(autoloads);

        _options = _settingsStore.Load();
        _customRegistry = new OptionsCustomRegistry(_options);

        _hotkeysService.Initialize();

        SetWindowMode();
        SetVSyncMode();
        SetWinSize();
        SetMaxFPS();
        SetLanguage();
        SetAntialiasing();
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
        _currentOptionsTab = tab;
    }

    public ResourceOptions GetOptions()
    {
        return _options;
    }

    public ResourceOptions Settings => _options;

    public ResourceHotkeys GetHotkeys()
    {
        return _hotkeysService.Hotkeys;
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

    /// <summary>
    /// Registers a custom slider option class.
    /// </summary>
    public void AddSlider(SliderOptionDefinition option)
    {
        RegisteredSliderOption slider = _customRegistry.AddSlider(option);
        SliderOptionRegistered?.Invoke(slider);
    }

    /// <summary>
    /// Registers a custom dropdown option class.
    /// </summary>
    public void AddDropdown(DropdownOptionDefinition option)
    {
        RegisteredDropdownOption dropdown = _customRegistry.AddDropdown(option);
        DropdownOptionRegistered?.Invoke(dropdown);
    }

    /// <summary>
    /// Registers a custom line edit option class.
    /// </summary>
    public void AddLineEdit(LineEditOptionDefinition option)
    {
        RegisteredLineEditOption lineEdit = _customRegistry.AddLineEdit(option);
        LineEditOptionRegistered?.Invoke(lineEdit);
    }

    /// <summary>
    /// Registers a custom boolean toggle option class.
    /// </summary>
    public void AddToggle(ToggleOptionDefinition option)
    {
        RegisteredToggleOption toggle = _customRegistry.AddToggle(option);
        ToggleOptionRegistered?.Invoke(toggle);
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

    private void SetWindowMode()
    {
        switch (_options.WindowMode)
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
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
        _options.WindowMode = WindowMode.Fullscreen;
        WindowModeChanged?.Invoke(WindowMode.Fullscreen);
    }

    private void SwitchToWindow()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        _options.WindowMode = WindowMode.Windowed;
        WindowModeChanged?.Invoke(WindowMode.Windowed);
    }

    private void SetVSyncMode()
    {
        DisplayServer.WindowSetVsyncMode(_options.VSyncMode);
    }

    private void SetWinSize()
    {
        if (Engine.IsEmbeddedInEditor())
            return;

        Vector2I windowSize = new(_options.WindowWidth, _options.WindowHeight);

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

    private void SetMaxFPS()
    {
        if (DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Disabled)
        {
            Engine.MaxFps = _options.MaxFPS;
        }
    }

    private void SetLanguage()
    {
        TranslationServer.SetLocale(
        _options.Language.ToString()[..2].ToLower());
    }

    private void SetAntialiasing()
    {
        // Set both 2D and 3D settings to the same value.
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_2d", _options.Antialiasing);
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_3d", _options.Antialiasing);
    }

    private void OnWindowResized()
    {
        if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Windowed)
            return;

        Vector2I size = DisplayServer.WindowGetSize();
        _options.WindowWidth = size.X;
        _options.WindowHeight = size.Y;
    }

    private Task SaveSettingsOnQuit()
    {
        SaveOptions();
        SaveHotkeys();

        return Task.CompletedTask;
    }
}
