using Godot;
using GodotUtils;
using System.Collections.Generic;
using System.Linq;
using VSyncMode = Godot.DisplayServer.VSyncMode;

namespace __TEMPLATE__.Ui;

public sealed class OptionsDisplayTab : IOptionsTabRegistrar
{
    private const int MinResolutionStep = 1;
    private const int MaxResolutionStep = 36;
    private const int MaxFpsLimit = 240;

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
                label: "WINDOW_SIZE",
                items: [.. windowSizes.Select(size => $"{size.X} x {size.Y}")],
                getValue: () => GetCurrentWindowSizeIndex(optionsService, windowSizes),
                setValue: value => ApplyWindowSize(optionsService, windowSizes, value),
                saveKey: OptionsSaveKeys.WindowSize,
                defaultValue: GetCurrentWindowSizeIndex(optionsService, windowSizes)));

        optionsService.AddOption(
            OptionDefinitions.Slider(
                tab: TabName,
                label: "RESOLUTION",
                minValue: MinResolutionStep,
                maxValue: MaxResolutionStep,
                getValue: () => optionsService.Settings.Resolution,
                setValue: value => optionsService.Settings.Resolution = value,
                step: 1.0,
                saveKey: OptionsSaveKeys.Resolution,
                defaultValue: MinResolutionStep));

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
                label: "MAX_FPS",
                minValue: 0,
                maxValue: MaxFpsLimit,
                getValue: () => optionsService.Settings.MaxFPS,
                setValue: value => optionsService.Settings.MaxFPS = value,
                step: 1.0,
                saveKey: OptionsSaveKeys.MaxFps,
                defaultValue: 60));
    }

    private static void ApplyWindowSize(IOptionsService optionsService, IReadOnlyList<Vector2I> windowSizes, int index)
    {
        if ((uint)index >= (uint)windowSizes.Count)
            return;

        Vector2I size = windowSizes[index];
        optionsService.Settings.WindowWidth = size.X;
        optionsService.Settings.WindowHeight = size.Y;
    }

    private static int GetCurrentWindowSizeIndex(IOptionsService optionsService, IReadOnlyList<Vector2I> windowSizes)
    {
        Vector2I configuredSize = new(
            optionsService.Settings.WindowWidth,
            optionsService.Settings.WindowHeight);
        Vector2I targetSize = configuredSize == Vector2I.Zero ? DisplayServer.WindowGetSize() : configuredSize;

        for (int i = 0; i < windowSizes.Count; i++)
        {
            if (windowSizes[i] == targetSize)
                return i;
        }

        return 0;
    }

    private static List<Vector2I> BuildWindowSizes()
    {
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        Vector2I current = DisplayServer.WindowGetSize();

        List<Vector2I> sizes = [
            .. _baseWindowSizes
                .Where(size => IsWithinScreen(size, screenSize))
                .OrderBy(size => size.X * size.Y)
        ];

        if (current != Vector2I.Zero && !sizes.Contains(current))
            sizes.Add(current);

        if (sizes.Count == 0)
            sizes.Add(current == Vector2I.Zero ? new Vector2I(1280, 720) : current);

        return sizes.OrderBy(size => size.X * size.Y).ToList();
    }

    private static bool IsWithinScreen(Vector2I size, Vector2I screenSize)
    {
        if (screenSize == Vector2I.Zero)
            return true;

        return size.X <= screenSize.X && size.Y <= screenSize.Y;
    }
}
