using Godot;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Debugging;

public class Profiler
{
    // Variables
    private static readonly Dictionary<string, ProfilerEntry> _entries = [];
    private static readonly Dictionary<string, IDisposable> _monitorHandles = [];
    private const int DefaultAccuracy = 2;

    // API
    public static void Start(string key)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
        {
            entry = new ProfilerEntry();
            _entries[key] = entry;
        }

        entry.Start();
    }

    public static void Stop(string key, int accuracy = DefaultAccuracy)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
        {
            LogMissingKey(key);
            return;
        }

        ulong elapsedUsec = Time.GetTicksUsec() - entry.StartTimeUsec;
        ulong elapsedMs = elapsedUsec / 1000UL;

        GD.Print($"{key} {elapsedMs.ToString($"F{accuracy}")} ms");
        entry.Reset();
    }

    public static void StartProcess(string key, int accuracy = DefaultAccuracy)
    {
        StartMonitor(key, accuracy, Game.Metrics);
    }

    public static void StopProcess(string key)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
        {
            LogMissingKey(key);
            return;
        }

        entry.Stop();

        if (_monitorHandles.Remove(key, out IDisposable? monitorHandle))
            monitorHandle.Dispose();
    }

    // Private Methods
    private static void LogMissingKey(string key) =>
        GD.PrintErr($"Profiler key '{key}' was not started.");

    private static void StartMonitor(string key, int accuracy, IMetricsOverlay metrics)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
        {
            entry = new ProfilerEntry();
            _entries[key] = entry;
        }

        if (_monitorHandles.Remove(key, out IDisposable? existingHandle))
            existingHandle.Dispose();

        _monitorHandles[key] = metrics.StartMonitoring(key, () => _entries[key].GetAverageMs(accuracy) + " ms");

        entry.Start();
    }

    // Dispose
    public static void Dispose()
    {
        foreach (IDisposable monitorHandle in _monitorHandles.Values)
            monitorHandle.Dispose();

        _monitorHandles.Clear();
        _entries.Clear();
    }
}
