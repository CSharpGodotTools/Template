using ENet;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace __TEMPLATE__.Netcode.Client;

public abstract class ENetClient : ENetLow
{
    private const string LogTag = "Client";
    private static readonly ClientLogAggregator SharedLogAggregator = new();
    private static int _activeClientWorkers;

    private readonly ClientQueueManager _queues;
    private readonly ClientCommandProcessor _commands;
    private readonly ClientIncomingProcessor _incoming;
    private readonly ClientOutgoingProcessor _outgoing;
    private readonly ClientLogAggregator _logAggregator;
    private readonly ConcurrentQueue<Cmd<GodotOpcode>> _mainThreadCommands = new();
    private readonly ConcurrentQueue<PacketData> _mainThreadPackets = new();

    private Peer _peer;
    private ushort _streamCounter;
    protected long _connected;

    protected ENetClient() : this(null, null, null, null, null) { }

    private protected ENetClient(
        ClientQueueManager? queueManager,
        ClientCommandProcessor? commandProcessor,
        ClientIncomingProcessor? incomingProcessor,
        ClientOutgoingProcessor? outgoingProcessor,
        ClientLogAggregator? logAggregator)
    {
        _queues = queueManager ?? new ClientQueueManager(() => Options, message => Log(message), DisconnectPeer);
        _commands = commandProcessor ?? new ClientCommandProcessor(_queues, () => CTS.IsCancellationRequested, message => Log(message), DisconnectPeer);
        _incoming = incomingProcessor ?? new ClientIncomingProcessor(_queues, _mainThreadPackets, () => Options, message => Log(message));
        _outgoing = outgoingProcessor ?? new ClientOutgoingProcessor(_queues, () => _peer, NextStreamId, GetMaxFragmentsPerPacket, data => CreateReliablePacket(data), exception => LogOutgoingSendFailure(exception, LogTag));
        _logAggregator = logAggregator ?? SharedLogAggregator;
    }

    protected ConcurrentQueue<Cmd<GodotOpcode>> MainThreadCommands => _mainThreadCommands;
    protected ConcurrentQueue<PacketData> MainThreadPackets => _mainThreadPackets;

    protected virtual uint PingIntervalMs { get; } = 1000;
    protected virtual uint PeerTimeoutMs { get; } = 5000;
    protected virtual uint PeerTimeoutMinimumMs { get; } = 5000;
    protected virtual uint PeerTimeoutMaximumMs { get; } = 5000;

    public uint PeerId => _peer.ID;
    public int CommandQueueHighWaterMark => _queues.CommandHighWaterMark;
    public int IncomingQueueHighWaterMark => _queues.IncomingHighWaterMark;
    public int OutgoingQueueHighWaterMark => _queues.OutgoingHighWaterMark;
    public long CommandQueueDroppedCount => _queues.CommandDroppedCount;
    public long IncomingQueueDroppedCount => _queues.IncomingDroppedCount;
    public long OutgoingQueueDroppedCount => _queues.OutgoingDroppedCount;

    public sealed override void Log(object message, BBColor color = BBColor.Gray) => LoggerService.Log($"{BuildTimestampPrefix()}[Client] {message}", color);

    protected sealed override void ConcurrentQueues()
    {
        _commands.Process();
        _incoming.Process();
        _outgoing.Process();
        _logAggregator.Flush(message => Log(message), false);
    }

    protected virtual void OnConnected() { }
    protected virtual void OnDisconnected() { }
    protected virtual void OnTimedOut() { }

    protected sealed override void OnConnectLow(Event netEvent)
    {
        Interlocked.Exchange(ref _connected, 1);
        _mainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Connected));
        _logAggregator.RecordConnect(netEvent.Peer.ID);
        TryInvoke(OnConnected);
    }

    protected sealed override void OnDisconnectLow(Event netEvent)
    {
        QueueDisconnected((DisconnectOpcode)netEvent.Data);
        OnDisconnectCleanup(netEvent.Peer);
        _logAggregator.RecordDisconnect(netEvent.Peer.ID);
        TryInvoke(OnDisconnected);
    }

    protected sealed override void OnTimeoutLow(Event netEvent)
    {
        QueueDisconnected(DisconnectOpcode.Timeout);
        _mainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Timeout));
        OnDisconnectCleanup(netEvent.Peer);
        _logAggregator.RecordTimeout(netEvent.Peer.ID);
        TryInvoke(OnTimedOut);
    }

    protected sealed override void OnReceiveLow(Event netEvent)
    {
        Packet packet = netEvent.Packet;
        if (packet.Length > GamePacket.MaxSize)
        {
            Log($"Tried to read packet from server of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
            packet.Dispose();
            return;
        }

        _queues.EnqueueIncoming(packet);
    }

    protected sealed override void OnDisconnectCleanup(Peer peer)
    {
        base.OnDisconnectCleanup(peer);
        Interlocked.Exchange(ref _connected, 0);
        _incoming.ClearReassembly();
    }

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
                _logAggregator.Flush(message => Log(message), true);
        }
    }

    protected void EnqueueOutgoing(byte[] data) => _queues.EnqueueOutgoing(data);
    protected void RequestDisconnect() => _queues.EnqueueCommand(new Cmd<ENetClientOpcode>(ENetClientOpcode.Disconnect));

    private string BuildTimestampPrefix() => Options != null && Options.ShowLogTimestamps ? $"[{DateTime.Now:HH:mm:ss}] " : string.Empty;
    private void QueueDisconnected(DisconnectOpcode opcode) => _mainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Disconnected, opcode));
    private void DisconnectPeer(DisconnectOpcode opcode) => _peer.Disconnect((uint)opcode);
    private ushort NextStreamId() => _streamCounter++;

    private ushort GetMaxFragmentsPerPacket()
    {
        ushort configured = Options?.MaxFragmentsPerPacket ?? (ushort)1024;
        return configured > 0 ? configured : (ushort)1024;
    }

    private static Address CreateAddress(string ip, ushort port)
    {
        Address address = new() { Port = port };
        address.SetHost(ip);
        return address;
    }

    private void TryInvoke(Action action)
    {
        try { action(); }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            LoggerService.LogErr(exception, LogTag);
        }
    }
}
