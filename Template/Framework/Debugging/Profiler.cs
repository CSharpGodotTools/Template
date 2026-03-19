using Godot;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Debugging;

public class Profiler
{
    // Variables
    private static readonly Dictionary<string, ProfilerEntry> _entries = [];
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
        StartMonitor(key, accuracy, Game.Metrics.StartMonitoring);
    }

    public static void StopProcess(string key)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
        {
            LogMissingKey(key);
            return;
        }

        entry.Stop();
    }

    // Private Methods
    private static void LogMissingKey(string key) =>
        GD.PrintErr($"Profiler key '{key}' was not started.");

    private static void StartMonitor(string key, int accuracy, Action<string, Func<object>> registerAction)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
        {
            entry = new ProfilerEntry();
            _entries[key] = entry;
        }

        // Register (or update) the metric with the overlay for this key.
        registerAction(key, () => _entries[key].GetAverageMs(accuracy) + " ms");

        entry.Start();
    }

    // Dispose
    public static void Dispose()
    {
        _entries.Clear();
    }
}
