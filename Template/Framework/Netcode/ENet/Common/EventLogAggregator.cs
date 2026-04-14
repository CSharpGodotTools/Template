using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Base class for event aggregation that coalesces rapid lifecycle events into summarized log messages.
/// Provides common timing, windowing, and formatting infrastructure.
/// </summary>
internal abstract class EventLogAggregator
{
    private const double QuietGapSeconds = 0.5;
    private const double MaxWindowSeconds = 5.0;

    /// <summary>
    /// Records a connect lifecycle event.
    /// </summary>
    /// <param name="peerId">Peer identifier associated with the connect event.</param>
    public abstract void RecordConnect(uint peerId);

    /// <summary>
    /// Records a disconnect lifecycle event.
    /// </summary>
    /// <param name="peerId">Peer identifier associated with the disconnect event.</param>
    public abstract void RecordDisconnect(uint peerId);

    /// <summary>
    /// Records a timeout lifecycle event.
    /// </summary>
    /// <param name="peerId">Peer identifier associated with the timeout event.</param>
    public abstract void RecordTimeout(uint peerId);

    /// <summary>
    /// Emits a coalesced lifecycle log report. Derived classes provide snapshot state.
    /// </summary>
    /// <param name="log">Log callback used to emit coalesced messages.</param>
    /// <param name="force">Whether to force flushing regardless of timing thresholds.</param>
    public abstract void Flush(Action<string> log, bool force = false);

    /// <summary>
    /// Protected helper to check if enough time has passed to emit a report.
    /// </summary>
    /// <param name="windowStartTicks">Timestamp ticks when the current event window began.</param>
    /// <param name="lastEventTicks">Timestamp ticks of the most recent event in the window.</param>
    /// <param name="windowSeconds">Computed event-window duration in seconds.</param>
    /// <returns><see langword="true"/> when burst timing indicates a flush should occur.</returns>
    protected static bool ShouldFlush(long windowStartTicks, long lastEventTicks, out double windowSeconds)
    {
        // Cannot compute flush timing until both start and last-event ticks are set.
        if (windowStartTicks == 0 || lastEventTicks == 0)
        {
            windowSeconds = 0;
            return false;
        }

        long nowTicks = Stopwatch.GetTimestamp();
        double secondsSinceLast = (nowTicks - lastEventTicks) / (double)Stopwatch.Frequency;
        windowSeconds = (lastEventTicks - windowStartTicks) / (double)Stopwatch.Frequency;

        return secondsSinceLast >= QuietGapSeconds || windowSeconds >= MaxWindowSeconds;
    }

    /// <summary>
    /// Formats a human-readable count string.
    /// </summary>
    /// <param name="singular">Singular noun phrase for the counted item.</param>
    /// <param name="count">Number of items to format.</param>
    /// <returns>Formatted count string with pluralization.</returns>
    protected static string FormatCount(string singular, int count)
    {
        // Use singular wording for exactly one event.
        if (count == 1)
            return $"1 {singular}";

        return $"{count} {singular}s";
    }

    /// <summary>
    /// Formats time suffix for burst messages.
    /// </summary>
    /// <param name="count">Number of events included in the burst.</param>
    /// <param name="seconds">Burst duration in seconds.</param>
    /// <returns>Formatted duration suffix for burst messages.</returns>
    protected static string FormatLastSuffix(int count, double seconds)
    {
        // Omit duration suffix when there is only a single event.
        if (count == 1)
            return string.Empty;

        return $" (last {seconds:0.##}s)";
    }

    /// <summary>
    /// Emits log entries in chronological order based on tick timestamps.
    /// </summary>
    /// <param name="entries">Entries to sort and emit.</param>
    protected static void EmitLogEntries(List<LogEntry> entries)
    {
        entries.Sort(static (left, right) => left.Tick.CompareTo(right.Tick));

        foreach (LogEntry entry in entries)
            entry.LogAction();
    }

    /// <summary>
    /// Log entry with timestamp for ordering.
    /// </summary>
    protected struct LogEntry
    {
        /// <summary>
        /// Gets or sets event timestamp ticks used for sort ordering.
        /// </summary>
        public long Tick { get; set; }

        /// <summary>
        /// Gets or sets callback that emits this log entry.
        /// </summary>
        public Action LogAction { get; set; }
    }
}
