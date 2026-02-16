using Godot;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Monitor = Godot.Performance.Monitor;
using Vector2 = System.Numerics.Vector2;

namespace Framework.Debugging;

public class MetricsOverlay
{
    // Config
    private const int BytesInMegabyte     = 1048576;
    private const int BytesInKilobyte     = 1024;
    private const int MaxFpsBuffer        = 100;
    private const int WindowWidth         = 220;
    private const int FpsGraphWidthMargin = 15;
    private const int FpsGraphHeight      = 30;

    private const string ImGuiWindowName = "Metrics Overlay";
    private const string LabelMetrics    = "Metrics";
    private const string LabelVariables  = "Variables";
    private const string LabelFpsGraph   = "##FPSGraph"; // The ## hides the text

    // Variables
    // This was made static to allow tracking variables even before metrics overlay instance gets initialized
    private readonly Dictionary<string, Func<object>> _processMonitors = [];
    private readonly Dictionary<string, Func<string>> _currentMetrics = [];

    private readonly float[] _fpsBuffer = new float[MaxFpsBuffer];
    private float _cachedFps;
    private bool _visible;
    private int _fpsIndex;

    public MetricsOverlay()
    {
        Dictionary<string, (bool Enabled, Func<string> ValueProvider)> metrics = new()
        {
            { "FPS",                    (true,  () => $"{_cachedFps}") },
            { "Process",                (false, () => $"{Retrieve(Monitor.TimeProcess) * 1000:0.00} ms") },
            { "Physics Process",        (false, () => $"{Retrieve(Monitor.TimePhysicsProcess) * 1000:0.00} ms") },
            { "Navigation Process",     (false, () => $"{Retrieve(Monitor.TimeNavigationProcess) * 1000:0.00} ms") },
            { "Static Memory",          (true,  () => $"{Retrieve(Monitor.MemoryStatic) / BytesInMegabyte:0.0} MiB") },
            { "Static Memory Max",      (false, () => $"{Retrieve(Monitor.MemoryStaticMax) / BytesInMegabyte:0.0} MiB") },
            { "Video Memory",           (true,  () => $"{Retrieve(Monitor.RenderVideoMemUsed) / BytesInMegabyte:0.0} MiB") },
            { "Texture Memory",         (false, () => $"{Retrieve(Monitor.RenderTextureMemUsed) / BytesInMegabyte:0.0} MiB") },
            { "Buffer Memory",          (false, () => $"{Retrieve(Monitor.RenderBufferMemUsed) / BytesInMegabyte:0.0} MiB") },
            { "Message Buffer Max",     (false, () => $"{Retrieve(Monitor.MemoryMessageBufferMax) / BytesInKilobyte:0.0} KiB") },
            { "Resource Count",         (false, () => $"{Retrieve(Monitor.ObjectResourceCount)}") },
            { "Node Count",             (true,  () => $"{Retrieve(Monitor.ObjectNodeCount)}") },
            { "Orphan Node Count",      (true,  () => $"{Retrieve(Monitor.ObjectOrphanNodeCount)}") },
            { "Object Count",           (true,  () => $"{Retrieve(Monitor.ObjectCount)}") },
            { "Total Objects Drawn",    (false, () => $"{Retrieve(Monitor.RenderTotalObjectsInFrame)}") },
            { "Total Primitives Drawn", (false, () => $"{Retrieve(Monitor.RenderTotalPrimitivesInFrame)}") },
            { "Total Draw Calls",       (false, () => $"{Retrieve(Monitor.RenderTotalDrawCallsInFrame)}") },
        };

        foreach (var metric in metrics)
        {
            if (metric.Value.Enabled)
            {
                _currentMetrics.Add(metric.Key, metric.Value.ValueProvider);
            }
        }
    }

    public void Update()
    {
        if (Input.IsActionJustPressed(InputActions.DebugOverlay))
        {
            _visible = !_visible;
        }

        if (_visible)
        {
            UpdateFpsBuffer(ref _cachedFps, _fpsBuffer, ref _fpsIndex);
            RenderProcessOverlay(_currentMetrics, _fpsBuffer, ref _fpsIndex);
        }
    }

    // API
    public void StartMonitoring(string key, Func<object> function)
    {
        _visible = true;
        _processMonitors.Add(key, function);
    }

    public void StopMonitoring(string key)
    {
        _processMonitors.Remove(key);
    }

    // Private Methods
    private void RenderProcessMonitors()
    {
        int processMonitors = _processMonitors.Count;

        if (processMonitors == 0)
            return;

        if (!ImGui.CollapsingHeader(LabelVariables, ImGuiTreeNodeFlags.DefaultOpen))
            return;

        if (processMonitors > 0)
        {
            foreach (KeyValuePair<string, Func<object>> kvp in _processMonitors)
            {
                ImGui.Text($"{kvp.Key}: {kvp.Value()}");
            }
        }
    }

    private void RenderProcessOverlay(Dictionary<string, Func<string>> metrics, float[] fpsBuffer, ref int fpsIndex)
    {
        Vector2 topRight = new(ImGui.GetIO().DisplaySize.X - WindowWidth, 0);
        BeginOverlayWindow(topRight);

        RenderMetrics(metrics, fpsBuffer, fpsIndex);

        RenderProcessMonitors();

        EndOverlayWindow();
    }

    private static void BeginOverlayWindow(Vector2 position)
    {
        ImGui.SetNextWindowPos(position, ImGuiCond.Always);
        ImGui.Begin(ImGuiWindowName, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);
    }

    private static void EndOverlayWindow()
    {
        ImGui.End();
    }

    private static void RenderMetrics(Dictionary<string, Func<string>> metrics, float[] fpsBuffer, int fpsIndex)
    {
        if (!ImGui.CollapsingHeader(LabelMetrics, ImGuiTreeNodeFlags.DefaultOpen))
            return;

        foreach ((string key, Func<string> valueProvider) in metrics)
        {
            ImGui.Text($"{key}: {valueProvider()}");

            if (key == "FPS")
            {
                RenderFpsGraph(fpsBuffer, fpsIndex);
            }
        }
    }

    private static void UpdateFpsBuffer(ref float cachedFps, float[] fpsBuffer, ref int fpsIndex)
    {
        cachedFps = (float)Retrieve(Monitor.TimeFps);
        fpsBuffer[fpsIndex] = cachedFps;
        fpsIndex = (fpsIndex + 1) % fpsBuffer.Length;
    }

    private static void RenderFpsGraph(float[] fpsBuffer, int fpsIndex)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

        ImGui.PlotLines(LabelFpsGraph, ref fpsBuffer[0], fpsBuffer.Length, fpsIndex,
            overlay_text: null,
            scale_min: 0,
            scale_max: DisplayServer.ScreenGetRefreshRate(),
            graph_size: new Vector2(WindowWidth - FpsGraphWidthMargin, FpsGraphHeight));

        ImGui.PopStyleVar();
    }

    private static double Retrieve(Monitor monitor) => Performance.GetMonitor(monitor);
}
