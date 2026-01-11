using Godot;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Debugging;

public class Profiler
{
    #region Variables
    private Dictionary<string, ProfilerEntry> _entries = [];
    private const int DefaultAccuracy = 2;
    #endregion

    #region API
    public void Start(string key)
    {
        if (!_entries.TryGetValue(key, out ProfilerEntry entry))
        {
            entry = new ProfilerEntry();
            _entries[key] = entry;
        }

        entry.Start();
    }

    public void Stop(string key, int accuracy = DefaultAccuracy)
    {
        ProfilerEntry entry = _entries[key];

        ulong elapsedUsec = Time.GetTicksUsec() - entry.StartTimeUsec;
        ulong elapsedMs = elapsedUsec / 1000UL;

        GD.Print($"{key} {elapsedMs.ToString($"F{accuracy}")} ms");
        entry.Reset();
    }

    public void StartProcess(string key, int accuracy = DefaultAccuracy)
    {
        StartMonitor(key, accuracy, Game.Metrics.StartMonitoringProcess);
    }

    public void StopProcess(string key)
    {
        _entries[key].Stop();
    }

    public void StartPhysicsProcess(string key, int accuracy = DefaultAccuracy)
    {
        StartMonitor(key, accuracy, Game.Metrics.StartMonitoringPhysicsProcess);
    }

    public void StopPhysicsProcess(string key)
    {
        _entries[key].Stop();
    }
    #endregion

    #region Private Methods
    private void StartMonitor(string key, int accuracy, Action<string, Func<object>> registerAction)
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
    #endregion

    public void Dispose()
    {
        _entries = null;
    }
}
