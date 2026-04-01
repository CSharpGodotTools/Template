using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace __TEMPLATE__.Netcode.Client;

/// <summary>
/// Coalesces rapid client lifecycle events into summarised log messages.
/// Shared across all client worker threads.
/// </summary>
internal sealed class ClientLogAggregator : EventLogAggregator
{
    private int _connectedCount;
    private int _disconnectedCount;
    private int _timeoutCount;

    private long _eventWindowStartTicks;
    private long _eventLastEventTicks;

    private long _lastConnectTicks;
    private long _lastDisconnectTicks;
    private long _lastTimeoutTicks;
    private long _lastConnectPeerId;
    private long _lastDisconnectPeerId;
    private long _lastTimeoutPeerId;

    /// <summary>
    /// Records a connect lifecycle event.
    /// </summary>
    /// <param name="peerId">Peer identifier associated with the connect event.</param>
    public override void RecordConnect(uint peerId)
    {
        Interlocked.Increment(ref _connectedCount);
        Interlocked.Exchange(ref _lastConnectPeerId, peerId);
        MarkEvent(ref _lastConnectTicks);
    }

    /// <summary>
    /// Records a disconnect lifecycle event.
    /// </summary>
    /// <param name="peerId">Peer identifier associated with the disconnect event.</param>
    public override void RecordDisconnect(uint peerId)
    {
        Interlocked.Increment(ref _disconnectedCount);
        Interlocked.Exchange(ref _lastDisconnectPeerId, peerId);
        MarkEvent(ref _lastDisconnectTicks);
    }

    /// <summary>
    /// Records a timeout lifecycle event.
    /// </summary>
    /// <param name="peerId">Peer identifier associated with the timeout event.</param>
    public override void RecordTimeout(uint peerId)
    {
        Interlocked.Increment(ref _timeoutCount);
        Interlocked.Exchange(ref _lastTimeoutPeerId, peerId);
        MarkEvent(ref _lastTimeoutTicks);
    }

    /// <summary>
    /// Emits a coalesced lifecycle log report when burst thresholds are reached.
    /// </summary>
    /// <param name="log">Log callback used to emit coalesced messages.</param>
    /// <param name="force">Whether to force a flush regardless of quiet-gap thresholds.</param>
    public override void Flush(Action<string> log, bool force = false)
    {
        int connectedSnapshot = Volatile.Read(ref _connectedCount);
        int disconnectedSnapshot = Volatile.Read(ref _disconnectedCount);
        int timeoutSnapshot = Volatile.Read(ref _timeoutCount);

        // Exit early when no lifecycle events were captured.
        if (connectedSnapshot == 0 && disconnectedSnapshot == 0 && timeoutSnapshot == 0)
        {
            return;
        }

        long windowStartTicks = Interlocked.Read(ref _eventWindowStartTicks);
        long eventLastTicks = Interlocked.Read(ref _eventLastEventTicks);

        // Skip emitting until burst thresholds are reached unless a forced flush is requested.
        if (!ShouldFlush(windowStartTicks, eventLastTicks, out double windowSeconds) && !force)
        {
            return;
        }

        // Try to claim ownership of flushing
        if (!force && Interlocked.CompareExchange(ref _eventLastEventTicks, 0, eventLastTicks) != eventLastTicks)
        {
            return;
        }

        int connects = Interlocked.Exchange(ref _connectedCount, 0);
        int disconnects = Interlocked.Exchange(ref _disconnectedCount, 0);
        int timeouts = Interlocked.Exchange(ref _timeoutCount, 0);
        long lastConnectTicks = Interlocked.Exchange(ref _lastConnectTicks, 0);
        long lastDisconnectTicks = Interlocked.Exchange(ref _lastDisconnectTicks, 0);
        long lastTimeoutTicks = Interlocked.Exchange(ref _lastTimeoutTicks, 0);
        long lastConnectPeerId = Interlocked.Exchange(ref _lastConnectPeerId, 0);
        long lastDisconnectPeerId = Interlocked.Exchange(ref _lastDisconnectPeerId, 0);
        long lastTimeoutPeerId = Interlocked.Exchange(ref _lastTimeoutPeerId, 0);

        // Reset last-event marker when flush was forced.
        if (force)
        {
            Interlocked.Exchange(ref _eventLastEventTicks, 0);
        }

        Interlocked.CompareExchange(ref _eventWindowStartTicks, 0, windowStartTicks);

        double reportSeconds = Math.Max(windowSeconds, 0.01);
        List<LogEntry> logEntries = new(3);

        // Emit connect summary when one or more connect events were captured.
        if (connects > 0)
        {
            logEntries.Add(new LogEntry { Tick = lastConnectTicks, LogAction = () => log(FormatConnectMessage(connects, lastConnectPeerId, reportSeconds)) });
        }

        // Emit disconnect summary when one or more disconnect events were captured.
        if (disconnects > 0)
        {
            logEntries.Add(new LogEntry { Tick = lastDisconnectTicks, LogAction = () => log(FormatDisconnectMessage(disconnects, lastDisconnectPeerId, reportSeconds)) });
        }

        // Emit timeout summary when one or more timeout events were captured.
        if (timeouts > 0)
        {
            logEntries.Add(new LogEntry { Tick = lastTimeoutTicks, LogAction = () => log(FormatTimeoutMessage(timeouts, lastTimeoutPeerId, reportSeconds)) });
        }

        EmitLogEntries(logEntries);
    }

    /// <summary>
    /// Marks the lifecycle event window and updates last-seen timestamps for an event type.
    /// </summary>
    /// <param name="eventTypeLastTicks">Reference to the event-type specific last tick field.</param>
    private void MarkEvent(ref long eventTypeLastTicks)
    {
        long nowTicks = Stopwatch.GetTimestamp();

        // Start a burst window on first event, then keep last-event timestamps current.
        Interlocked.CompareExchange(ref _eventWindowStartTicks, nowTicks, 0);
        Interlocked.Exchange(ref _eventLastEventTicks, nowTicks);
        Interlocked.Exchange(ref eventTypeLastTicks, nowTicks);
    }

    /// <summary>
    /// Formats a connect summary message for either single or burst events.
    /// </summary>
    /// <param name="count">Number of connect events in the flush window.</param>
    /// <param name="peerId">Last observed peer identifier.</param>
    /// <param name="seconds">Flush window duration in seconds.</param>
    /// <returns>Human-readable connect summary.</returns>
    private static string FormatConnectMessage(int count, long peerId, double seconds)
    {
        // Use detailed singular wording for a single connect event.
        if (count == 1)
        {
            // Include peer id when available.
            if (peerId > 0)
            {
                return $"Connected to server as peer {peerId}";
            }

            return "Connected to server";
        }

        return $"{FormatCount("connect event", count)}{FormatLastSuffix(count, seconds)}";
    }

    /// <summary>
    /// Formats a disconnect summary message for either single or burst events.
    /// </summary>
    /// <param name="count">Number of disconnect events in the flush window.</param>
    /// <param name="peerId">Last observed peer identifier.</param>
    /// <param name="seconds">Flush window duration in seconds.</param>
    /// <returns>Human-readable disconnect summary.</returns>
    private static string FormatDisconnectMessage(int count, long peerId, double seconds)
    {
        // Use detailed singular wording for a single disconnect event.
        if (count == 1)
        {
            // Include peer id when available.
            if (peerId > 0)
            {
                return $"Disconnected from server (peer {peerId})";
            }

            return "Disconnected from server";
        }

        return $"{FormatCount("disconnect event", count)}{FormatLastSuffix(count, seconds)}";
    }

    /// <summary>
    /// Formats a timeout summary message for either single or burst events.
    /// </summary>
    /// <param name="count">Number of timeout events in the flush window.</param>
    /// <param name="peerId">Last observed peer identifier.</param>
    /// <param name="seconds">Flush window duration in seconds.</param>
    /// <returns>Human-readable timeout summary.</returns>
    private static string FormatTimeoutMessage(int count, long peerId, double seconds)
    {
        // Use detailed singular wording for a single timeout event.
        if (count == 1)
            return $"Connection to server timed out (peer {peerId})";

        return $"{FormatCount("timeout event", count)}{FormatLastSuffix(count, seconds)}";
    }
}
