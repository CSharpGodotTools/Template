using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

public static class Profiler
{
    private static Dictionary<string, ProfilerEntry> _entries = [];
    private const int DefaultAccuracy = 2;

    public static void Start(string key)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry entry))
        {
            entry = new ProfilerEntry();
            _entries[key] = entry;
        }

        entry.Start();
    }

    public static void Stop(string key, int accuracy = DefaultAccuracy)
    {
        ProfilerEntry entry = _entries[key];

        ulong elapsedUsec = Time.GetTicksUsec() - entry.StartTimeUsec;
        ulong elapsedMs = elapsedUsec / 1000UL;

        GD.Print($"{key} {elapsedMs.ToString($"F{accuracy}")} ms");
        entry.Reset();
    }

    public static void StartProcess(string key, int accuracy = DefaultAccuracy)
    {
        StartMonitor(key, accuracy, MetricsOverlay.StartMonitoringProcess);
    }

    public static void StopProcess(string key)
    {
        _entries[key].Stop();
    }

    public static void StartPhysicsProcess(string key, int accuracy = DefaultAccuracy)
    {
        StartMonitor(key, accuracy, MetricsOverlay.StartMonitoringPhysicsProcess);
    }

    public static void StopPhysicsProcess(string key)
    {
        _entries[key].Stop();
    }

    private static void StartMonitor(string key, int accuracy, Action<string, Func<object>> registerAction)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry entry))
        {
            entry = new ProfilerEntry();
            _entries[key] = entry;

            // Register the metric with the appropriate overlay
            registerAction(key, () => _entries[key].GetAverageMs(accuracy) + " ms");
        }

        entry.Start();
    }

    public static void Dispose()
    {
        _entries = null;
    }
}
