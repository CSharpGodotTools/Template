using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Framework.Netcode.Client;

/// <summary>
/// Coalesces rapid client lifecycle events into summarised log messages.
/// Thread-safe: shared across all client worker threads.
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
    public override void RecordConnect(uint peerId)
    {
        Interlocked.Increment(ref _connectedCount);
        Interlocked.Exchange(ref _lastConnectPeerId, peerId);
        MarkEvent(ref _lastConnectTicks);
    }

    /// <summary>
    /// Records a disconnect lifecycle event.
    /// </summary>
    public override void RecordDisconnect(uint peerId)
    {
        Interlocked.Increment(ref _disconnectedCount);
        Interlocked.Exchange(ref _lastDisconnectPeerId, peerId);
        MarkEvent(ref _lastDisconnectTicks);
    }

    /// <summary>
    /// Records a timeout lifecycle event.
    /// </summary>
    public override void RecordTimeout(uint peerId)
    {
        Interlocked.Increment(ref _timeoutCount);
        Interlocked.Exchange(ref _lastTimeoutPeerId, peerId);
        MarkEvent(ref _lastTimeoutTicks);
    }

    /// <summary>
    /// Emits a coalesced lifecycle log report when burst thresholds are reached.
    /// </summary>
    public override void Flush(Action<string> log, bool force = false)
    {
        int connectedSnapshot = Volatile.Read(ref _connectedCount);
        int disconnectedSnapshot = Volatile.Read(ref _disconnectedCount);
        int timeoutSnapshot = Volatile.Read(ref _timeoutCount);

        if (connectedSnapshot == 0 && disconnectedSnapshot == 0 && timeoutSnapshot == 0)
        {
            return;
        }

        long windowStartTicks = Interlocked.Read(ref _eventWindowStartTicks);
        long eventLastTicks = Interlocked.Read(ref _eventLastEventTicks);

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

        if (force)
        {
            Interlocked.Exchange(ref _eventLastEventTicks, 0);
        }

        Interlocked.CompareExchange(ref _eventWindowStartTicks, 0, windowStartTicks);

        double reportSeconds = Math.Max(windowSeconds, 0.01);
        List<LogEntry> logEntries = new(3);

        if (connects > 0)
        {
            logEntries.Add(new LogEntry { Tick = lastConnectTicks, LogAction = () => log(FormatConnectMessage(connects, lastConnectPeerId, reportSeconds)) });
        }

        if (disconnects > 0)
        {
            logEntries.Add(new LogEntry { Tick = lastDisconnectTicks, LogAction = () => log(FormatDisconnectMessage(disconnects, lastDisconnectPeerId, reportSeconds)) });
        }

        if (timeouts > 0)
        {
            logEntries.Add(new LogEntry { Tick = lastTimeoutTicks, LogAction = () => log(FormatTimeoutMessage(timeouts, lastTimeoutPeerId, reportSeconds)) });
        }

        EmitLogEntries(log, logEntries);
    }

    private void MarkEvent(ref long eventTypeLastTicks)
    {
        long nowTicks = Stopwatch.GetTimestamp();

        if (Interlocked.CompareExchange(ref _eventWindowStartTicks, nowTicks, 0) == 0)
        {
            Interlocked.Exchange(ref _eventLastEventTicks, nowTicks);
        }
        else
        {
            Interlocked.Exchange(ref _eventLastEventTicks, nowTicks);
        }

        Interlocked.Exchange(ref eventTypeLastTicks, nowTicks);
    }

    private string FormatConnectMessage(int count, long peerId, double seconds)
    {
        if (count == 1)
        {
            if (peerId > 0)
            {
                return $"Connected to server as peer {peerId}";
            }

            return "Connected to server";
        }

        return $"{FormatCount("connect event", count)}{FormatLastSuffix(count, seconds)}";
    }

    private string FormatDisconnectMessage(int count, long peerId, double seconds)
    {
        if (count == 1)
        {
            if (peerId > 0)
            {
                return $"Disconnected from server (peer {peerId})";
            }

            return "Disconnected from server";
        }

        return $"{FormatCount("disconnect event", count)}{FormatLastSuffix(count, seconds)}";
    }

    private string FormatTimeoutMessage(int count, long peerId, double seconds)
    {
        if (count == 1)
        {
            if (peerId > 0)
            {
                return $"Connection to server timed out (peer {peerId})";
            }

            return "Connection to server timed out";
        }

        return $"{FormatCount("timeout event", count)}{FormatLastSuffix(count, seconds)}";
    }
}
