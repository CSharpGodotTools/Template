using ENet;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace __TEMPLATE__.Netcode.Client;

internal sealed class ClientQueueManager
{
    private const int DefaultMaxCommandQueueDepth = 1024;
    private const int DefaultMaxIncomingQueueDepth = 4096;
    private const int DefaultMaxOutgoingQueueDepth = 4096;
    private const int DefaultQueueOverflowLogIntervalMs = 2000;

    private readonly Func<ENetOptions?> _optionsProvider;
    private readonly Action<string> _log;
    private readonly Action<DisconnectOpcode> _disconnect;
    private readonly ConcurrentDictionary<string, long> _overflowLogTicks = new();
    private readonly QueueMetrics _commandMetrics = new();
    private readonly QueueMetrics _incomingMetrics = new();
    private readonly QueueMetrics _outgoingMetrics = new();
    private readonly ConcurrentQueue<Cmd<ENetClientOpcode>> _commandQueue = new();
    private readonly ConcurrentQueue<Packet> _incomingQueue = new();
    private readonly ConcurrentQueue<byte[]> _outgoingQueue = new();

    public ClientQueueManager(Func<ENetOptions?> optionsProvider, Action<string> log, Action<DisconnectOpcode> disconnect)
    {
        _optionsProvider = optionsProvider;
        _log = log;
        _disconnect = disconnect;
    }

    public int CommandHighWaterMark => _commandMetrics.HighWaterMark;
    public int IncomingHighWaterMark => _incomingMetrics.HighWaterMark;
    public int OutgoingHighWaterMark => _outgoingMetrics.HighWaterMark;
    public long CommandDroppedCount => _commandMetrics.DroppedCount;
    public long IncomingDroppedCount => _incomingMetrics.DroppedCount;
    public long OutgoingDroppedCount => _outgoingMetrics.DroppedCount;

    public bool TryDequeueCommand(out Cmd<ENetClientOpcode>? command)
    {
        if (!_commandQueue.TryDequeue(out command)) return false;
        _commandMetrics.Release();
        return true;
    }

    public bool TryDequeueIncoming(out Packet packet)
    {
        if (!_incomingQueue.TryDequeue(out packet)) return false;
        _incomingMetrics.Release();
        return true;
    }

    public bool TryDequeueOutgoing(out byte[]? data)
    {
        if (!_outgoingQueue.TryDequeue(out data)) return false;
        _outgoingMetrics.Release();
        return true;
    }

    public void EnqueueCommand(Cmd<ENetClientOpcode> command)
    {
        int limit = GetCommandLimit();
        if (_commandMetrics.TryReserve(limit, out _)) { _commandQueue.Enqueue(command); return; }
        HandleCommandOverflow(command, limit);
    }

    public void EnqueueIncoming(Packet packet)
    {
        int limit = GetIncomingLimit();
        if (_incomingMetrics.TryReserve(limit, out _)) { _incomingQueue.Enqueue(packet); return; }
        HandleIncomingOverflow(packet, limit);
    }

    public void EnqueueOutgoing(byte[] data)
    {
        int limit = GetOutgoingLimit();
        if (_outgoingMetrics.TryReserve(limit, out _)) { _outgoingQueue.Enqueue(data); return; }
        HandleOutgoingOverflow(data, limit);
    }

    private void HandleCommandOverflow(Cmd<ENetClientOpcode> command, int limit)
    {
        QueueOverflowPolicy policy = GetCommandPolicy();
        if (policy == QueueOverflowPolicy.DropOldest && _commandQueue.TryDequeue(out _))
        {
            _commandMetrics.Release();
            long dropped = _commandMetrics.IncrementDropped();
            if (_commandMetrics.TryReserve(limit, out _))
            {
                _commandQueue.Enqueue(command);
                LogOverflow("command", policy, "Dropped oldest command and kept newest", dropped, _commandMetrics.HighWaterMark, limit);
                return;
            }
        }

        long droppedNewest = _commandMetrics.IncrementDropped();
        string action = policy == QueueOverflowPolicy.DisconnectNoisyPeer
            ? "DisconnectNoisyPeer unsupported for command queue; dropped newest command"
            : "Dropped newest command";
        LogOverflow("command", policy, action, droppedNewest, _commandMetrics.HighWaterMark, limit);
    }

    private void HandleIncomingOverflow(Packet packet, int limit)
    {
        QueueOverflowPolicy policy = GetIncomingPolicy();
        if (policy == QueueOverflowPolicy.DropOldest && _incomingQueue.TryDequeue(out Packet droppedPacket))
        {
            _incomingMetrics.Release();
            droppedPacket.Dispose();
            long dropped = _incomingMetrics.IncrementDropped();
            if (_incomingMetrics.TryReserve(limit, out _))
            {
                _incomingQueue.Enqueue(packet);
                LogOverflow("incoming", policy, "Dropped oldest packet and kept newest", dropped, _incomingMetrics.HighWaterMark, limit);
                return;
            }
        }

        if (policy == QueueOverflowPolicy.DisconnectNoisyPeer) _disconnect(DisconnectOpcode.Kicked);
        packet.Dispose();
        long droppedNewest = _incomingMetrics.IncrementDropped();
        string action = policy == QueueOverflowPolicy.DisconnectNoisyPeer
            ? "Disconnected noisy peer and dropped newest packet"
            : "Dropped newest packet";
        LogOverflow("incoming", policy, action, droppedNewest, _incomingMetrics.HighWaterMark, limit);
    }

    private void HandleOutgoingOverflow(byte[] data, int limit)
    {
        QueueOverflowPolicy policy = GetOutgoingPolicy();
        if (policy == QueueOverflowPolicy.DropOldest && _outgoingQueue.TryDequeue(out _))
        {
            _outgoingMetrics.Release();
            long dropped = _outgoingMetrics.IncrementDropped();
            if (_outgoingMetrics.TryReserve(limit, out _))
            {
                _outgoingQueue.Enqueue(data);
                LogOverflow("outgoing", policy, "Dropped oldest message and kept newest", dropped, _outgoingMetrics.HighWaterMark, limit);
                return;
            }

            LogOverflow("outgoing", policy, "Dropped oldest message", dropped, _outgoingMetrics.HighWaterMark, limit);
        }

        if (policy == QueueOverflowPolicy.DisconnectNoisyPeer) _disconnect(DisconnectOpcode.Kicked);
        long droppedNewest = _outgoingMetrics.IncrementDropped();
        string action = policy == QueueOverflowPolicy.DisconnectNoisyPeer
            ? "Disconnected peer and dropped newest message"
            : "Dropped newest message";
        LogOverflow("outgoing", policy, action, droppedNewest, _outgoingMetrics.HighWaterMark, limit);
    }

    private void LogOverflow(string queueName, QueueOverflowPolicy policy, string action, long droppedCount, int highWater, int limit)
    {
        int intervalMs = NormalizePositive(_optionsProvider()?.QueueOverflowLogIntervalMs ?? DefaultQueueOverflowLogIntervalMs, DefaultQueueOverflowLogIntervalMs);
        string key = $"{queueName}:{policy}:{action}";
        if (!ShouldLogNow(_overflowLogTicks, key, intervalMs)) return;
        _log($"Queue overflow: queue={queueName}, policy={policy}, action={action}, dropped={droppedCount}, highWater={highWater}, limit={limit}");
    }

    private int GetCommandLimit() => NormalizePositive(_optionsProvider()?.MaxCommandQueueDepth ?? DefaultMaxCommandQueueDepth, DefaultMaxCommandQueueDepth);
    private int GetIncomingLimit() => NormalizePositive(_optionsProvider()?.MaxIncomingQueueDepth ?? DefaultMaxIncomingQueueDepth, DefaultMaxIncomingQueueDepth);
    private int GetOutgoingLimit() => NormalizePositive(_optionsProvider()?.MaxOutgoingQueueDepth ?? DefaultMaxOutgoingQueueDepth, DefaultMaxOutgoingQueueDepth);
    private QueueOverflowPolicy GetCommandPolicy() => _optionsProvider()?.CommandQueueOverflowPolicy ?? QueueOverflowPolicy.DropNewest;
    private QueueOverflowPolicy GetIncomingPolicy() => _optionsProvider()?.IncomingQueueOverflowPolicy ?? QueueOverflowPolicy.DropOldest;
    private QueueOverflowPolicy GetOutgoingPolicy() => _optionsProvider()?.OutgoingQueueOverflowPolicy ?? QueueOverflowPolicy.DropOldest;
    private static int NormalizePositive(int configured, int fallback) => configured > 0 ? configured : fallback;

    private static bool ShouldLogNow(ConcurrentDictionary<string, long> throttleMap, string key, int intervalMs)
    {
        long now = Stopwatch.GetTimestamp();
        long intervalTicks = (long)(intervalMs * (double)Stopwatch.Frequency / 1000.0);
        if (!throttleMap.TryGetValue(key, out long lastLogged) || now - lastLogged >= intervalTicks)
        {
            throttleMap[key] = now;
            return true;
        }

        return false;
    }
}
