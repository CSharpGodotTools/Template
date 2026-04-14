using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Coalesces rapid server lifecycle events into summarised log messages.
/// Expected to run on a single ENet worker thread.
/// </summary>
internal sealed class ServerLogAggregator : EventLogAggregator
{
    private int _connectedCount;
    private int _disconnectedCount;
    private int _timeoutCount;

    private long _windowStartTicks;
    private long _lastEventTicks;

    private long _lastConnectTicks;
    private long _lastDisconnectTicks;
    private long _lastTimeoutTicks;
    private uint _lastConnectPeerId;
    private uint _lastDisconnectPeerId;
    private uint _lastTimeoutPeerId;

    /// <summary>
    /// Records a connect lifecycle event.
    /// </summary>
    /// <param name="peerId">Peer identifier associated with the connect event.</param>
    public override void RecordConnect(uint peerId)
    {
        _connectedCount++;
        _lastConnectPeerId = peerId;
        MarkEvent(ref _lastConnectTicks);
    }

    /// <summary>
    /// Records a disconnect lifecycle event.
    /// </summary>
    /// <param name="peerId">Peer identifier associated with the disconnect event.</param>
    public override void RecordDisconnect(uint peerId)
    {
        _disconnectedCount++;
        _lastDisconnectPeerId = peerId;
        MarkEvent(ref _lastDisconnectTicks);
    }

    /// <summary>
    /// Records a timeout lifecycle event.
    /// </summary>
    /// <param name="peerId">Peer identifier associated with the timeout event.</param>
    public override void RecordTimeout(uint peerId)
    {
        _timeoutCount++;
        _lastTimeoutPeerId = peerId;
        MarkEvent(ref _lastTimeoutTicks);
    }

    /// <summary>
    /// Emits a coalesced lifecycle log report when burst thresholds are reached.
    /// </summary>
    /// <param name="log">Log callback used to emit coalesced messages.</param>
    /// <param name="force">Whether to force a flush regardless of timing thresholds.</param>
    public override void Flush(Action<string> log, bool force = false)
    {
        // Exit early when no lifecycle events were captured.
        if (_connectedCount == 0 && _disconnectedCount == 0 && _timeoutCount == 0)
            return;

        // Wait for quiet-gap/max-window thresholds unless a forced flush was requested.
        if (!ShouldFlush(_windowStartTicks, _lastEventTicks, out double windowSeconds) && !force)
            return;

        int connects = _connectedCount;
        int disconnects = _disconnectedCount;
        int timeouts = _timeoutCount;
        long lastConnectTicks = _lastConnectTicks;
        long lastDisconnectTicks = _lastDisconnectTicks;
        long lastTimeoutTicks = _lastTimeoutTicks;
        uint lastConnectPeerId = _lastConnectPeerId;
        uint lastDisconnectPeerId = _lastDisconnectPeerId;
        uint lastTimeoutPeerId = _lastTimeoutPeerId;

        _connectedCount = 0;
        _disconnectedCount = 0;
        _timeoutCount = 0;
        _windowStartTicks = 0;
        _lastEventTicks = 0;
        _lastConnectTicks = 0;
        _lastDisconnectTicks = 0;
        _lastTimeoutTicks = 0;
        _lastConnectPeerId = 0;
        _lastDisconnectPeerId = 0;
        _lastTimeoutPeerId = 0;

        double reportSeconds = Math.Max(windowSeconds, 0.01);
        List<LogEntry> logEntries = new(3);

        // Emit connect summary when one or more connect events were captured.
        if (connects > 0)
            logEntries.Add(new LogEntry { Tick = lastConnectTicks, LogAction = () => log(FormatConnectMessage(connects, lastConnectPeerId, reportSeconds)) });

        // Emit disconnect summary when one or more disconnect events were captured.
        if (disconnects > 0)
            logEntries.Add(new LogEntry { Tick = lastDisconnectTicks, LogAction = () => log(FormatDisconnectMessage(disconnects, lastDisconnectPeerId, reportSeconds)) });

        // Emit timeout summary when one or more timeout events were captured.
        if (timeouts > 0)
            logEntries.Add(new LogEntry { Tick = lastTimeoutTicks, LogAction = () => log(FormatTimeoutMessage(timeouts, lastTimeoutPeerId, reportSeconds)) });

        EmitLogEntries(logEntries);
    }

    /// <summary>
    /// Marks event-window timing for a lifecycle event type.
    /// </summary>
    /// <param name="eventTypeLastTicks">Reference to the event-type specific tick field.</param>
    private void MarkEvent(ref long eventTypeLastTicks)
    {
        long nowTicks = Stopwatch.GetTimestamp();

        // Capture first event timestamp as window start, then track latest event tick.
        if (_windowStartTicks == 0)
            _windowStartTicks = nowTicks;

        _lastEventTicks = nowTicks;
        eventTypeLastTicks = nowTicks;
    }

    /// <summary>
    /// Formats connect lifecycle messages for single or burst events.
    /// </summary>
    /// <param name="count">Connect event count.</param>
    /// <param name="peerId">Last peer id seen for this event type.</param>
    /// <param name="seconds">Burst window duration in seconds.</param>
    /// <returns>Formatted connect message.</returns>
    private static string FormatConnectMessage(int count, uint peerId, double seconds)
    {
        // Use detailed singular wording for a single connect event.
        if (count == 1)
            return $"Client with id {peerId} connected";

        return $"{FormatCount("client", count)} connected{FormatLastSuffix(count, seconds)}";
    }

    /// <summary>
    /// Formats disconnect lifecycle messages for single or burst events.
    /// </summary>
    /// <param name="count">Disconnect event count.</param>
    /// <param name="peerId">Last peer id seen for this event type.</param>
    /// <param name="seconds">Burst window duration in seconds.</param>
    /// <returns>Formatted disconnect message.</returns>
    private static string FormatDisconnectMessage(int count, uint peerId, double seconds)
    {
        // Use detailed singular wording for a single disconnect event.
        if (count == 1)
            return $"Client with id {peerId} disconnected";

        return $"{FormatCount("client", count)} disconnected{FormatLastSuffix(count, seconds)}";
    }

    /// <summary>
    /// Formats timeout lifecycle messages for single or burst events.
    /// </summary>
    /// <param name="count">Timeout event count.</param>
    /// <param name="peerId">Last peer id seen for this event type.</param>
    /// <param name="seconds">Burst window duration in seconds.</param>
    /// <returns>Formatted timeout message.</returns>
    private static string FormatTimeoutMessage(int count, uint peerId, double seconds)
    {
        // Use detailed singular wording for a single timeout event.
        if (count == 1)
            return $"Client with id {peerId} timed out";

        return $"{FormatCount("client", count)} timed out{FormatLastSuffix(count, seconds)}";
    }
}
