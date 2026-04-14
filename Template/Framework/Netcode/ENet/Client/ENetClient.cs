using ENet;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace __TEMPLATE__.Netcode.Client;

/// <summary>
/// Shared ENet client worker implementation that bridges transport events to main-thread queues.
/// </summary>
public abstract class ENetClient : ENetLow
{
    private const string LogTag = "Client";
    private static readonly ClientLogAggregator _sharedLogAggregator = new();
    private static int _activeClientWorkers;

    private readonly ClientQueueManager _queues;
    private readonly ClientCommandProcessor _commands;
    private readonly ClientIncomingProcessor _incoming;
    private readonly ClientOutgoingProcessor _outgoing;
    private readonly ClientLogAggregator _logAggregator;

    private Peer _peer;
    private ushort _streamCounter;
    protected long _connected;

    /// <summary>
    /// Creates a client with default queue and processor components.
    /// </summary>
    protected ENetClient() : this(null, null, null, null, null) { }

    /// <summary>
    /// Creates a client with optional custom queue/processor components.
    /// </summary>
    /// <param name="queueManager">Optional queue manager override.</param>
    /// <param name="commandProcessor">Optional command processor override.</param>
    /// <param name="incomingProcessor">Optional incoming processor override.</param>
    /// <param name="outgoingProcessor">Optional outgoing processor override.</param>
    /// <param name="logAggregator">Optional lifecycle log aggregator override.</param>
    private protected ENetClient(
        ClientQueueManager? queueManager,
        ClientCommandProcessor? commandProcessor,
        ClientIncomingProcessor? incomingProcessor,
        ClientOutgoingProcessor? outgoingProcessor,
        ClientLogAggregator? logAggregator)
    {
        _queues = queueManager ?? new ClientQueueManager(() => Options, message => Log(message), DisconnectPeer);
        _commands = commandProcessor ?? new ClientCommandProcessor(_queues, () => CTS.IsCancellationRequested, message => Log(message), DisconnectPeer);
        _incoming = incomingProcessor ?? new ClientIncomingProcessor(_queues, MainThreadPackets, () => Options, message => Log(message));
        _outgoing = outgoingProcessor ?? new ClientOutgoingProcessor(_queues, () => _peer, NextStreamId, GetMaxFragmentsPerPacket, data => CreateReliablePacket(data), exception => LogOutgoingSendFailure(exception, LogTag));
        _logAggregator = logAggregator ?? _sharedLogAggregator;
    }

    /// <summary>
    /// Gets the queue of transport lifecycle commands to be consumed on the main thread.
    /// </summary>
    protected ConcurrentQueue<Cmd<GodotOpcode>> MainThreadCommands { get; } = new();

    /// <summary>
    /// Gets the queue of decoded server packets to be consumed on the main thread.
    /// </summary>
    protected ConcurrentQueue<PacketData> MainThreadPackets { get; } = new();

    /// <summary>
    /// Gets configurable ENet ping interval in milliseconds.
    /// </summary>
    protected virtual uint PingIntervalMs { get; } = 1000;

    /// <summary>
    /// Gets configurable ENet peer timeout in milliseconds.
    /// </summary>
    protected virtual uint PeerTimeoutMs { get; } = 5000;

    /// <summary>
    /// Gets configurable ENet minimum timeout in milliseconds.
    /// </summary>
    protected virtual uint PeerTimeoutMinimumMs { get; } = 5000;

    /// <summary>
    /// Gets configurable ENet maximum timeout in milliseconds.
    /// </summary>
    protected virtual uint PeerTimeoutMaximumMs { get; } = 5000;

    /// <summary>
    /// Gets the current ENet peer identifier.
    /// </summary>
    public uint PeerId => _peer.ID;

    /// <summary>
    /// Gets command queue high-water mark.
    /// </summary>
    public int CommandQueueHighWaterMark => _queues.CommandHighWaterMark;

    /// <summary>
    /// Gets incoming queue high-water mark.
    /// </summary>
    public int IncomingQueueHighWaterMark => _queues.IncomingHighWaterMark;

    /// <summary>
    /// Gets outgoing queue high-water mark.
    /// </summary>
    public int OutgoingQueueHighWaterMark => _queues.OutgoingHighWaterMark;

    /// <summary>
    /// Gets total dropped command count.
    /// </summary>
    public long CommandQueueDroppedCount => _queues.CommandDroppedCount;

    /// <summary>
    /// Gets total dropped incoming packet count.
    /// </summary>
    public long IncomingQueueDroppedCount => _queues.IncomingDroppedCount;

    /// <summary>
    /// Gets total dropped outgoing payload count.
    /// </summary>
    public long OutgoingQueueDroppedCount => _queues.OutgoingDroppedCount;

    /// <inheritdoc />
    public sealed override void Log(object message, BBColor color = BBColor.Gray) => LoggerService.Log($"{BuildTimestampPrefix()}[Client] {message}", color);

    /// <inheritdoc />
    protected sealed override void ConcurrentQueues()
    {
        _commands.Process();
        _incoming.Process();
        _outgoing.Process();
        _logAggregator.Flush(message => Log(message), false);
    }

    /// <summary>
    /// Called after a successful connect event.
    /// </summary>
    protected virtual void OnConnected() { }

    /// <summary>
    /// Called after a disconnect event.
    /// </summary>
    protected virtual void OnDisconnected() { }

    /// <summary>
    /// Called after a timeout event.
    /// </summary>
    protected virtual void OnTimedOut() { }

    /// <inheritdoc />
    protected sealed override void OnConnectLow(Event netEvent)
    {
        Interlocked.Exchange(ref _connected, 1);
        MainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Connected));
        _logAggregator.RecordConnect(netEvent.Peer.ID);
        TryInvoke(OnConnected);
    }

    /// <inheritdoc />
    protected sealed override void OnDisconnectLow(Event netEvent)
    {
        QueueDisconnected((DisconnectOpcode)netEvent.Data);
        OnDisconnectCleanup(netEvent.Peer);
        _logAggregator.RecordDisconnect(netEvent.Peer.ID);
        TryInvoke(OnDisconnected);
    }

    /// <inheritdoc />
    protected sealed override void OnTimeoutLow(Event netEvent)
    {
        QueueDisconnected(DisconnectOpcode.Timeout);
        MainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Timeout));
        OnDisconnectCleanup(netEvent.Peer);
        _logAggregator.RecordTimeout(netEvent.Peer.ID);
        TryInvoke(OnTimedOut);
    }

    /// <inheritdoc />
    protected sealed override void OnReceiveLow(Event netEvent)
    {
        Packet packet = netEvent.Packet;

        // Drop oversized packets before they enter client processing queues.
        if (packet.Length > GamePacket.MaxSize)
        {
            Log($"Tried to read packet from server of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
            packet.Dispose();
            return;
        }

        _queues.EnqueueIncoming(packet);
    }

    /// <inheritdoc />
    protected sealed override void OnDisconnectCleanup(Peer peer)
    {
        base.OnDisconnectCleanup(peer);
        Interlocked.Exchange(ref _connected, 0);
        _incoming.ClearReassembly();
    }

    /// <summary>
    /// Runs the client worker thread lifecycle for connect, process loop, and teardown.
    /// </summary>
    /// <param name="ip">Remote host/IP.</param>
    /// <param name="port">Remote port.</param>
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

            // Flush logs only when the final active client worker exits.
            if (Interlocked.Decrement(ref _activeClientWorkers) == 0)
            {
                // Flush once when the last active worker exits so bursts are not lost.
                _logAggregator.Flush(message => Log(message), true);
            }
        }
    }

    /// <summary>
    /// Enqueues an outgoing payload for worker-thread send processing.
    /// </summary>
    /// <param name="data">Serialized packet bytes.</param>
    protected void EnqueueOutgoing(byte[] data) => _queues.EnqueueOutgoing(data);

    /// <summary>
    /// Requests graceful disconnect through command queue processing.
    /// </summary>
    protected void RequestDisconnect() => _queues.EnqueueCommand(new Cmd<ENetClientOpcode>(ENetClientOpcode.Disconnect));

    /// <summary>
    /// Builds an optional timestamp prefix based on runtime options.
    /// </summary>
    /// <returns>Prefix string prepended to log messages.</returns>
    private string BuildTimestampPrefix() => Options?.ShowLogTimestamps == true ? $"[{DateTime.Now:HH:mm:ss}] " : string.Empty;

    /// <summary>
    /// Queues a disconnect lifecycle command for main-thread handling.
    /// </summary>
    /// <param name="opcode">Disconnect reason opcode.</param>
    private void QueueDisconnected(DisconnectOpcode opcode) => MainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Disconnected, opcode));

    /// <summary>
    /// Sends a transport-level disconnect signal to the current peer.
    /// </summary>
    /// <param name="opcode">Disconnect reason opcode.</param>
    private void DisconnectPeer(DisconnectOpcode opcode) => _peer.Disconnect((uint)opcode);

    /// <summary>
    /// Gets the next fragment stream id for outgoing fragmented payloads.
    /// </summary>
    /// <returns>Next stream identifier.</returns>
    private ushort NextStreamId() => _streamCounter++;

    /// <summary>
    /// Resolves configured max fragments per packet with positive-value normalization.
    /// </summary>
    /// <returns>Validated max fragments per packet.</returns>
    private ushort GetMaxFragmentsPerPacket()
    {
        ushort configured = Options?.MaxFragmentsPerPacket ?? (ushort)1024;
        return configured > 0 ? configured : (ushort)1024;
    }

    /// <summary>
    /// Creates an ENet address from host and port.
    /// </summary>
    /// <param name="ip">Host/IP string.</param>
    /// <param name="port">Port number.</param>
    /// <returns>Configured ENet address.</returns>
    private static Address CreateAddress(string ip, ushort port)
    {
        Address address = new() { Port = port };
        address.SetHost(ip);
        return address;
    }

    /// <summary>
    /// Invokes a callback and logs non-fatal exceptions.
    /// </summary>
    /// <param name="action">Callback to invoke.</param>
    private void TryInvoke(Action action)
    {
        try { action(); }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            LoggerService.LogErr(exception, LogTag);
        }
    }
}
