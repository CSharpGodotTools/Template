using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Framework.Netcode.Server;

/// <summary>
/// Coalesces rapid server lifecycle events into summarised log messages.
/// Not thread-safe: expected to run on a single ENet worker thread.
/// </summary>
internal sealed class ServerLogAggregator
{
    private const double QuietGapSeconds = 0.5;
    private const double MaxWindowSeconds = 5.0;

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
    public void RecordConnect(uint peerId)
    {
        _connectedCount++;
        _lastConnectPeerId = peerId;
        MarkEvent(ref _lastConnectTicks);
    }

    /// <summary>
    /// Records a disconnect lifecycle event.
    /// </summary>
    public void RecordDisconnect(uint peerId)
    {
        _disconnectedCount++;
        _lastDisconnectPeerId = peerId;
        MarkEvent(ref _lastDisconnectTicks);
    }

    /// <summary>
    /// Records a timeout lifecycle event.
    /// </summary>
    public void RecordTimeout(uint peerId)
    {
        _timeoutCount++;
        _lastTimeoutPeerId = peerId;
        MarkEvent(ref _lastTimeoutTicks);
    }

    /// <summary>
    /// Emits a coalesced lifecycle log report when burst thresholds are reached.
    /// </summary>
    public void Flush(Action<string> log)
    {
        if (_connectedCount == 0 && _disconnectedCount == 0 && _timeoutCount == 0)
        {
            return;
        }

        if (_windowStartTicks == 0 || _lastEventTicks == 0)
        {
            return;
        }

        long nowTicks = Stopwatch.GetTimestamp();
        double sinceLast = (nowTicks - _lastEventTicks) / (double)Stopwatch.Frequency;
        double windowSeconds = (_lastEventTicks - _windowStartTicks) / (double)Stopwatch.Frequency;

        if (sinceLast < QuietGapSeconds && windowSeconds < MaxWindowSeconds)
        {
            return;
        }

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
        List<(long Tick, Action LogAction)> logEntries = new(3);

        if (connects > 0)
        {
            logEntries.Add((lastConnectTicks, () => log(FormatConnectMessage(connects, lastConnectPeerId, reportSeconds))));
        }

        if (disconnects > 0)
        {
            logEntries.Add((lastDisconnectTicks, () => log(FormatDisconnectMessage(disconnects, lastDisconnectPeerId, reportSeconds))));
        }

        if (timeouts > 0)
        {
            logEntries.Add((lastTimeoutTicks, () => log(FormatTimeoutMessage(timeouts, lastTimeoutPeerId, reportSeconds))));
        }

        logEntries.Sort(static (left, right) => left.Tick.CompareTo(right.Tick));

        foreach ((long Tick, Action LogAction) in logEntries)
        {
            LogAction();
        }
    }

    private void MarkEvent(ref long eventTypeLastTicks)
    {
        long nowTicks = Stopwatch.GetTimestamp();

        if (_windowStartTicks == 0)
        {
            _windowStartTicks = nowTicks;
        }

        _lastEventTicks = nowTicks;
        eventTypeLastTicks = nowTicks;
    }

    private static string FormatCount(string singular, int count)
    {
        if (count == 1)
        {
            return $"1 {singular}";
        }

        return $"{count} {singular}s";
    }

    private static string FormatLastSuffix(int count, double seconds)
    {
        if (count == 1)
        {
            return string.Empty;
        }

        return $" (last {seconds:0.##}s)";
    }

    private static string FormatConnectMessage(int count, uint peerId, double seconds)
    {
        if (count == 1)
        {
            return $"Client with id {peerId} connected";
        }

        return $"{FormatCount("client", count)} connected{FormatLastSuffix(count, seconds)}";
    }

    private static string FormatDisconnectMessage(int count, uint peerId, double seconds)
    {
        if (count == 1)
        {
            return $"Client with id {peerId} disconnected";
        }

        return $"{FormatCount("client", count)} disconnected{FormatLastSuffix(count, seconds)}";
    }

    private static string FormatTimeoutMessage(int count, uint peerId, double seconds)
    {
        if (count == 1)
        {
            return $"Client with id {peerId} timed out";
        }

        return $"{FormatCount("client", count)} timed out{FormatLastSuffix(count, seconds)}";
    }
}
