using ENet;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace __TEMPLATE__.Netcode.Server;

internal sealed class ServerQueueManager
{
    private const int DefaultMaxCommandQueueDepth = 1024;
    private const int DefaultMaxIncomingQueueDepth = 4096;
    private const int DefaultMaxOutgoingQueueDepth = 4096;
    private const int DefaultQueueOverflowLogIntervalMs = 2000;

    private readonly Func<ENetOptions?> _optionsProvider;
    private readonly Action<string> _log;
    private readonly Action<uint, DisconnectOpcode> _disconnectPeer;
    private readonly ConcurrentDictionary<string, long> _overflowLogTicks = new();
    private readonly QueueMetrics _commandMetrics = new();
    private readonly QueueMetrics _incomingMetrics = new();
    private readonly QueueMetrics _outgoingMetrics = new();
    private readonly ConcurrentQueue<Cmd<ENetServerOpcode>> _commandQueue = new();
    private readonly ConcurrentQueue<IncomingPacket> _incomingQueue = new();
    private readonly ConcurrentQueue<OutgoingMessage> _outgoingQueue = new();

    public ServerQueueManager(Func<ENetOptions?> optionsProvider, Action<string> log, Action<uint, DisconnectOpcode> disconnectPeer)
    {
        _optionsProvider = optionsProvider;
        _log = log;
        _disconnectPeer = disconnectPeer;
    }

    public int CommandHighWaterMark => _commandMetrics.HighWaterMark;
    public int IncomingHighWaterMark => _incomingMetrics.HighWaterMark;
    public int OutgoingHighWaterMark => _outgoingMetrics.HighWaterMark;
    public long CommandDroppedCount => _commandMetrics.DroppedCount;
    public long IncomingDroppedCount => _incomingMetrics.DroppedCount;
    public long OutgoingDroppedCount => _outgoingMetrics.DroppedCount;

    public bool TryDequeueCommand(out Cmd<ENetServerOpcode>? command)
    {
        if (!_commandQueue.TryDequeue(out command)) return false;
        _commandMetrics.Release();
        return true;
    }

    public bool TryDequeueIncoming(out IncomingPacket? packet)
    {
        if (!_incomingQueue.TryDequeue(out packet)) return false;
        _incomingMetrics.Release();
        return true;
    }

    public bool TryDequeueOutgoing(out OutgoingMessage? message)
    {
        if (!_outgoingQueue.TryDequeue(out message)) return false;
        _outgoingMetrics.Release();
        return true;
    }

    public void EnqueueCommand(Cmd<ENetServerOpcode> command)
    {
        int limit = GetCommandQueueLimit();
        if (_commandMetrics.TryReserve(limit, out _)) { _commandQueue.Enqueue(command); return; }
        HandleCommandOverflow(command, limit);
    }

    public void EnqueueIncomingPacket(Packet packet, Peer peer)
    {
        int limit = GetIncomingQueueLimit();
        if (_incomingMetrics.TryReserve(limit, out _)) { _incomingQueue.Enqueue(new IncomingPacket(packet, peer)); return; }
        HandleIncomingOverflow(packet, peer, limit);
    }

    public void EnqueueOutgoing(OutgoingMessage message)
    {
        int limit = GetOutgoingQueueLimit();
        if (_outgoingMetrics.TryReserve(limit, out _)) { _outgoingQueue.Enqueue(message); return; }
        HandleOutgoingOverflow(message, limit);
    }

    private void HandleCommandOverflow(Cmd<ENetServerOpcode> command, int limit)
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

    private void HandleIncomingOverflow(Packet packet, Peer peer, int limit)
    {
        QueueOverflowPolicy policy = GetIncomingPolicy();
        if (policy == QueueOverflowPolicy.DropOldest && _incomingQueue.TryDequeue(out IncomingPacket? droppedPacket))
        {
            _incomingMetrics.Release();
            droppedPacket.Packet.Dispose();
            long dropped = _incomingMetrics.IncrementDropped();
            if (_incomingMetrics.TryReserve(limit, out _))
            {
                _incomingQueue.Enqueue(new IncomingPacket(packet, peer));
                LogOverflow("incoming", policy, "Dropped oldest packet and kept newest", dropped, _incomingMetrics.HighWaterMark, limit);
                return;
            }
        }

        if (policy == QueueOverflowPolicy.DisconnectNoisyPeer) _disconnectPeer(peer.ID, DisconnectOpcode.Kicked);
        packet.Dispose();
        long droppedNewest = _incomingMetrics.IncrementDropped();
        string action = policy == QueueOverflowPolicy.DisconnectNoisyPeer
            ? "Disconnected noisy peer and dropped newest packet"
            : "Dropped newest packet";
        LogOverflow("incoming", policy, action, droppedNewest, _incomingMetrics.HighWaterMark, limit);
    }

    private void HandleOutgoingOverflow(OutgoingMessage message, int limit)
    {
        QueueOverflowPolicy policy = GetOutgoingPolicy();
        if (policy == QueueOverflowPolicy.DropOldest && _outgoingQueue.TryDequeue(out _))
        {
            _outgoingMetrics.Release();
            long dropped = _outgoingMetrics.IncrementDropped();
            if (_outgoingMetrics.TryReserve(limit, out _))
            {
                _outgoingQueue.Enqueue(message);
                LogOverflow("outgoing", policy, "Dropped oldest message and kept newest", dropped, _outgoingMetrics.HighWaterMark, limit);
                return;
            }
        }

        if (policy == QueueOverflowPolicy.DisconnectNoisyPeer && !message.IsBroadcast)
            _disconnectPeer(message.TargetPeerId, DisconnectOpcode.Kicked);

        long droppedNewest = _outgoingMetrics.IncrementDropped();
        string action = policy == QueueOverflowPolicy.DisconnectNoisyPeer
            ? "Dropped newest message (and disconnected target peer for unicast)"
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

    private int GetCommandQueueLimit() => NormalizePositive(_optionsProvider()?.MaxCommandQueueDepth ?? DefaultMaxCommandQueueDepth, DefaultMaxCommandQueueDepth);
    private int GetIncomingQueueLimit() => NormalizePositive(_optionsProvider()?.MaxIncomingQueueDepth ?? DefaultMaxIncomingQueueDepth, DefaultMaxIncomingQueueDepth);
    private int GetOutgoingQueueLimit() => NormalizePositive(_optionsProvider()?.MaxOutgoingQueueDepth ?? DefaultMaxOutgoingQueueDepth, DefaultMaxOutgoingQueueDepth);
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
