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
    public abstract void RecordConnect(uint peerId);

    /// <summary>
    /// Records a disconnect lifecycle event.
    /// </summary>
    public abstract void RecordDisconnect(uint peerId);

    /// <summary>
    /// Records a timeout lifecycle event.
    /// </summary>
    public abstract void RecordTimeout(uint peerId);

    /// <summary>
    /// Emits a coalesced lifecycle log report. Derived classes provide snapshot state.
    /// </summary>
    public abstract void Flush(Action<string> log, bool force = false);

    /// <summary>
    /// Protected helper to check if enough time has passed to emit a report.
    /// </summary>
    protected static bool ShouldFlush(long windowStartTicks, long lastEventTicks, out double windowSeconds)
    {
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
    protected static string FormatCount(string singular, int count)
    {
        if (count == 1)
        {
            return $"1 {singular}";
        }

        return $"{count} {singular}s";
    }

    /// <summary>
    /// Formats time suffix for burst messages.
    /// </summary>
    protected static string FormatLastSuffix(int count, double seconds)
    {
        if (count == 1)
        {
            return string.Empty;
        }

        return $" (last {seconds:0.##}s)";
    }

    /// <summary>
    /// Emits log entries in chronological order based on tick timestamps.
    /// </summary>
    protected static void EmitLogEntries(Action<string> log, List<LogEntry> entries)
    {
        entries.Sort(static (left, right) => left.Tick.CompareTo(right.Tick));

        foreach (LogEntry entry in entries)
        {
            entry.LogAction();
        }
    }

    /// <summary>
    /// Log entry with timestamp for ordering.
    /// </summary>
    protected struct LogEntry
    {
        public long Tick { get; set; }
        public Action LogAction { get; set; }
    }
}
