using Godot;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Debugging;

/// <summary>
/// Tracks named timings and publishes averaged process metrics to the debug overlay.
/// </summary>
public class Profiler
{
    // Variables
    private static readonly Dictionary<string, ProfilerEntry> _entries = [];
    private static readonly Dictionary<string, IDisposable> _monitorHandles = [];
    private const int DefaultAccuracy = 2;
    private static IMetricsOverlay? _metrics;

    /// <summary>
    /// Configures the metrics overlay target used by process monitoring helpers.
    /// </summary>
    /// <param name="metrics">Overlay instance that receives profiler monitor values.</param>
    public static void Configure(IMetricsOverlay metrics)
    {
        _metrics = metrics;
    }

    // API
    /// <summary>
    /// Starts timing for the specified profiler key.
    /// </summary>
    /// <param name="key">Unique key identifying the measured operation.</param>
    public static void Start(string key)
    {
        // Lazily create profiler entries the first time a key is observed.
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
        {
            entry = new ProfilerEntry();
            _entries[key] = entry;
        }

        entry.Start();
    }

    /// <summary>
    /// Stops timing for a key and prints elapsed milliseconds.
    /// </summary>
    /// <param name="key">Unique key identifying the measured operation.</param>
    /// <param name="accuracy">Decimal precision used in the printed value.</param>
    public static void Stop(string key, int accuracy = DefaultAccuracy)
    {
        // Stop requests for unknown keys are logged and ignored.
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

    /// <summary>
    /// Starts profiling a repeating process and publishes average frame time to the overlay.
    /// </summary>
    /// <param name="key">Unique key identifying the measured process.</param>
    /// <param name="accuracy">Decimal precision used in the published average.</param>
    public static void StartProcess(string key, int accuracy = DefaultAccuracy)
    {
        // Process monitors require a configured metrics sink.
        if (_metrics == null)
        {
            GD.PrintErr("Profiler metrics are not configured.");
            return;
        }

        StartMonitor(key, accuracy, _metrics);
    }

    /// <summary>
    /// Stops process profiling for a key and removes its overlay monitor.
    /// </summary>
    /// <param name="key">Unique key identifying the measured process.</param>
    public static void StopProcess(string key)
    {
        // Ignore stop requests for keys that were never started.
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
        {
            LogMissingKey(key);
            return;
        }

        entry.Stop();

        // Dispose and remove any active monitor stream for this key.
        if (_monitorHandles.Remove(key, out IDisposable? monitorHandle))
            monitorHandle.Dispose();
    }

    // Private Methods
    /// <summary>
    /// Logs a consistent error when stop operations reference an unknown key.
    /// </summary>
    /// <param name="key">Profiler key that could not be resolved.</param>
    private static void LogMissingKey(string key) =>
        GD.PrintErr($"Profiler key '{key}' was not started.");

    /// <summary>
    /// Starts or replaces an overlay-backed monitor for a profiler key.
    /// </summary>
    /// <param name="key">Profiler key to monitor.</param>
    /// <param name="accuracy">Decimal precision used in average output.</param>
    /// <param name="metrics">Overlay that receives monitor updates.</param>
    private static void StartMonitor(string key, int accuracy, IMetricsOverlay metrics)
    {
        // Ensure monitor-backed profiling also has a backing entry.
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
        {
            entry = new ProfilerEntry();
            _entries[key] = entry;
        }

        // Replace any previous monitor handle so only one active stream exists per key.
        if (_monitorHandles.Remove(key, out IDisposable? existingHandle))
            existingHandle.Dispose();

        _monitorHandles[key] = metrics.StartMonitoring(key, () => _entries[key].GetAverageMs(accuracy) + " ms");

        entry.Start();
    }

    // Dispose
    /// <summary>
    /// Disposes all active monitor handles and resets profiler state.
    /// </summary>
    public static void Dispose()
    {
        foreach (IDisposable monitorHandle in _monitorHandles.Values)
            monitorHandle.Dispose();

        _monitorHandles.Clear();
        _entries.Clear();
        _metrics = null;
    }
}
