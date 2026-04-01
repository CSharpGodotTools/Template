using ENet;
using GodotUtils;
using System;
using System.Threading;

namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Shared ENet server worker implementation that manages peers, queues, and packet dispatch.
/// </summary>
public abstract class ENetServer : ENetLow
{
    private const string LogTag = "Server";

    private readonly ServerPeerStore _peers;
    private readonly ServerQueueManager _queues;
    private readonly ServerIncomingProcessor _incoming;
    private readonly ServerOutgoingProcessor _outgoing;
    private readonly ServerCommandProcessor _commands;
    private readonly ServerLogAggregator _logAggregator;
    private ushort _streamCounter;

    /// <summary>
    /// Creates a server with default queue, processor, and peer-store components.
    /// </summary>
    protected ENetServer() : this(null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a server with optional custom queue/processor components.
    /// </summary>
    /// <param name="peerStore">Optional peer-store override.</param>
    /// <param name="queueManager">Optional queue-manager override.</param>
    /// <param name="incomingProcessor">Optional incoming-processor override.</param>
    /// <param name="outgoingProcessor">Optional outgoing-processor override.</param>
    /// <param name="commandProcessor">Optional command-processor override.</param>
    /// <param name="logAggregator">Optional lifecycle log-aggregator override.</param>
    private protected ENetServer(
        ServerPeerStore? peerStore,
        ServerQueueManager? queueManager,
        ServerIncomingProcessor? incomingProcessor,
        ServerOutgoingProcessor? outgoingProcessor,
        ServerCommandProcessor? commandProcessor,
        ServerLogAggregator? logAggregator)
    {
        _peers = peerStore ?? new ServerPeerStore();
        _queues = queueManager ?? new ServerQueueManager(() => Options, message => Log(message), DisconnectPeerById);
        _incoming = incomingProcessor ?? new ServerIncomingProcessor(_queues, _peers, () => Options, () => IgnoredPackets, message => Log(message), exception => LoggerService.LogErr(exception, LogTag));
        _outgoing = outgoingProcessor ?? new ServerOutgoingProcessor(_queues, _peers, () => Host, NextStreamId, GetMaxFragmentsPerPacket, data => CreateReliablePacket(data), exception => LogOutgoingSendFailure(exception, LogTag));
        _commands = commandProcessor ?? new ServerCommandProcessor(_queues, _peers, () => CTS.IsCancellationRequested, () => CTS.Cancel(), message => Log(message), DisconnectPeer, DisconnectAllPeers);
        _logAggregator = logAggregator ?? new ServerLogAggregator();
    }

    /// <summary>
    /// Gets current connected peer count.
    /// </summary>
    public int ConnectedPeerCount => _peers.ConnectedPeerCount;

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
    /// Gets total dropped outgoing message count.
    /// </summary>
    public long OutgoingQueueDroppedCount => _queues.OutgoingDroppedCount;

    /// <summary>
    /// Registers a typed client-packet handler.
    /// </summary>
    /// <typeparam name="TPacket">Client packet type to handle.</typeparam>
    /// <param name="handler">Handler callback.</param>
    protected void OnPacket<TPacket>(Action<PacketFromPeer<TPacket>> handler) where TPacket : ClientPacket => _incoming.RegisterHandler(handler);

    /// <inheritdoc />
    public sealed override void Log(object message, BBColor color = BBColor.Gray) => LoggerService.Log($"{BuildTimestampPrefix()}[Server] {message}", color);

    /// <summary>
    /// Queues a kick-all command with the provided disconnect opcode.
    /// </summary>
    /// <param name="opcode">Disconnect reason sent to peers.</param>
    public void KickAll(DisconnectOpcode opcode) => _queues.EnqueueCommand(new Cmd<ENetServerOpcode>(ENetServerOpcode.KickAll, opcode));

    /// <summary>
    /// Enqueues an outgoing message envelope.
    /// </summary>
    /// <param name="message">Outgoing message.</param>
    private protected void EnqueueOutgoing(OutgoingMessage message) => _queues.EnqueueOutgoing(message);

    /// <summary>
    /// Requests graceful server stop through command processing.
    /// </summary>
    protected void RequestStop() => _queues.EnqueueCommand(new Cmd<ENetServerOpcode>(ENetServerOpcode.Stop));

    /// <summary>
    /// Requests disconnect of a specific peer.
    /// </summary>
    /// <param name="peerId">Target peer id.</param>
    /// <param name="opcode">Disconnect reason opcode.</param>
    protected void RequestKick(uint peerId, DisconnectOpcode opcode) => _queues.EnqueueCommand(new Cmd<ENetServerOpcode>(ENetServerOpcode.Kick, peerId, opcode));

    /// <summary>
    /// Alias for <see cref="KickAll"/>.
    /// </summary>
    /// <param name="opcode">Disconnect reason opcode.</param>
    protected void RequestKickAll(DisconnectOpcode opcode) => KickAll(opcode);

    /// <inheritdoc />
    protected sealed override void ConcurrentQueues()
    {
        _commands.Process();
        _incoming.Process();
        _outgoing.Process();
        _logAggregator.Flush(message => Log(message));
    }

    /// <inheritdoc />
    protected sealed override void OnConnectLow(Event netEvent)
    {
        _peers.AddPeer(netEvent.Peer);
        _logAggregator.RecordConnect(netEvent.Peer.ID);
    }

    /// <summary>
    /// Called after a peer has been removed from server state.
    /// </summary>
    /// <param name="peerId">Disconnected peer id.</param>
    protected virtual void OnPeerDisconnected(uint peerId) { }

    /// <inheritdoc />
    protected sealed override void OnDisconnectLow(Event netEvent) => HandlePeerDisconnected(netEvent, _logAggregator.RecordDisconnect);

    /// <inheritdoc />
    protected sealed override void OnTimeoutLow(Event netEvent) => HandlePeerDisconnected(netEvent, _logAggregator.RecordTimeout);

    /// <inheritdoc />
    protected sealed override void OnReceiveLow(Event netEvent)
    {
        Packet packet = netEvent.Packet;

        // Drop oversized packets before they enter server processing queues.
        if (packet.Length > GamePacket.MaxSize)
        {
            Log($"Tried to read packet from client of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
            packet.Dispose();
            return;
        }

        _queues.EnqueueIncomingPacket(packet, netEvent.Peer);
    }

    /// <summary>
    /// Runs the server worker thread lifecycle for host setup, event loop, and teardown.
    /// </summary>
    /// <param name="port">Listening port.</param>
    /// <param name="maxClients">Maximum number of clients.</param>
    protected void WorkerThread(ushort port, int maxClients)
    {
        Host? host = TryCreateServerHost(port, maxClients);

        // Abort startup when host creation or port bind fails.
        if (host == null) return;

        Host = host;
        Interlocked.Exchange(ref _running, 1);
        Log("Server is running");

        try { WorkerLoop(); }
        finally
        {
            _logAggregator.Flush(message => Log(message));
            Host.Dispose();
            Interlocked.Exchange(ref _running, 0);
            Log("Server has stopped");
        }
    }

    /// <inheritdoc />
    protected sealed override void OnDisconnectCleanup(Peer peer)
    {
        base.OnDisconnectCleanup(peer);
        RemovePeerState(peer.ID);
    }

    /// <summary>
    /// Builds an optional timestamp prefix based on runtime options.
    /// </summary>
    /// <returns>Prefix string prepended to log messages.</returns>
    private string BuildTimestampPrefix() => Options != null && Options.ShowLogTimestamps ? $"[{DateTime.Now:HH:mm:ss}] " : string.Empty;

    /// <summary>
    /// Attempts to create and bind an ENet host for server listening.
    /// </summary>
    /// <param name="port">Listening port.</param>
    /// <param name="maxClients">Maximum accepted clients.</param>
    /// <returns>Created host, or null when creation fails.</returns>
    private Host? TryCreateServerHost(ushort port, int maxClients)
    {
        Host host = new();
        try
        {
            host.Create(new Address { Port = port }, maxClients);
            return host;
        }
        catch (InvalidOperationException exception)
        {
            Log($"A server is running on port {port} already! {exception.Message}");
            host.Dispose();
            return null;
        }
    }

    /// <summary>
    /// Handles disconnect/timeout event teardown and lifecycle callbacks.
    /// </summary>
    /// <param name="netEvent">ENet event payload.</param>
    /// <param name="recordLog">Lifecycle log recorder callback.</param>
    private void HandlePeerDisconnected(Event netEvent, Action<uint> recordLog)
    {
        uint peerId = netEvent.Peer.ID;

        // Invoke disconnect callbacks only when tracked peer state was removed.
        if (RemovePeerState(peerId)) TryInvokePeerDisconnected(peerId);
        recordLog(peerId);
    }

    /// <summary>
    /// Disconnects a peer by id when currently tracked.
    /// </summary>
    /// <param name="peerId">Target peer id.</param>
    /// <param name="opcode">Disconnect reason opcode.</param>
    private void DisconnectPeerById(uint peerId, DisconnectOpcode opcode)
    {
        // Disconnect only peers that are still present in the store.
        if (_peers.TryGetPeer(peerId, out Peer peer)) DisconnectPeer(peerId, peer, opcode);
    }

    /// <summary>
    /// Disconnects all currently connected peers.
    /// </summary>
    /// <param name="opcode">Disconnect reason opcode.</param>
    private void DisconnectAllPeers(DisconnectOpcode opcode)
    {
        foreach (Peer peer in _peers.SnapshotPeers()) DisconnectPeer(peer.ID, peer, opcode);
        _peers.ClearAll();
    }

    /// <summary>
    /// Disconnects one peer and removes associated server state.
    /// </summary>
    /// <param name="peerId">Target peer id.</param>
    /// <param name="peer">Peer handle.</param>
    /// <param name="opcode">Disconnect reason opcode.</param>
    private void DisconnectPeer(uint peerId, Peer peer, DisconnectOpcode opcode)
    {
        peer.DisconnectNow((uint)opcode);

        // Emit disconnect callbacks only when peer state existed and was removed.
        if (RemovePeerState(peerId)) TryInvokePeerDisconnected(peerId);
    }

    /// <summary>
    /// Removes tracked state for a peer id.
    /// </summary>
    /// <param name="peerId">Peer identifier.</param>
    /// <returns><see langword="true"/> when state existed and was removed.</returns>
    private bool RemovePeerState(uint peerId) => _peers.RemovePeerState(peerId);

    /// <summary>
    /// Invokes peer-disconnected callback and logs non-fatal callback exceptions.
    /// </summary>
    /// <param name="peerId">Disconnected peer id.</param>
    private void TryInvokePeerDisconnected(uint peerId)
    {
        try { OnPeerDisconnected(peerId); }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            LoggerService.LogErr(exception, LogTag);
        }
    }

    /// <summary>
    /// Gets next stream id for outgoing fragmented payloads.
    /// </summary>
    /// <returns>Next stream identifier.</returns>
    private ushort NextStreamId() => _streamCounter++;

    /// <summary>
    /// Resolves max fragments per packet from options with fallback normalization.
    /// </summary>
    /// <returns>Validated max fragment count.</returns>
    private ushort GetMaxFragmentsPerPacket()
    {
        ushort configured = Options?.MaxFragmentsPerPacket ?? (ushort)1024;
        return configured > 0 ? configured : (ushort)1024;
    }
}
