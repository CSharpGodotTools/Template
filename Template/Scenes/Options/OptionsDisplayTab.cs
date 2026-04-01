using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using VSyncMode = Godot.DisplayServer.VSyncMode;

namespace __TEMPLATE__.Ui;

public sealed class OptionsDisplayTab : IOptionsTabRegistrar
{
    private const int MaxFpsLimit = 240;
    private const string WindowSizeLabel = "WINDOW_SIZE";
    private const string MaxFpsLabel = "MAX_FPS";
    private readonly Dictionary<Control, IDisposable> _rightControlDisposables = [];

    private static readonly Vector2I[] _baseWindowSizes =
    [
        new(800, 600),
        new(1024, 768),
        new(1280, 720),
        new(1280, 960),
        new(1366, 768),
        new(1600, 900),
        new(1600, 1200),
        new(1920, 1080),
        new(2560, 1080),
        new(2560, 1440),
        new(3440, 1440),
        new(3840, 2160),
    ];

    public string TabName => OptionsTabs.Display;

    public void Register(IOptionsService optionsService)
    {
        optionsService.AddOption(
            OptionDefinitions.Dropdown(
                tab: TabName,
                label: "WINDOW_MODE",
                items: ["WINDOWED", "BORDERLESS", "FULLSCREEN"],
                getValue: () => optionsService.Settings.WindowMode,
                setValue: value => optionsService.Settings.WindowMode = value,
                saveKey: OptionsSaveKeys.WindowMode,
                defaultValue: (int)WindowMode.Windowed));

        List<Vector2I> windowSizes = BuildWindowSizes();
        optionsService.AddOption(
            OptionDefinitions.Dropdown(
                tab: TabName,
                label: WindowSizeLabel,
                items: [.. windowSizes.Select(size => $"{size.X} x {size.Y}")],
                getValue: () => GetCurrentWindowSizeIndex(optionsService, windowSizes),
                setValue: value => ApplyWindowSize(optionsService, windowSizes, value),
                saveKey: OptionsSaveKeys.WindowSize,
                defaultValue: GetCurrentWindowSizeIndex(optionsService, windowSizes)));

        optionsService.AddRightControl(
            OptionDefinitions.RightControl(
                tab: TabName,
                targetLabel: WindowSizeLabel,
                name: "WindowSizeCustomButton",
                createControl: _ => CreateWindowSizeCustomControl(optionsService),
                onDetaching: (control, _) => DetachRightControl(control)));

        optionsService.AddOption(
            OptionDefinitions.Dropdown(
                tab: TabName,
                label: "VSYNC_MODE",
                items: ["DISABLED", "ENABLED", "ADAPTIVE"],
                getValue: () => optionsService.Settings.VSyncMode,
                setValue: value => optionsService.Settings.VSyncMode = value,
                saveKey: OptionsSaveKeys.VSyncMode,
                defaultValue: (int)VSyncMode.Enabled));

        optionsService.AddOption(
            OptionDefinitions.Slider(
                tab: TabName,
                label: MaxFpsLabel,
                minValue: 0,
                maxValue: MaxFpsLimit,
                getValue: () => optionsService.Settings.MaxFPS,
                setValue: value => optionsService.Settings.MaxFPS = value,
                step: 1.0,
                saveKey: OptionsSaveKeys.MaxFps,
                defaultValue: 60));

        optionsService.AddRightControl(
            OptionDefinitions.RightControl(
                tab: TabName,
                targetLabel: MaxFpsLabel,
                name: "MaxFpsFeedback",
                createControl: anchorControl => CreateMaxFpsFeedbackControl(anchorControl, optionsService),
                onDetaching: (control, _) => DetachRightControl(control)));
    }

    /// <summary>
    /// Creates the custom window-size button and attaches its popup controller lifecycle.
    /// </summary>
    /// <param name="optionsService">Options service used by the custom size controller.</param>
    /// <returns>Configured custom-size button control.</returns>
    private Button CreateWindowSizeCustomControl(IOptionsService optionsService)
    {
        Button button = new()
        {
            Name = "WindowSizeCustomButton",
            Text = "Custom",
            CustomMinimumSize = new Vector2(90, 0)
        };

        TrackRightControl(button, new WindowSizeCustomButtonController(button, optionsService));
        return button;
    }

    /// <summary>
    /// Creates readonly FPS feedback text bound to the max-FPS slider when available.
    /// </summary>
    /// <param name="anchorControl">Primary option control associated with this right-side feedback control.</param>
    /// <param name="optionsService">Options service providing initial max-FPS value.</param>
    /// <returns>Configured feedback line edit.</returns>
    private LineEdit CreateMaxFpsFeedbackControl(Control anchorControl, IOptionsService optionsService)
    {
        LineEdit feedback = new()
        {
            Name = "MaxFpsFeedback",
            Editable = false,
            FocusMode = Control.FocusModeEnum.None,
            CustomMinimumSize = new Vector2(90, 0),
            Text = optionsService.Settings.MaxFPS.ToString()
        };

        // Bind live slider feedback only when anchor control is a slider.
        if (anchorControl is HSlider slider)
            TrackRightControl(feedback, new MaxFpsFeedbackController(slider, feedback));

        return feedback;
    }

    /// <summary>
    /// Tracks disposable resources associated with a right-side control and replaces older bindings.
    /// </summary>
    /// <param name="control">Right-side control instance.</param>
    /// <param name="disposable">Disposable lifecycle object bound to the control.</param>
    private void TrackRightControl(Control control, IDisposable disposable)
    {
        // Dispose previous disposable when replacing a tracked control binding.
        if (_rightControlDisposables.TryGetValue(control, out IDisposable? existing))
            existing.Dispose();

        _rightControlDisposables[control] = disposable;
    }

    /// <summary>
    /// Disposes and removes a tracked right-side control binding.
    /// </summary>
    /// <param name="control">Control whose disposable binding should be removed.</param>
    private void DetachRightControl(Control control)
    {
        // Ignore detach requests for controls without tracked disposables.
        if (!_rightControlDisposables.TryGetValue(control, out IDisposable? disposable))
            return;

        disposable.Dispose();
        _rightControlDisposables.Remove(control);
    }

    /// <summary>
    /// Applies a selected window size preset to persisted option settings.
    /// </summary>
    /// <param name="optionsService">Options service whose settings are updated.</param>
    /// <param name="windowSizes">Available window-size presets.</param>
    /// <param name="index">Selected preset index.</param>
    private static void ApplyWindowSize(IOptionsService optionsService, List<Vector2I> windowSizes, int index)
    {
        // Ignore invalid dropdown indices.
        if ((uint)index >= (uint)windowSizes.Count)
            return;

        Vector2I size = windowSizes[index];
        optionsService.Settings.WindowWidth = size.X;
        optionsService.Settings.WindowHeight = size.Y;
    }

    /// <summary>
    /// Resolves the dropdown index matching the currently configured or active window size.
    /// </summary>
    /// <param name="optionsService">Options service containing configured dimensions.</param>
    /// <param name="windowSizes">Available window-size presets.</param>
    /// <returns>Index of the matching preset, or 0 when no match exists.</returns>
    private static int GetCurrentWindowSizeIndex(IOptionsService optionsService, List<Vector2I> windowSizes)
    {
        Vector2I configuredSize = new(
            optionsService.Settings.WindowWidth,
            optionsService.Settings.WindowHeight);
        Vector2I targetSize = configuredSize == Vector2I.Zero ? DisplayServer.WindowGetSize() : configuredSize;

        for (int i = 0; i < windowSizes.Count; i++)
        {
            // Return index when configured size matches a known preset.
            if (windowSizes[i] == targetSize)
                return i;
        }

        return 0;
    }

    /// <summary>
    /// Builds and sorts the window-size preset list, including the current size when needed.
    /// </summary>
    /// <returns>Display-size presets suitable for the options dropdown.</returns>
    private static List<Vector2I> BuildWindowSizes()
    {
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        Vector2I current = DisplayServer.WindowGetSize();

        List<Vector2I> sizes = [];
        for (int index = 0; index < _baseWindowSizes.Length; index++)
        {
            Vector2I size = _baseWindowSizes[index];

            // Keep only presets that fit the current screen bounds.
            if (IsWithinScreen(size, screenSize))
                sizes.Add(size);
        }

        // Preserve current window size when it is not part of presets.
        if (current != Vector2I.Zero && !sizes.Contains(current))
            sizes.Add(current);

        // Guarantee at least one dropdown entry.
        if (sizes.Count == 0)
            sizes.Add(current == Vector2I.Zero ? new Vector2I(1280, 720) : current);

        sizes.Sort(static (left, right) =>
        {
            int leftArea = left.X * left.Y;
            int rightArea = right.X * right.Y;
            return leftArea.CompareTo(rightArea);
        });

        return sizes;
    }

    /// <summary>
    /// Returns whether a candidate size fits within the current screen bounds.
    /// </summary>
    /// <param name="size">Candidate window size.</param>
    /// <param name="screenSize">Screen size used as bounds.</param>
    /// <returns><see langword="true"/> when the candidate fits the screen.</returns>
    private static bool IsWithinScreen(Vector2I size, Vector2I screenSize)
    {
        // Treat unknown screen size as unbounded.
        if (screenSize == Vector2I.Zero)
            return true;

        return size.X <= screenSize.X && size.Y <= screenSize.Y;
    }
}
