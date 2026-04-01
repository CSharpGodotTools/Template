using Godot;
using GodotUtils;
using System;
using VSyncMode = Godot.DisplayServer.VSyncMode;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Applies display and visual options and persists normalized values.
/// </summary>
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

    /// <summary>
    /// Raised when window mode changes after applying a mode update.
    /// </summary>
    public event Action<WindowMode>? WindowModeChanged;

    /// <summary>
    /// Initializes display settings dependencies.
    /// </summary>
    /// <param name="autoloads">Autoload access for scene tree/window references.</param>
    /// <param name="valueStore">Persistent options value storage.</param>
    /// <param name="visualSettings">Visual settings component.</param>
    public OptionsDisplaySettingsComponent(
        AutoloadsFramework autoloads,
        OptionsValueStoreComponent valueStore,
        OptionsVisualSettingsComponent visualSettings)
    {
        _autoloads = autoloads;
        _valueStore = valueStore;
        _visualSettings = visualSettings;
    }

    /// <summary>
    /// Applies persisted startup display and visual settings.
    /// </summary>
    public void ApplyStartupSettings()
    {
        ApplyWindowMode();
        ApplyVSyncMode();
        ApplyWindowSize();
        ApplyMaxFps();
        _visualSettings.ApplyStartupSettings();
    }

    /// <summary>
    /// Applies language selection through visual settings.
    /// </summary>
    /// <param name="language">Selected language index.</param>
    public void SetLanguage(int language) => _visualSettings.SetLanguage(language);

    /// <summary>
    /// Applies quality preset through visual settings.
    /// </summary>
    /// <param name="qualityPreset">Selected quality preset index.</param>
    public void SetQualityPreset(int qualityPreset) => _visualSettings.SetQualityPreset(qualityPreset);

    /// <summary>
    /// Applies anti-aliasing selection through visual settings.
    /// </summary>
    /// <param name="antialiasing">Selected anti-aliasing index.</param>
    public void SetAntialiasing(int antialiasing) => _visualSettings.SetAntialiasing(antialiasing);

    /// <summary>
    /// Toggles between windowed and fullscreen presentation modes.
    /// </summary>
    public void ToggleFullscreen()
    {
        // Toggle between windowed and fullscreen modes.
        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed)
            SetWindowMode(WindowMode.Fullscreen);
        else
            SetWindowMode(WindowMode.Windowed);
    }

    /// <summary>
    /// Stores and applies a new window mode.
    /// </summary>
    /// <param name="windowMode">Requested window mode.</param>
    public void SetWindowMode(WindowMode windowMode)
    {
        WindowMode clamped = CoerceWindowMode((int)windowMode);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowMode, (int)clamped);
        ApplyWindowMode();

        // Reapply stored window size whenever returning to windowed mode.
        if (clamped == WindowMode.Windowed)
            ApplyWindowSize();

        WindowModeChanged?.Invoke(clamped);
    }

    /// <summary>
    /// Stores and applies window size when running in windowed mode.
    /// </summary>
    /// <param name="width">Requested width in pixels.</param>
    /// <param name="height">Requested height in pixels.</param>
    public void SetWindowSize(int width, int height)
    {
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowWidth, Math.Max(0, width));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowHeight, Math.Max(0, height));

        // Apply size changes immediately only in windowed mode.
        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed)
            ApplyWindowSize();
    }

    /// <summary>
    /// Stores and applies a new VSync mode.
    /// </summary>
    /// <param name="vsyncMode">Requested VSync mode.</param>
    public void SetVSyncMode(VSyncMode vsyncMode)
    {
        VSyncMode clamped = CoerceVSyncMode((int)vsyncMode);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.VSyncMode, (int)clamped);
        ApplyVSyncMode();
        ApplyMaxFps();
    }

    /// <summary>
    /// Stores and applies max FPS cap.
    /// </summary>
    /// <param name="maxFps">Requested max FPS value.</param>
    public void SetMaxFps(int maxFps)
    {
        _valueStore.SetInt(FrameworkOptionsSaveKeys.MaxFps, Math.Max(0, maxFps));
        ApplyMaxFps();
    }

    /// <summary>
    /// Persists current runtime window size when in windowed mode.
    /// </summary>
    public void PersistWindowSizeFromRuntime()
    {
        // Persist runtime size only while currently windowed.
        if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Windowed)
            return;

        Vector2I size = DisplayServer.WindowGetSize();
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowWidth, Math.Max(0, size.X));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowHeight, Math.Max(0, size.Y));
    }

    /// <summary>
    /// Applies persisted window mode to the host window.
    /// </summary>
    private void ApplyWindowMode()
    {
        // Skip window mode updates when embedded in editor.
        if (Engine.IsEmbeddedInEditor())
            return;


        // Map project mode values to Godot window mode variants.
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

    /// <summary>
    /// Applies persisted VSync mode to the host window.
    /// </summary>
    private void ApplyVSyncMode() => DisplayServer.WindowSetVsyncMode(GetVSyncMode());

    /// <summary>
    /// Applies persisted window size and centers the window on the primary screen.
    /// </summary>
    private void ApplyWindowSize()
    {
        // Skip window size updates when embedded in editor.
        if (Engine.IsEmbeddedInEditor())
            return;

        Vector2I windowSize = new(GetWindowWidth(), GetWindowHeight());

        // Ignore size apply when stored size is unset.
        if (windowSize == Vector2I.Zero)
            return;

        _autoloads.GetTree().Root.Size = windowSize;


        // Recenter after resize to keep window placement consistent.
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        Vector2I winSize = _autoloads.GetTree().Root.Size;
        DisplayServer.WindowSetPosition(screenSize / 2 - winSize / 2);
    }

    /// <summary>
    /// Applies FPS cap when VSync is disabled.
    /// </summary>
    private void ApplyMaxFps()
    {
        // Apply frame cap only when VSync is disabled.
        if (DisplayServer.WindowGetVsyncMode() == VSyncMode.Disabled)
            Engine.MaxFps = GetMaxFps();
    }

    /// <summary>
    /// Reads and normalizes persisted window mode.
    /// </summary>
    /// <returns>Valid window mode value.</returns>
    private WindowMode GetWindowMode()
    {
        WindowMode windowMode = CoerceWindowMode(_valueStore.GetInt(FrameworkOptionsSaveKeys.WindowMode, DefaultWindowMode));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowMode, (int)windowMode);
        return windowMode;
    }

    /// <summary>
    /// Reads and normalizes persisted VSync mode.
    /// </summary>
    /// <returns>Valid VSync mode value.</returns>
    private VSyncMode GetVSyncMode()
    {
        VSyncMode mode = CoerceVSyncMode(_valueStore.GetInt(FrameworkOptionsSaveKeys.VSyncMode, DefaultVSyncMode));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.VSyncMode, (int)mode);
        return mode;
    }

    /// <summary>
    /// Reads and normalizes persisted window width.
    /// </summary>
    /// <returns>Non-negative window width.</returns>
    private int GetWindowWidth()
    {
        int width = Math.Max(0, _valueStore.GetInt(FrameworkOptionsSaveKeys.WindowWidth, DefaultWindowWidth));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowWidth, width);
        return width;
    }

    /// <summary>
    /// Reads and normalizes persisted window height.
    /// </summary>
    /// <returns>Non-negative window height.</returns>
    private int GetWindowHeight()
    {
        int height = Math.Max(0, _valueStore.GetInt(FrameworkOptionsSaveKeys.WindowHeight, DefaultWindowHeight));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.WindowHeight, height);
        return height;
    }

    /// <summary>
    /// Reads and normalizes persisted max FPS cap.
    /// </summary>
    /// <returns>Non-negative FPS value.</returns>
    private int GetMaxFps()
    {
        int fps = Math.Max(0, _valueStore.GetInt(FrameworkOptionsSaveKeys.MaxFps, DefaultMaxFps));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.MaxFps, fps);
        return fps;
    }

    /// <summary>
    /// Converts raw stored integer to a supported <see cref="WindowMode"/> value.
    /// </summary>
    /// <param name="raw">Raw mode value from storage.</param>
    /// <returns>Coerced window mode.</returns>
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

    /// <summary>
    /// Converts raw stored integer to a supported <see cref="VSyncMode"/> value.
    /// </summary>
    /// <param name="raw">Raw mode value from storage.</param>
    /// <returns>Coerced VSync mode.</returns>
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
