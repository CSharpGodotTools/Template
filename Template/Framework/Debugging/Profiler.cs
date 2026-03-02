using Godot;
using System;
using System.Collections.Generic;

namespace Framework.Debugging;

public class Profiler
{
    // Variables
    private static readonly Dictionary<string, ProfilerEntry> _entries = [];
    private const int DefaultAccuracy = 2;

    // API
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
        if (!_entries.TryGetValue(key, out ProfilerEntry entry))
        {
            GD.PrintErr($"Profiler key '{key}' was not started.");
            return;
        }

        ulong elapsedUsec = Time.GetTicksUsec() - entry.StartTimeUsec;
        ulong elapsedMs = elapsedUsec / 1000UL;

        GD.Print($"{key} {elapsedMs.ToString($"F{accuracy}")} ms");
        entry.Reset();
    }

    public static void StartProcess(string key, int accuracy = DefaultAccuracy)
    {
        StartMonitor(key, accuracy, GameFramework.Metrics.StartMonitoring);
    }

    public static void StopProcess(string key)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry entry))
        {
            GD.PrintErr($"Profiler key '{key}' was not started.");
            return;
        }

        entry.Stop();
    }

    // Private Methods
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

    // Dispose
    public static void Dispose()
    {
        _entries.Clear();
    }
}
