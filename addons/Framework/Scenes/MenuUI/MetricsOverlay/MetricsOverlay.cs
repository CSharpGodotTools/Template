using Godot;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Monitor = Godot.Performance.Monitor;
using Vector2 = System.Numerics.Vector2;

namespace __TEMPLATE__.UI;

[SceneTree]
public partial class MetricsOverlay : Node
{
    private const int BYTES_IN_MEGABYTE = 1048576;
    private const int BYTES_IN_KILOBYTE = 1024;
    private const int MAX_FPS_BUFFER = 100;
    private const int WINDOW_WIDTH = 220;

    private float[] _fpsBuffer = new float[MAX_FPS_BUFFER];
    private float _cachedFPS;
    private int _fpsIndex;
    private bool _enableFPSGraph;

    private bool _visible;

    private Dictionary<string, (bool enabled, Func<string> valueProvider)> _metrics;

    private static double Retrieve(Monitor monitor) => Performance.GetMonitor(monitor);

    private const string SETTINGS_FILE = "user://metrics_settings.json";

    public override void _Ready()
    {
        _metrics = new()
        {
            { "FPS",                    (true,  () => $"{_cachedFPS}") },
            { "Process",                (true,  () => $"{Retrieve(Monitor.TimeProcess) * 1000:0.00} ms") },
            { "Physics Process",        (false, () => $"{Retrieve(Monitor.TimePhysicsProcess) * 1000:0.00} ms") },
            { "Navigation Process",     (false, () => $"{Retrieve(Monitor.TimeNavigationProcess) * 1000:0.00} ms") },
            { "Static Memory",          (true,  () => $"{Retrieve(Monitor.MemoryStatic) / BYTES_IN_MEGABYTE:0.0} MiB") },
            { "Static Memory Max",      (false, () => $"{Retrieve(Monitor.MemoryStaticMax) / BYTES_IN_MEGABYTE:0.0} MiB") },
            { "Message Buffer Max",     (false, () => $"{Retrieve(Monitor.MemoryMessageBufferMax) / BYTES_IN_KILOBYTE:0.0} KiB") },
            { "Object Count",           (true,  () => $"{Retrieve(Monitor.ObjectCount)}") },
            { "Resource Count",         (true,  () => $"{Retrieve(Monitor.ObjectResourceCount)}") },
            { "Node Count",             (true,  () => $"{Retrieve(Monitor.ObjectNodeCount)}") },
            { "Orphan Node Count",      (true,  () => $"{Retrieve(Monitor.ObjectOrphanNodeCount)}") },
            { "Total Objects Drawn",    (false, () => $"{Retrieve(Monitor.RenderTotalObjectsInFrame)}") },
            { "Total Primitives Drawn", (false, () => $"{Retrieve(Monitor.RenderTotalPrimitivesInFrame)}") },
            { "Total Draw Calls",       (false, () => $"{Retrieve(Monitor.RenderTotalDrawCallsInFrame)}") },
            { "Video Memory",           (true,  () => $"{Retrieve(Monitor.RenderVideoMemUsed) / BYTES_IN_MEGABYTE:0.0} MiB") },
            { "Texture Memory",         (false, () => $"{Retrieve(Monitor.RenderTextureMemUsed) / BYTES_IN_MEGABYTE:0.0} MiB") },
            { "Buffer Memory",          (false, () => $"{Retrieve(Monitor.RenderBufferMemUsed) / BYTES_IN_MEGABYTE:0.0} MiB") }
        };

        LoadSettings();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputActions.DebugOverlay))
        {
            _visible = !_visible;
        }

        if (_visible)
        {
            RenderPerformanceMetrics();
        }
    }

    // Will be called in exported release but not in the editor
    public override void _ExitTree()
    {
        SaveSettings();
    }

    private void RenderPerformanceMetrics()
    {
        _cachedFPS = (float)Retrieve(Monitor.TimeFps);
        UpdateFpsBuffer();

        SetupImGuiWindow();

        RenderSettings();
        RenderMetrics();

        ImGui.End();
    }

    private static void SetupImGuiWindow()
    {
        // Print the window scale
        Godot.Vector2 size = DisplayServer.WindowGetSize();
        Vector2 screenSize = new(size.X, size.Y);
        Vector2 windowSize = new(WINDOW_WIDTH, 0);
        Vector2 topRight = new Vector2(screenSize.X, 0) - new Vector2(windowSize.X, -ImGui.GetFrameHeight());

        ImGui.SetNextWindowPos(topRight, ImGuiCond.Always);
        ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
        ImGui.Begin("Metrics Overlay", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);
    }

    private void RenderSettings()
    {
        if (ImGui.CollapsingHeader("Settings"))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.One * 2);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.One * 2);

            ImGui.Checkbox("Enable FPS Graph", ref _enableFPSGraph);

            foreach (string key in _metrics.Keys)
            {
                bool enabled = _metrics[key].enabled;

                if (ImGui.Checkbox($"Log {key}", ref enabled))
                {
                    _metrics[key] = (enabled, _metrics[key].valueProvider);
                }
            }

            ImGui.PopStyleVar(2);
        }
    }

    private void RenderMetrics()
    {
        if (ImGui.CollapsingHeader("Metrics", ImGuiTreeNodeFlags.DefaultOpen))
        {
            foreach ((string key, (bool enabled, Func<string> valueProvider)) in _metrics)
            {
                if (enabled)
                {
                    ImGui.Text($"{key}: {valueProvider()}");

                    if (key == "FPS" && _enableFPSGraph)
                    {
                        RenderFpsGraph();
                    }
                }
            }
        }
    }

    private void RenderFpsGraph()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

        ImGui.PlotLines("##FPSGraph", ref _fpsBuffer[0], _fpsBuffer.Length, _fpsIndex,
            overlay_text: null,
            scale_min: 0,
            scale_max: DisplayServer.ScreenGetRefreshRate(),
            graph_size: new Vector2(WINDOW_WIDTH - 15, 30));

        ImGui.PopStyleVar();
    }

    private void UpdateFpsBuffer()
    {
        _fpsBuffer[_fpsIndex] = _cachedFPS;
        _fpsIndex = (_fpsIndex + 1) % _fpsBuffer.Length;
    }

    private void SaveSettings()
    {
        Dictionary<string, bool> settings = [];

        foreach (string key in _metrics.Keys)
        {
            settings[key] = _metrics[key].enabled;
        }

        string json = JsonSerializer.Serialize(settings);

        FileAccess file = FileAccess.Open(SETTINGS_FILE, FileAccess.ModeFlags.Write);

        file.StoreString(json);
        file.Close();
    }

    private void LoadSettings()
    {
        if (!FileAccess.FileExists(SETTINGS_FILE))
            return;

        FileAccess file = FileAccess.Open(SETTINGS_FILE, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();
        file.Close();

        Dictionary<string, bool> settings = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);

        if (settings == null)
            return;

        foreach (string key in settings.Keys)
        {
            if (_metrics.TryGetValue(key, out (bool enabled, Func<string> valueProvider) value))
            {
                _metrics[key] = (settings[key], value.valueProvider);
            }
        }
    }
}

