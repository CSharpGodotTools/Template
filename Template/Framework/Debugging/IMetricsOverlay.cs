using System;

namespace __TEMPLATE__.Debugging;

/// <summary>
/// Exposes a lightweight overlay contract for tracking named runtime metrics.
/// </summary>
public interface IMetricsOverlay
{
    /// <summary>
    /// Starts monitoring a value provider and renders it in the overlay until disposed.
    /// </summary>
    /// <param name="key">Display label for the monitored value.</param>
    /// <param name="function">Callback used to retrieve the current value.</param>
    /// <returns>A handle that stops monitoring when disposed.</returns>
    IDisposable StartMonitoring(string key, Func<object> function);
}
