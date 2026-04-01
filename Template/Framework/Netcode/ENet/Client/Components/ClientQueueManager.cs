using ENet;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace __TEMPLATE__.Netcode.Client;

/// <summary>
/// Manages bounded command/incoming/outgoing queues for the ENet client.
/// </summary>
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

    /// <summary>
    /// Initializes queue manager dependencies.
    /// </summary>
    /// <param name="optionsProvider">Provides current ENet options snapshot.</param>
    /// <param name="log">Queue overflow log sink.</param>
    /// <param name="disconnect">Disconnect callback used by noisy-peer policy.</param>
    public ClientQueueManager(Func<ENetOptions?> optionsProvider, Action<string> log, Action<DisconnectOpcode> disconnect)
    {
        _optionsProvider = optionsProvider;
        _log = log;
        _disconnect = disconnect;
    }

    /// <summary>
    /// Gets highest observed depth for command queue.
    /// </summary>
    public int CommandHighWaterMark => _commandMetrics.HighWaterMark;

    /// <summary>
    /// Gets highest observed depth for incoming queue.
    /// </summary>
    public int IncomingHighWaterMark => _incomingMetrics.HighWaterMark;

    /// <summary>
    /// Gets highest observed depth for outgoing queue.
    /// </summary>
    public int OutgoingHighWaterMark => _outgoingMetrics.HighWaterMark;

    /// <summary>
    /// Gets total dropped command count.
    /// </summary>
    public long CommandDroppedCount => _commandMetrics.DroppedCount;

    /// <summary>
    /// Gets total dropped incoming packet count.
    /// </summary>
    public long IncomingDroppedCount => _incomingMetrics.DroppedCount;

    /// <summary>
    /// Gets total dropped outgoing payload count.
    /// </summary>
    public long OutgoingDroppedCount => _outgoingMetrics.DroppedCount;

    /// <summary>
    /// Attempts to dequeue one command.
    /// </summary>
    /// <param name="command">Dequeued command on success.</param>
    /// <returns><see langword="true"/> when a command was dequeued.</returns>
    public bool TryDequeueCommand(out Cmd<ENetClientOpcode>? command)
    {
        // Exit quickly when no command is currently queued.
        if (!_commandQueue.TryDequeue(out command)) return false;
        _commandMetrics.Release();
        return true;
    }

    /// <summary>
    /// Attempts to dequeue one incoming packet.
    /// </summary>
    /// <param name="packet">Dequeued packet on success.</param>
    /// <returns><see langword="true"/> when a packet was dequeued.</returns>
    public bool TryDequeueIncoming(out Packet packet)
    {
        // Exit quickly when no incoming packet is currently queued.
        if (!_incomingQueue.TryDequeue(out packet)) return false;
        _incomingMetrics.Release();
        return true;
    }

    /// <summary>
    /// Attempts to dequeue one outgoing payload.
    /// </summary>
    /// <param name="data">Dequeued payload on success.</param>
    /// <returns><see langword="true"/> when payload was dequeued.</returns>
    public bool TryDequeueOutgoing(out byte[]? data)
    {
        // Exit quickly when no outgoing payload is currently queued.
        if (!_outgoingQueue.TryDequeue(out data)) return false;
        _outgoingMetrics.Release();
        return true;
    }

    /// <summary>
    /// Enqueues a command or applies overflow policy when full.
    /// </summary>
    /// <param name="command">Command to enqueue.</param>
    public void EnqueueCommand(Cmd<ENetClientOpcode> command)
    {
        int limit = GetCommandLimit();

        // Fast path: reserve slot and enqueue when capacity is available.
        if (_commandMetrics.TryReserve(limit, out _)) { _commandQueue.Enqueue(command); return; }
        HandleCommandOverflow(command, limit);
    }

    /// <summary>
    /// Enqueues an incoming packet or applies overflow policy when full.
    /// </summary>
    /// <param name="packet">Packet to enqueue.</param>
    public void EnqueueIncoming(Packet packet)
    {
        int limit = GetIncomingLimit();

        // Fast path: reserve slot and enqueue when capacity is available.
        if (_incomingMetrics.TryReserve(limit, out _)) { _incomingQueue.Enqueue(packet); return; }
        HandleIncomingOverflow(packet, limit);
    }

    /// <summary>
    /// Enqueues an outgoing payload or applies overflow policy when full.
    /// </summary>
    /// <param name="data">Payload to enqueue.</param>
    public void EnqueueOutgoing(byte[] data)
    {
        int limit = GetOutgoingLimit();

        // Fast path: reserve slot and enqueue when capacity is available.
        if (_outgoingMetrics.TryReserve(limit, out _)) { _outgoingQueue.Enqueue(data); return; }
        HandleOutgoingOverflow(data, limit);
    }

    /// <summary>
    /// Resolves command queue overflow using configured policy.
    /// </summary>
    /// <param name="command">New command that triggered overflow.</param>
    /// <param name="limit">Queue capacity limit.</param>
    private void HandleCommandOverflow(Cmd<ENetClientOpcode> command, int limit)
    {
        QueueOverflowPolicy policy = GetCommandPolicy();

        // Drop oldest command first when configured for oldest-drop behavior.
        if (policy == QueueOverflowPolicy.DropOldest && _commandQueue.TryDequeue(out _))
        {
            _commandMetrics.Release();
            long dropped = _commandMetrics.IncrementDropped();

            // Attempt to keep newest command after freeing one queue slot.
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

    /// <summary>
    /// Resolves incoming queue overflow using configured policy.
    /// </summary>
    /// <param name="packet">New packet that triggered overflow.</param>
    /// <param name="limit">Queue capacity limit.</param>
    private void HandleIncomingOverflow(Packet packet, int limit)
    {
        QueueOverflowPolicy policy = GetIncomingPolicy();

        // Drop oldest packet first when configured for oldest-drop behavior.
        if (policy == QueueOverflowPolicy.DropOldest && _incomingQueue.TryDequeue(out Packet droppedPacket))
        {
            _incomingMetrics.Release();
            droppedPacket.Dispose();
            long dropped = _incomingMetrics.IncrementDropped();

            // Attempt to keep newest packet after freeing one queue slot.
            if (_incomingMetrics.TryReserve(limit, out _))
            {
                _incomingQueue.Enqueue(packet);
                LogOverflow("incoming", policy, "Dropped oldest packet and kept newest", dropped, _incomingMetrics.HighWaterMark, limit);
                return;
            }
        }

        // Disconnect local peer when noisy-peer policy is configured.
        if (policy == QueueOverflowPolicy.DisconnectNoisyPeer) _disconnect(DisconnectOpcode.Kicked);
        packet.Dispose();
        long droppedNewest = _incomingMetrics.IncrementDropped();
        string action = policy == QueueOverflowPolicy.DisconnectNoisyPeer
            ? "Disconnected noisy peer and dropped newest packet"
            : "Dropped newest packet";
        LogOverflow("incoming", policy, action, droppedNewest, _incomingMetrics.HighWaterMark, limit);
    }

    /// <summary>
    /// Resolves outgoing queue overflow using configured policy.
    /// </summary>
    /// <param name="data">New payload that triggered overflow.</param>
    /// <param name="limit">Queue capacity limit.</param>
    private void HandleOutgoingOverflow(byte[] data, int limit)
    {
        QueueOverflowPolicy policy = GetOutgoingPolicy();

        // Drop oldest message first when configured for oldest-drop behavior.
        if (policy == QueueOverflowPolicy.DropOldest && _outgoingQueue.TryDequeue(out _))
        {
            _outgoingMetrics.Release();
            long dropped = _outgoingMetrics.IncrementDropped();

            // Attempt to keep newest message after freeing one queue slot.
            if (_outgoingMetrics.TryReserve(limit, out _))
            {
                _outgoingQueue.Enqueue(data);
                LogOverflow("outgoing", policy, "Dropped oldest message and kept newest", dropped, _outgoingMetrics.HighWaterMark, limit);
                return;
            }

            LogOverflow("outgoing", policy, "Dropped oldest message", dropped, _outgoingMetrics.HighWaterMark, limit);
        }

        // Disconnect local peer when noisy-peer policy is configured.
        if (policy == QueueOverflowPolicy.DisconnectNoisyPeer) _disconnect(DisconnectOpcode.Kicked);
        long droppedNewest = _outgoingMetrics.IncrementDropped();
        string action = policy == QueueOverflowPolicy.DisconnectNoisyPeer
            ? "Disconnected peer and dropped newest message"
            : "Dropped newest message";
        LogOverflow("outgoing", policy, action, droppedNewest, _outgoingMetrics.HighWaterMark, limit);
    }

    /// <summary>
    /// Logs queue overflow details with interval-based throttling.
    /// </summary>
    /// <param name="queueName">Queue name identifier.</param>
    /// <param name="policy">Applied overflow policy.</param>
    /// <param name="action">Action performed by policy.</param>
    /// <param name="droppedCount">Total dropped count for queue.</param>
    /// <param name="highWater">Observed high-water mark.</param>
    /// <param name="limit">Configured queue limit.</param>
    private void LogOverflow(string queueName, QueueOverflowPolicy policy, string action, long droppedCount, int highWater, int limit)
    {
        int intervalMs = NormalizePositive(_optionsProvider()?.QueueOverflowLogIntervalMs ?? DefaultQueueOverflowLogIntervalMs, DefaultQueueOverflowLogIntervalMs);
        string key = $"{queueName}:{policy}:{action}";

        // Throttle repeated overflow logs to avoid log spam.
        if (!ShouldLogNow(_overflowLogTicks, key, intervalMs)) return;
        _log($"Queue overflow: queue={queueName}, policy={policy}, action={action}, dropped={droppedCount}, highWater={highWater}, limit={limit}");
    }

    /// <summary>
    /// Gets normalized command queue limit.
    /// </summary>
    /// <returns>Effective command queue limit after validation.</returns>
    private int GetCommandLimit() => NormalizePositive(_optionsProvider()?.MaxCommandQueueDepth ?? DefaultMaxCommandQueueDepth, DefaultMaxCommandQueueDepth);

    /// <summary>
    /// Gets normalized incoming queue limit.
    /// </summary>
    /// <returns>Effective incoming queue limit after validation.</returns>
    private int GetIncomingLimit() => NormalizePositive(_optionsProvider()?.MaxIncomingQueueDepth ?? DefaultMaxIncomingQueueDepth, DefaultMaxIncomingQueueDepth);

    /// <summary>
    /// Gets normalized outgoing queue limit.
    /// </summary>
    /// <returns>Effective outgoing queue limit after validation.</returns>
    private int GetOutgoingLimit() => NormalizePositive(_optionsProvider()?.MaxOutgoingQueueDepth ?? DefaultMaxOutgoingQueueDepth, DefaultMaxOutgoingQueueDepth);

    /// <summary>
    /// Gets command queue overflow policy.
    /// </summary>
    /// <returns>Configured command queue overflow policy.</returns>
    private QueueOverflowPolicy GetCommandPolicy() => _optionsProvider()?.CommandQueueOverflowPolicy ?? QueueOverflowPolicy.DropNewest;

    /// <summary>
    /// Gets incoming queue overflow policy.
    /// </summary>
    /// <returns>Configured incoming queue overflow policy.</returns>
    private QueueOverflowPolicy GetIncomingPolicy() => _optionsProvider()?.IncomingQueueOverflowPolicy ?? QueueOverflowPolicy.DropOldest;

    /// <summary>
    /// Gets outgoing queue overflow policy.
    /// </summary>
    /// <returns>Configured outgoing queue overflow policy.</returns>
    private QueueOverflowPolicy GetOutgoingPolicy() => _optionsProvider()?.OutgoingQueueOverflowPolicy ?? QueueOverflowPolicy.DropOldest;

    /// <summary>
    /// Normalizes configured queue limits to positive values.
    /// </summary>
    /// <param name="configured">Configured value.</param>
    /// <param name="fallback">Fallback value when invalid.</param>
    /// <returns>Positive value.</returns>
    private static int NormalizePositive(int configured, int fallback) => configured > 0 ? configured : fallback;

    /// <summary>
    /// Determines whether throttle interval allows emitting a log now.
    /// </summary>
    /// <param name="throttleMap">Throttle map by log key.</param>
    /// <param name="key">Throttle key.</param>
    /// <param name="intervalMs">Minimum interval between logs.</param>
    /// <returns><see langword="true"/> when log can be emitted.</returns>
    private static bool ShouldLogNow(ConcurrentDictionary<string, long> throttleMap, string key, int intervalMs)
    {
        long now = Stopwatch.GetTimestamp();
        long intervalTicks = (long)(intervalMs * (double)Stopwatch.Frequency / 1000.0);

        // Allow logging when key is new or throttle interval has elapsed.
        if (!throttleMap.TryGetValue(key, out long lastLogged) || now - lastLogged >= intervalTicks)
        {
            throttleMap[key] = now;
            return true;
        }

        return false;
    }
}
