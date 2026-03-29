using Godot;
using GodotUtils;
using System;
using VSyncMode = Godot.DisplayServer.VSyncMode;

namespace __TEMPLATE__.Ui;

internal sealed class OptionsDisplaySettingsComponent
{
    private const int DefaultWindowMode = (int)WindowMode.Windowed;
    private const int DefaultVSyncMode = (int)VSyncMode.Enabled;
    private const int DefaultWindowWidth = 0;
    private const int DefaultWindowHeight = 0;
    private const int DefaultMaxFps = 60;

    private readonly AutoloadsFramework _autoloads;
    private readonly OptionsValueStoreComponent _valueStore;
    private readonly OptionsVisualSettingsComponent _visualSettings;

    public event Action<WindowMode>? WindowModeChanged;

    public OptionsDisplaySettingsComponent(
        AutoloadsFramework autoloads,
        OptionsValueStoreComponent valueStore,
        OptionsVisualSettingsComponent visualSettings)
    {
        _autoloads = autoloads;
        _valueStore = valueStore;
        _visualSettings = visualSettings;
    }

    public void ApplyStartupSettings()
    {
        ApplyWindowMode();
        ApplyVSyncMode();
        ApplyWindowSize();
        ApplyMaxFps();
        _visualSettings.ApplyStartupSettings();
    }

    public void SetLanguage(int language) => _visualSettings.SetLanguage(language);
    public void SetQualityPreset(int qualityPreset) => _visualSettings.SetQualityPreset(qualityPreset);
    public void SetAntialiasing(int antialiasing) => _visualSettings.SetAntialiasing(antialiasing);

    public void ToggleFullscreen()
    {
        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed)
            SetWindowMode(WindowMode.Fullscreen);
        else
            SetWindowMode(WindowMode.Windowed);
    }

    public void SetWindowMode(WindowMode windowMode)
    {
        WindowMode clamped = CoerceWindowMode((int)windowMode);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowMode, (int)clamped);
        ApplyWindowMode();

        if (clamped == WindowMode.Windowed)
            ApplyWindowSize();

        WindowModeChanged?.Invoke(clamped);
    }

    public void SetWindowSize(int width, int height)
    {
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowWidth, Math.Max(0, width));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowHeight, Math.Max(0, height));

        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed)
            ApplyWindowSize();
    }

    public void SetVSyncMode(VSyncMode vsyncMode)
    {
        VSyncMode clamped = CoerceVSyncMode((int)vsyncMode);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.VSyncMode, (int)clamped);
        ApplyVSyncMode();
        ApplyMaxFps();
    }

    public void SetMaxFps(int maxFps)
    {
        _valueStore.SetInt(FrameworkOptionsSaveKeys.MaxFps, Math.Max(0, maxFps));
        ApplyMaxFps();
    }

    public void PersistWindowSizeFromRuntime()
    {
        if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Windowed)
            return;

        Vector2I size = DisplayServer.WindowGetSize();
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowWidth, Math.Max(0, size.X));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowHeight, Math.Max(0, size.Y));
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

    private void ApplyVSyncMode() => DisplayServer.WindowSetVsyncMode(GetVSyncMode());

    private void ApplyWindowSize()
    {
        if (Engine.IsEmbeddedInEditor())
            return;

        Vector2I windowSize = new(GetWindowWidth(), GetWindowHeight());
        if (windowSize == Vector2I.Zero)
            return;

        _autoloads.GetTree().Root.Size = windowSize;

        Vector2I screenSize = DisplayServer.ScreenGetSize();
        Vector2I winSize = _autoloads.GetTree().Root.Size;
        DisplayServer.WindowSetPosition(screenSize / 2 - winSize / 2);
    }

    private void ApplyMaxFps()
    {
        if (DisplayServer.WindowGetVsyncMode() == VSyncMode.Disabled)
            Engine.MaxFps = GetMaxFps();
    }

    private WindowMode GetWindowMode()
    {
        WindowMode windowMode = CoerceWindowMode(_valueStore.GetInt(FrameworkOptionsSaveKeys.WindowMode, DefaultWindowMode));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowMode, (int)windowMode);
        return windowMode;
    }

    private VSyncMode GetVSyncMode()
    {
        VSyncMode mode = CoerceVSyncMode(_valueStore.GetInt(FrameworkOptionsSaveKeys.VSyncMode, DefaultVSyncMode));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.VSyncMode, (int)mode);
        return mode;
    }

    private int GetWindowWidth()
    {
        int width = Math.Max(0, _valueStore.GetInt(FrameworkOptionsSaveKeys.WindowWidth, DefaultWindowWidth));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowWidth, width);
        return width;
    }

    private int GetWindowHeight()
    {
        int height = Math.Max(0, _valueStore.GetInt(FrameworkOptionsSaveKeys.WindowHeight, DefaultWindowHeight));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowHeight, height);
        return height;
    }

    private int GetMaxFps()
    {
        int fps = Math.Max(0, _valueStore.GetInt(FrameworkOptionsSaveKeys.MaxFps, DefaultMaxFps));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.MaxFps, fps);
        return fps;
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
}
