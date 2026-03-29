using ENet;
using GodotUtils;
using System;
using System.Threading;

namespace __TEMPLATE__.Netcode.Server;

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

    protected ENetServer() : this(null, null, null, null, null, null) { }

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

    public int ConnectedPeerCount => _peers.ConnectedPeerCount;
    public int CommandQueueHighWaterMark => _queues.CommandHighWaterMark;
    public int IncomingQueueHighWaterMark => _queues.IncomingHighWaterMark;
    public int OutgoingQueueHighWaterMark => _queues.OutgoingHighWaterMark;
    public long CommandQueueDroppedCount => _queues.CommandDroppedCount;
    public long IncomingQueueDroppedCount => _queues.IncomingDroppedCount;
    public long OutgoingQueueDroppedCount => _queues.OutgoingDroppedCount;

    protected void OnPacket<TPacket>(Action<PacketFromPeer<TPacket>> handler) where TPacket : ClientPacket => _incoming.RegisterHandler(handler);

    public sealed override void Log(object message, BBColor color = BBColor.Gray) => LoggerService.Log($"{BuildTimestampPrefix()}[Server] {message}", color);

    public void KickAll(DisconnectOpcode opcode) => _queues.EnqueueCommand(new Cmd<ENetServerOpcode>(ENetServerOpcode.KickAll, opcode));
    private protected void EnqueueOutgoing(OutgoingMessage message) => _queues.EnqueueOutgoing(message);
    protected void RequestStop() => _queues.EnqueueCommand(new Cmd<ENetServerOpcode>(ENetServerOpcode.Stop));
    protected void RequestKick(uint peerId, DisconnectOpcode opcode) => _queues.EnqueueCommand(new Cmd<ENetServerOpcode>(ENetServerOpcode.Kick, peerId, opcode));
    protected void RequestKickAll(DisconnectOpcode opcode) => KickAll(opcode);

    protected sealed override void ConcurrentQueues()
    {
        _commands.Process();
        _incoming.Process();
        _outgoing.Process();
        _logAggregator.Flush(message => Log(message));
    }

    protected sealed override void OnConnectLow(Event netEvent)
    {
        _peers.AddPeer(netEvent.Peer);
        _logAggregator.RecordConnect(netEvent.Peer.ID);
    }

    protected virtual void OnPeerDisconnected(uint peerId) { }
    protected sealed override void OnDisconnectLow(Event netEvent) => HandlePeerDisconnected(netEvent, _logAggregator.RecordDisconnect);
    protected sealed override void OnTimeoutLow(Event netEvent) => HandlePeerDisconnected(netEvent, _logAggregator.RecordTimeout);

    protected sealed override void OnReceiveLow(Event netEvent)
    {
        Packet packet = netEvent.Packet;
        if (packet.Length > GamePacket.MaxSize)
        {
            Log($"Tried to read packet from client of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
            packet.Dispose();
            return;
        }

        _queues.EnqueueIncomingPacket(packet, netEvent.Peer);
    }

    protected void WorkerThread(ushort port, int maxClients)
    {
        Host? host = TryCreateServerHost(port, maxClients);
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

    protected sealed override void OnDisconnectCleanup(Peer peer)
    {
        base.OnDisconnectCleanup(peer);
        RemovePeerState(peer.ID);
    }

    private string BuildTimestampPrefix() => Options != null && Options.ShowLogTimestamps ? $"[{DateTime.Now:HH:mm:ss}] " : string.Empty;

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

    private void HandlePeerDisconnected(Event netEvent, Action<uint> recordLog)
    {
        uint peerId = netEvent.Peer.ID;
        if (RemovePeerState(peerId)) TryInvokePeerDisconnected(peerId);
        recordLog(peerId);
    }

    private void DisconnectPeerById(uint peerId, DisconnectOpcode opcode)
    {
        if (_peers.TryGetPeer(peerId, out Peer peer)) DisconnectPeer(peerId, peer, opcode);
    }

    private void DisconnectAllPeers(DisconnectOpcode opcode)
    {
        foreach (Peer peer in _peers.SnapshotPeers()) DisconnectPeer(peer.ID, peer, opcode);
        _peers.ClearAll();
    }

    private void DisconnectPeer(uint peerId, Peer peer, DisconnectOpcode opcode)
    {
        peer.DisconnectNow((uint)opcode);
        if (RemovePeerState(peerId)) TryInvokePeerDisconnected(peerId);
    }

    private bool RemovePeerState(uint peerId) => _peers.RemovePeerState(peerId);

    private void TryInvokePeerDisconnected(uint peerId)
    {
        try { OnPeerDisconnected(peerId); }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            LoggerService.LogErr(exception, LogTag);
        }
    }

    private ushort NextStreamId() => _streamCounter++;
    private ushort GetMaxFragmentsPerPacket()
    {
        ushort configured = Options?.MaxFragmentsPerPacket ?? (ushort)1024;
        return configured > 0 ? configured : (ushort)1024;
    }
}
