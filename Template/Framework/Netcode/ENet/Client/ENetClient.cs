using ENet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace __TEMPLATE__.Netcode.Client;

// ENet API Reference: https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md
/// <summary>
/// Base ENet client worker that owns the connection lifecycle, send queue, and packet dispatch.
/// Extend <see cref="GodotClient"/> for game-level packet registration.
/// </summary>
public abstract class ENetClient : ENetLow
{
    private const string LogTag = "Client";
    private const int DefaultMaxCommandQueueDepth = 1024;
    private const int DefaultMaxIncomingQueueDepth = 4096;
    private const int DefaultMaxOutgoingQueueDepth = 4096;
    private const int DefaultQueueOverflowLogIntervalMs = 2000;
    private const int DefaultMalformedFragmentLogIntervalMs = 2000;

    private readonly ConcurrentQueue<Cmd<ENetClientOpcode>> _enetCmds = new();
    private readonly ConcurrentQueue<byte[]> _outgoing = new();
    private readonly ConcurrentQueue<Packet> _incoming = new();
    private readonly ConcurrentDictionary<string, long> _queueOverflowLogTicks = new();
    private readonly ConcurrentDictionary<string, long> _malformedFragmentLogTicks = new();
    private static readonly ClientLogAggregator _logAggregator = new();
    private static int _activeClientWorkers;
    private Peer _peer;
    private ushort _streamCounter;
    private readonly Dictionary<ushort, FragmentBuffer> _reassemblyBuffers = [];
    private int _enetCmdDepth;
    private int _incomingDepth;
    private int _outgoingDepth;
    private int _enetCmdHighWaterMark;
    private int _incomingHighWaterMark;
    private int _outgoingHighWaterMark;
    private long _enetCmdDroppedCount;
    private long _incomingDroppedCount;
    private long _outgoingDroppedCount;

    /// <summary>Relay queue for lifecycle commands that must be processed on the Godot main thread.</summary>
    protected ConcurrentQueue<Cmd<GodotOpcode>> MainThreadCommands { get; } = new();

    /// <summary>Relay queue for incoming packet data that must be dispatched on the Godot main thread.</summary>
    protected ConcurrentQueue<PacketData> MainThreadPackets { get; } = new();
    protected long _connected;

    /// <summary>
    /// The ping interval in ms. The default is 1000.
    /// </summary>
    protected virtual uint PingIntervalMs { get; } = 1000;

    /// <summary>
    /// The peer timeout in ms. The default is 5000.
    /// </summary>
    protected virtual uint PeerTimeoutMs { get; } = 5000;

    /// <summary>
    /// The peer timeout minimum in ms. The default is 5000.
    /// </summary>
    protected virtual uint PeerTimeoutMinimumMs { get; } = 5000;

    /// <summary>
    /// The peer timeout maximum in ms. The default is 5000.
    /// </summary>
    protected virtual uint PeerTimeoutMaximumMs { get; } = 5000;

    /// <summary>ENet peer ID assigned by the server for this connection.</summary>
    public uint PeerId => _peer.ID;
    public int CommandQueueHighWaterMark => Volatile.Read(ref _enetCmdHighWaterMark);
    public int IncomingQueueHighWaterMark => Volatile.Read(ref _incomingHighWaterMark);
    public int OutgoingQueueHighWaterMark => Volatile.Read(ref _outgoingHighWaterMark);
    public long CommandQueueDroppedCount => Interlocked.Read(ref _enetCmdDroppedCount);
    public long IncomingQueueDroppedCount => Interlocked.Read(ref _incomingDroppedCount);
    public long OutgoingQueueDroppedCount => Interlocked.Read(ref _outgoingDroppedCount);

    /// <summary>
    /// Logs a message as the client.
    /// </summary>
    public sealed override void Log(object message, BBColor color = BBColor.Gray)
    {
        string timestampPrefix = BuildTimestampPrefix();
        LoggerService.Log($"{timestampPrefix}[Client] {message}", color);
    }

    /// <summary>
    /// Processes client worker queues each network tick.
    /// </summary>
    protected sealed override void ConcurrentQueues()
    {
        ProcessENetCommands();
        ProcessIncomingPackets();
        ProcessOutgoingPackets();
        _logAggregator.Flush(message => Log(message), false);
    }

    /// <summary>
    /// Called on the worker thread when the connection is established; override to send initial packets.
    /// </summary>
    protected virtual void OnConnected()
    {
    }

    /// <summary>
    /// Called on the worker thread when the server disconnects the client.
    /// </summary>
    protected virtual void OnDisconnected()
    {
    }

    /// <summary>
    /// Called on the worker thread when the connection times out.
    /// </summary>
    protected virtual void OnTimedOut()
    {
    }

    /// <summary>
    /// Internal connect handler that updates state and dispatches lifecycle callbacks.
    /// </summary>
    protected sealed override void OnConnectLow(Event netEvent)
    {
        Interlocked.Exchange(ref _connected, 1);
        MainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Connected));
        _logAggregator.RecordConnect(netEvent.Peer.ID);
        TryInvoke(OnConnected);
    }

    /// <summary>
    /// Internal disconnect handler that updates state and dispatches lifecycle callbacks.
    /// </summary>
    protected sealed override void OnDisconnectLow(Event netEvent)
    {
        DisconnectOpcode opcode = (DisconnectOpcode)netEvent.Data;
        QueueDisconnectedCommand(opcode);

        OnDisconnectCleanup(netEvent.Peer);
        _logAggregator.RecordDisconnect(netEvent.Peer.ID);
        TryInvoke(OnDisconnected);
    }

    /// <summary>
    /// Internal timeout handler that updates state and dispatches lifecycle callbacks.
    /// </summary>
    protected sealed override void OnTimeoutLow(Event netEvent)
    {
        QueueDisconnectedCommand(DisconnectOpcode.Timeout);
        MainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Timeout));

        OnDisconnectCleanup(netEvent.Peer);
        _logAggregator.RecordTimeout(netEvent.Peer.ID);
        TryInvoke(OnTimedOut);
    }

    /// <summary>
    /// Internal receive handler that validates packet size and enqueues payloads.
    /// </summary>
    protected sealed override void OnReceiveLow(Event netEvent)
    {
        Packet packet = netEvent.Packet;

        if (packet.Length > GamePacket.MaxSize)
        {
            Log($"Tried to read packet from server of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
            packet.Dispose();
            return;
        }

        EnqueueIncomingPacket(packet);
    }

    /// <summary>
    /// Clears client connection state and executes shared disconnect cleanup.
    /// </summary>
    protected sealed override void OnDisconnectCleanup(Peer peer)
    {
        base.OnDisconnectCleanup(peer);
        Interlocked.Exchange(ref _connected, 0);
        _reassemblyBuffers.Clear();
    }

    /// <summary>
    /// Runs the ENet client worker loop for a single connection attempt.
    /// </summary>
    protected void WorkerThread(string ip, ushort port)
    {
        Interlocked.Exchange(ref _running, 1);
        Interlocked.Increment(ref _activeClientWorkers);
        Host = new Host();

        try
        {
            Host.Create();
            _peer = Host.Connect(CreateAddress(ip, port));
            _peer.PingInterval(PingIntervalMs);
            _peer.Timeout(PeerTimeoutMs, PeerTimeoutMinimumMs, PeerTimeoutMaximumMs);

            WorkerLoop();
        }
        finally
        {
            Host.Dispose();
            Interlocked.Exchange(ref _running, 0);

            if (Interlocked.Decrement(ref _activeClientWorkers) == 0)
            {
                _logAggregator.Flush(message => Log(message), true);
            }
        }
    }

    /// <summary>
    /// Returns a formatted timestamp prefix when timestamp logging is enabled.
    /// </summary>
    private string BuildTimestampPrefix()
    {
        if (Options == null || !Options.ShowLogTimestamps)
        {
            return string.Empty;
        }

        return $"[{DateTime.Now:HH:mm:ss}] ";
    }

    /// <summary>
    /// Drains the ENet command queue and executes pending disconnect commands.
    /// </summary>
    private void ProcessENetCommands()
    {
        while (_enetCmds.TryDequeue(out Cmd<ENetClientOpcode>? command))
        {
            Interlocked.Decrement(ref _enetCmdDepth);

            switch (command.Opcode)
            {
                case ENetClientOpcode.Disconnect:
                    HandleDisconnectCommand();
                    break;
            }
        }
    }

    /// <summary>
    /// Sends a disconnect request to the server peer.
    /// </summary>
    private void HandleDisconnectCommand()
    {
        if (CTS.IsCancellationRequested)
        {
            Log("Client is in the middle of stopping");
            return;
        }

        _peer.Disconnect((uint)DisconnectOpcode.Disconnected);
    }

    /// <summary>
    /// Enqueues a disconnected lifecycle command for the Godot main thread.
    /// </summary>
    private void QueueDisconnectedCommand(DisconnectOpcode opcode)
    {
        MainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Disconnected, opcode));
    }

    /// <summary>
    /// Drains the incoming packet queue, reassembling fragments and staging data for main-thread dispatch.
    /// </summary>
    private void ProcessIncomingPackets()
    {
        while (_incoming.TryDequeue(out Packet packet))
        {
            Interlocked.Decrement(ref _incomingDepth);

            // Copy bytes eagerly so we can inspect the opcode before any higher-level parsing.
            byte[] bytes = new byte[packet.Length];
            packet.CopyTo(bytes);
            packet.Dispose();

            if (PacketFragmenter.IsFragment(bytes))
            {
                HandleFragmentBytes(bytes);
                continue;
            }

            if (!TryCreatePacketData(bytes, out PacketData? packetData))
                continue;

            MainThreadPackets.Enqueue(packetData!);
        }
    }

    /// <summary>
    /// Accumulates a fragment and stages the reassembled payload for main-thread dispatch when complete.
    /// </summary>
    private void HandleFragmentBytes(byte[] fragmentBytes)
    {
        if (!PacketFragmenter.TryReadHeader(fragmentBytes, out ushort streamId, out ushort fragIndex, out ushort totalFragments))
        {
            LogMalformedFragmentDrop("Fragment header was truncated.");
            return;
        }

        if (!PacketFragmenter.IsValidHeader(fragIndex, totalFragments, GetMaxFragmentsPerPacket(), out string validationError))
        {
            LogMalformedFragmentDrop($"stream={streamId}: {validationError}");
            return;
        }

        if (!_reassemblyBuffers.TryGetValue(streamId, out FragmentBuffer? buffer))
        {
            buffer = new FragmentBuffer(totalFragments);
            _reassemblyBuffers[streamId] = buffer;
        }
        else if (buffer.TotalFragments != totalFragments)
        {
            _reassemblyBuffers.Remove(streamId);
            LogMalformedFragmentDrop($"stream={streamId}: fragment count changed from {buffer.TotalFragments} to {totalFragments}.");
            return;
        }

        byte[] payload = PacketFragmenter.ExtractPayload(fragmentBytes);
        if (!buffer.Add(fragIndex, payload))
            return;

        _reassemblyBuffers.Remove(streamId);

        if (!TryCreatePacketData(buffer.Assemble(), out PacketData? packetData))
            return;

        MainThreadPackets.Enqueue(packetData!);
    }

    /// <summary>
    /// Reads the wire opcode and builds a <see cref="PacketData"/> record ready for main-thread dispatch.
    /// </summary>
    private bool TryCreatePacketData(byte[] bytes, out PacketData? packetData)
    {
        packetData = null;
        PacketReader reader = new(bytes);

        if (!TryReadPacketType(reader, out Type? packetType))
        {
            reader.Dispose();
            return false;
        }

        // The packet registry vends a shared singleton per packet type. Handlers must not
        // retain a reference to this instance across processing boundaries — the same object
        // is reused and mutated for every subsequent packet of the same type.
        ServerPacket handlerPacket = PacketRegistry.ServerPacketInfo[packetType!].Instance;
        packetData = new PacketData
        {
            Type = packetType!,
            PacketReader = reader,
            HandlePacket = handlerPacket
        };

        return true;
    }

    /// <summary>
    /// Reads the opcode from the reader and resolves the matching <see cref="ServerPacket"/> type.
    /// </summary>
    private bool TryReadPacketType(PacketReader reader, out Type? packetType)
    {
        packetType = null;

        ushort opcode;
        try
        {
            opcode = PacketRegistry.ReadOpcodeFromReader(reader);
        }
        catch (EndOfStreamException exception)
        {
            Log($"Received malformed packet: {exception.Message} (Ignoring)");
            return false;
        }

        if (!PacketRegistry.ServerPacketTypesWire.TryGetValue(opcode, out packetType))
        {
            Log($"Received malformed opcode: {opcode} (Ignoring)");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Drains the outgoing queue and transmits each payload over ENet, fragmenting when needed.
    /// </summary>
    private void ProcessOutgoingPackets()
    {
        while (_outgoing.TryDequeue(out byte[]? data))
        {
            Interlocked.Decrement(ref _outgoingDepth);

            try
            {
                if (data.Length > GamePacket.MaxSize)
                {
                    foreach (byte[] fragment in PacketFragmenter.Fragment(data, _streamCounter++, GetMaxFragmentsPerPacket()))
                    {
                        Packet fragPacket = CreateReliablePacket(fragment);
                        _peer.Send(DefaultChannelId, ref fragPacket);
                    }

                    continue;
                }

                Packet enetPacket = CreateReliablePacket(data);
                _peer.Send(DefaultChannelId, ref enetPacket);
            }
            catch (InvalidOperationException exception)
            {
                LoggerService.LogErr(exception, $"{LogTag}: invalid outgoing packet state");
            }
            catch (Exception exception)
            {
                LoggerService.LogErr(exception, LogTag);
            }
        }
    }

    /// <summary>
    /// Enqueues serialized packet data for sending on the worker thread.
    /// </summary>
    protected void EnqueueOutgoing(byte[] data)
    {
        EnqueueOutgoingData(data);
    }

    /// <summary>
    /// Requests a graceful disconnect from the worker thread.
    /// </summary>
    protected void RequestDisconnect()
    {
        EnqueueCommand(new Cmd<ENetClientOpcode>(ENetClientOpcode.Disconnect));
    }

    private void EnqueueCommand(Cmd<ENetClientOpcode> command)
    {
        int limit = GetCommandQueueLimit();
        if (TryReserveQueueSlot(ref _enetCmdDepth, limit, out int depth))
        {
            _enetCmds.Enqueue(command);
            UpdateHighWaterMark(ref _enetCmdHighWaterMark, depth);
            return;
        }

        HandleCommandQueueOverflow(command, limit);
    }

    private void EnqueueIncomingPacket(Packet packet)
    {
        int limit = GetIncomingQueueLimit();
        if (TryReserveQueueSlot(ref _incomingDepth, limit, out int depth))
        {
            _incoming.Enqueue(packet);
            UpdateHighWaterMark(ref _incomingHighWaterMark, depth);
            return;
        }

        HandleIncomingQueueOverflow(packet, limit);
    }

    private void EnqueueOutgoingData(byte[] data)
    {
        int limit = GetOutgoingQueueLimit();
        if (TryReserveQueueSlot(ref _outgoingDepth, limit, out int depth))
        {
            _outgoing.Enqueue(data);
            UpdateHighWaterMark(ref _outgoingHighWaterMark, depth);
            return;
        }

        HandleOutgoingQueueOverflow(data, limit);
    }

    private void HandleCommandQueueOverflow(Cmd<ENetClientOpcode> command, int limit)
    {
        QueueOverflowPolicy policy = GetCommandQueueOverflowPolicy();

        if (policy == QueueOverflowPolicy.DropOldest && _enetCmds.TryDequeue(out _))
        {
            Interlocked.Decrement(ref _enetCmdDepth);
            long dropped = Interlocked.Increment(ref _enetCmdDroppedCount);

            if (TryReserveQueueSlot(ref _enetCmdDepth, limit, out int depth))
            {
                _enetCmds.Enqueue(command);
                UpdateHighWaterMark(ref _enetCmdHighWaterMark, depth);
                LogQueueOverflow(
                    queueName: "command",
                    policy,
                    action: "Dropped oldest command and kept newest",
                    dropped,
                    Volatile.Read(ref _enetCmdHighWaterMark),
                    limit);
                return;
            }
        }

        long droppedNewest = Interlocked.Increment(ref _enetCmdDroppedCount);
        string action = policy == QueueOverflowPolicy.DisconnectNoisyPeer
            ? "DisconnectNoisyPeer unsupported for command queue; dropped newest command"
            : "Dropped newest command";

        LogQueueOverflow(
            queueName: "command",
            policy,
            action,
            droppedNewest,
            Volatile.Read(ref _enetCmdHighWaterMark),
            limit);
    }

    private void HandleIncomingQueueOverflow(Packet packet, int limit)
    {
        QueueOverflowPolicy policy = GetIncomingQueueOverflowPolicy();

        if (policy == QueueOverflowPolicy.DropOldest && _incoming.TryDequeue(out Packet droppedPacket))
        {
            Interlocked.Decrement(ref _incomingDepth);
            droppedPacket.Dispose();
            long dropped = Interlocked.Increment(ref _incomingDroppedCount);

            if (TryReserveQueueSlot(ref _incomingDepth, limit, out int depth))
            {
                _incoming.Enqueue(packet);
                UpdateHighWaterMark(ref _incomingHighWaterMark, depth);
                LogQueueOverflow(
                    queueName: "incoming",
                    policy,
                    action: "Dropped oldest packet and kept newest",
                    dropped,
                    Volatile.Read(ref _incomingHighWaterMark),
                    limit);
                return;
            }
        }

        if (policy == QueueOverflowPolicy.DisconnectNoisyPeer)
        {
            _peer.Disconnect((uint)DisconnectOpcode.Kicked);
        }

        packet.Dispose();
        long droppedNewest = Interlocked.Increment(ref _incomingDroppedCount);
        LogQueueOverflow(
            queueName: "incoming",
            policy,
            action: policy == QueueOverflowPolicy.DisconnectNoisyPeer
                ? "Disconnected noisy peer and dropped newest packet"
                : "Dropped newest packet",
            droppedNewest,
            Volatile.Read(ref _incomingHighWaterMark),
            limit);
    }

    private void HandleOutgoingQueueOverflow(byte[] data, int limit)
    {
        QueueOverflowPolicy policy = GetOutgoingQueueOverflowPolicy();

        if (policy == QueueOverflowPolicy.DropOldest && _outgoing.TryDequeue(out _))
        {
            Interlocked.Decrement(ref _outgoingDepth);
            long dropped = Interlocked.Increment(ref _outgoingDroppedCount);

            if (TryReserveQueueSlot(ref _outgoingDepth, limit, out int depth))
            {
                _outgoing.Enqueue(data);
                UpdateHighWaterMark(ref _outgoingHighWaterMark, depth);
                LogQueueOverflow(
                    queueName: "outgoing",
                    policy,
                    action: "Dropped oldest message and kept newest",
                    dropped,
                    Volatile.Read(ref _outgoingHighWaterMark),
                    limit);
                return;
            }

            LogQueueOverflow(
                queueName: "outgoing",
                policy,
                action: "Dropped oldest message",
                dropped,
                Volatile.Read(ref _outgoingHighWaterMark),
                limit);
        }

        if (policy == QueueOverflowPolicy.DisconnectNoisyPeer)
        {
            _peer.Disconnect((uint)DisconnectOpcode.Kicked);
        }

        long droppedNewest = Interlocked.Increment(ref _outgoingDroppedCount);
        LogQueueOverflow(
            queueName: "outgoing",
            policy,
            action: policy == QueueOverflowPolicy.DisconnectNoisyPeer
                ? "Disconnected peer and dropped newest message"
                : "Dropped newest message",
            droppedNewest,
            Volatile.Read(ref _outgoingHighWaterMark),
            limit);
    }

    private void LogMalformedFragmentDrop(string reason)
    {
        int intervalMs = NormalizePositive(
            Options?.MalformedFragmentLogIntervalMs ?? DefaultMalformedFragmentLogIntervalMs,
            DefaultMalformedFragmentLogIntervalMs);

        if (!ShouldLogNow(_malformedFragmentLogTicks, reason, intervalMs))
            return;

        Log($"Dropped malformed fragment from server. reason={reason}");
    }

    private void LogQueueOverflow(
        string queueName,
        QueueOverflowPolicy policy,
        string action,
        long droppedCount,
        int highWaterMark,
        int limit)
    {
        int intervalMs = NormalizePositive(
            Options?.QueueOverflowLogIntervalMs ?? DefaultQueueOverflowLogIntervalMs,
            DefaultQueueOverflowLogIntervalMs);

        string throttleKey = $"{queueName}:{policy}:{action}";
        if (!ShouldLogNow(_queueOverflowLogTicks, throttleKey, intervalMs))
            return;

        Log($"Queue overflow: queue={queueName}, policy={policy}, action={action}, dropped={droppedCount}, highWater={highWaterMark}, limit={limit}");
    }

    private ushort GetMaxFragmentsPerPacket()
    {
        return NormalizePositive(
            Options?.MaxFragmentsPerPacket ?? (ushort)1024,
            (ushort)1024);
    }

    private int GetCommandQueueLimit()
    {
        return NormalizePositive(
            Options?.MaxCommandQueueDepth ?? DefaultMaxCommandQueueDepth,
            DefaultMaxCommandQueueDepth);
    }

    private int GetIncomingQueueLimit()
    {
        return NormalizePositive(
            Options?.MaxIncomingQueueDepth ?? DefaultMaxIncomingQueueDepth,
            DefaultMaxIncomingQueueDepth);
    }

    private int GetOutgoingQueueLimit()
    {
        return NormalizePositive(
            Options?.MaxOutgoingQueueDepth ?? DefaultMaxOutgoingQueueDepth,
            DefaultMaxOutgoingQueueDepth);
    }

    private QueueOverflowPolicy GetCommandQueueOverflowPolicy()
    {
        return Options?.CommandQueueOverflowPolicy ?? QueueOverflowPolicy.DropNewest;
    }

    private QueueOverflowPolicy GetIncomingQueueOverflowPolicy()
    {
        return Options?.IncomingQueueOverflowPolicy ?? QueueOverflowPolicy.DropOldest;
    }

    private QueueOverflowPolicy GetOutgoingQueueOverflowPolicy()
    {
        return Options?.OutgoingQueueOverflowPolicy ?? QueueOverflowPolicy.DropOldest;
    }

    private static int NormalizePositive(int configured, int fallback)
    {
        return configured > 0 ? configured : fallback;
    }

    private static ushort NormalizePositive(ushort configured, ushort fallback)
    {
        return configured > 0 ? configured : fallback;
    }

    private static bool TryReserveQueueSlot(ref int queueDepth, int maxDepth, out int depthAfterReserve)
    {
        while (true)
        {
            int observed = Volatile.Read(ref queueDepth);
            if (observed >= maxDepth)
            {
                depthAfterReserve = observed;
                return false;
            }

            int next = observed + 1;
            if (Interlocked.CompareExchange(ref queueDepth, next, observed) == observed)
            {
                depthAfterReserve = next;
                return true;
            }
        }
    }

    private static void UpdateHighWaterMark(ref int highWaterMark, int currentDepth)
    {
        while (true)
        {
            int observed = Volatile.Read(ref highWaterMark);
            if (currentDepth <= observed)
                return;

            if (Interlocked.CompareExchange(ref highWaterMark, currentDepth, observed) == observed)
                return;
        }
    }

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

    /// <summary>
    /// Builds an ENet <see cref="Address"/> from an IP string and port number.
    /// </summary>
    private static Address CreateAddress(string ip, ushort port)
    {
        Address address = new() { Port = port };
        address.SetHost(ip);
        return address;
    }

    /// <summary>
    /// Invokes an action, catching and logging any exceptions thrown by the worker-thread hook.
    /// </summary>
    private void TryInvoke(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            LoggerService.LogErr(exception, LogTag);
        }
    }
}
