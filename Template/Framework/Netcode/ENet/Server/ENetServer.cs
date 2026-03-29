using ENet;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace __TEMPLATE__.Netcode.Server;

// ENet API Reference: https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md
/// <summary>
/// Base ENet server worker that owns peer tracking, incoming packet dispatch, and outgoing message queues.
/// Extend <see cref="GodotServer"/> for game-level packet registration.
/// </summary>
public abstract partial class ENetServer : ENetLow
{
    private const string LogTag = "Server";
    private const int DefaultMaxCommandQueueDepth = 1024;
    private const int DefaultMaxIncomingQueueDepth = 4096;
    private const int DefaultMaxOutgoingQueueDepth = 4096;
    private const int DefaultQueueOverflowLogIntervalMs = 2000;
    private const int DefaultMalformedFragmentLogIntervalMs = 2000;

    private readonly ConcurrentQueue<Cmd<ENetServerOpcode>> _enetCmds = new();
    private readonly ConcurrentQueue<IncomingPacket> _incoming = new();
    private readonly ConcurrentQueue<OutgoingMessage> _outgoing = new();
    private readonly ConcurrentDictionary<Type, Action<PacketFromPeer<ClientPacket>>> _clientPacketHandlers = new();
    private readonly ConcurrentDictionary<string, long> _queueOverflowLogTicks = new();
    private readonly ConcurrentDictionary<string, long> _malformedFragmentLogTicks = new();

    /// <summary>
    /// Peer lookup. Only accessed on the ENet worker thread.
    /// </summary>
    private readonly Dictionary<uint, Peer> _peers = [];

    private readonly ServerLogAggregator _logAggregator = new();
    private ushort _streamCounter;
    private readonly Dictionary<uint, Dictionary<ushort, FragmentBuffer>> _reassemblyBuffers = [];
    private int _connectedPeerCount;
    private int _enetCmdDepth;
    private int _incomingDepth;
    private int _outgoingDepth;
    private int _enetCmdHighWaterMark;
    private int _incomingHighWaterMark;
    private int _outgoingHighWaterMark;
    private long _enetCmdDroppedCount;
    private long _incomingDroppedCount;
    private long _outgoingDroppedCount;

    /// <summary>
    /// Number of currently connected peers.
    /// </summary>
    public int ConnectedPeerCount => Interlocked.CompareExchange(ref _connectedPeerCount, 0, 0);
    public int CommandQueueHighWaterMark => Volatile.Read(ref _enetCmdHighWaterMark);
    public int IncomingQueueHighWaterMark => Volatile.Read(ref _incomingHighWaterMark);
    public int OutgoingQueueHighWaterMark => Volatile.Read(ref _outgoingHighWaterMark);
    public long CommandQueueDroppedCount => Interlocked.Read(ref _enetCmdDroppedCount);
    public long IncomingQueueDroppedCount => Interlocked.Read(ref _incomingDroppedCount);
    public long OutgoingQueueDroppedCount => Interlocked.Read(ref _outgoingDroppedCount);

    /// <summary>
    /// Registers a handler for incoming <typeparamref name="TPacket"/> packets, dispatched on the ENet worker thread.
    /// </summary>
    protected void OnPacket<TPacket>(Action<PacketFromPeer<TPacket>> handler) where TPacket : ClientPacket
    {
        ArgumentNullException.ThrowIfNull(handler);

        _clientPacketHandlers[typeof(TPacket)] = peer =>
            handler(new PacketFromPeer<TPacket> { Packet = (TPacket)peer.Packet, PeerId = peer.PeerId });
    }

    /// <summary>
    /// Logs a message as the server.
    /// </summary>
    public sealed override void Log(object message, BBColor color = BBColor.Gray)
    {
        string timestampPrefix = BuildTimestampPrefix();
        LoggerService.Log($"{timestampPrefix}[Server] {message}", color);
    }

    /// <summary>
    /// Kick everyone on the server with a specified opcode.
    /// </summary>
    public void KickAll(DisconnectOpcode opcode)
    {
        EnqueueCommand(new Cmd<ENetServerOpcode>(ENetServerOpcode.KickAll, opcode));
    }

    /// <summary>
    /// Enqueues a send/broadcast command for the worker thread.
    /// </summary>
    protected void EnqueueOutgoing(OutgoingMessage message)
    {
        EnqueueOutgoingMessage(message);
    }

    /// <summary>
    /// Requests a graceful server shutdown from any thread.
    /// </summary>
    protected void RequestStop()
    {
        EnqueueCommand(new Cmd<ENetServerOpcode>(ENetServerOpcode.Stop));
    }

    /// <summary>
    /// Requests a peer kick from any thread.
    /// </summary>
    protected void RequestKick(uint peerId, DisconnectOpcode opcode)
    {
        EnqueueCommand(new Cmd<ENetServerOpcode>(ENetServerOpcode.Kick, peerId, opcode));
    }

    /// <summary>
    /// Requests a kick-all from any thread.
    /// </summary>
    protected void RequestKickAll(DisconnectOpcode opcode) => KickAll(opcode);

    /// <summary>
    /// Processes server worker queues each network tick.
    /// </summary>
    protected sealed override void ConcurrentQueues()
    {
        ProcessENetCommands();
        ProcessIncomingPackets();
        ProcessOutgoingPackets();
        _logAggregator.Flush(message => Log(message));
    }

    /// <summary>
    /// Internal connect handler that tracks active peers.
    /// </summary>
    protected sealed override void OnConnectLow(Event netEvent)
    {
        _peers[netEvent.Peer.ID] = netEvent.Peer;
        Interlocked.Increment(ref _connectedPeerCount);
        _logAggregator.RecordConnect(netEvent.Peer.ID);
    }

    /// <summary>
    /// Called on the worker thread when a peer disconnects or times out.
    /// </summary>
    protected virtual void OnPeerDisconnected(uint peerId)
    {
    }

    /// <summary>
    /// Internal disconnect handler that removes peer state.
    /// </summary>
    protected sealed override void OnDisconnectLow(Event netEvent)
    {
        HandlePeerDisconnected(netEvent, _logAggregator.RecordDisconnect);
    }

    /// <summary>
    /// Internal timeout handler that removes peer state.
    /// </summary>
    protected sealed override void OnTimeoutLow(Event netEvent)
    {
        HandlePeerDisconnected(netEvent, _logAggregator.RecordTimeout);
    }

    /// <summary>
    /// Internal receive handler that validates packet size and enqueues payloads.
    /// </summary>
    protected sealed override void OnReceiveLow(Event netEvent)
    {
        Packet packet = netEvent.Packet;

        if (packet.Length > GamePacket.MaxSize)
        {
            Log($"Tried to read packet from client of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
            packet.Dispose();
            return;
        }

        EnqueueIncomingPacket(packet, netEvent.Peer);
    }

    /// <summary>
    /// Runs the ENet server worker loop for the configured listen port.
    /// </summary>
    protected void WorkerThread(ushort port, int maxClients)
    {
        Host? host = TryCreateServerHost(port, maxClients);

        if (host == null)
            return;

        Host = host;
        Interlocked.Exchange(ref _running, 1);
        Log("Server is running");

        try
        {
            WorkerLoop();
        }
        finally
        {
            _logAggregator.Flush(message => Log(message));
            Host.Dispose();
            Interlocked.Exchange(ref _running, 0);
            Log("Server has stopped");
        }
    }

    /// <summary>
    /// Clears server peer state and executes shared disconnect cleanup.
    /// </summary>
    protected sealed override void OnDisconnectCleanup(Peer peer)
    {
        base.OnDisconnectCleanup(peer);
        RemovePeerState(peer.ID);
    }

    /// <summary>
    /// Returns a formatted timestamp prefix when timestamp logging is enabled.
    /// </summary>
    private string BuildTimestampPrefix()
    {
        if (Options == null || !Options.ShowLogTimestamps)
            return string.Empty;

        return $"[{DateTime.Now:HH:mm:ss}] ";
    }

    /// <summary>
    /// Creates and binds an ENet host on the specified port. Returns <c>null</c> if the port is already in use.
    /// </summary>
    private Host? TryCreateServerHost(ushort port, int maxClients)
    {
        Host host = new();

        try
        {
            host.Create(new Address { Port = port }, maxClients);
        }
        catch (InvalidOperationException exception)
        {
            Log($"A server is running on port {port} already! {exception.Message}");
            host.Dispose();
            return null;
        }

        return host;
    }

    /// <summary>
    /// Drains the ENet command queue and executes pending stop, kick, and kick-all commands.
    /// </summary>
    private void ProcessENetCommands()
    {
        while (_enetCmds.TryDequeue(out Cmd<ENetServerOpcode>? command))
        {
            Interlocked.Decrement(ref _enetCmdDepth);

            switch (command.Opcode)
            {
                case ENetServerOpcode.Stop:
                    HandleStopCommand();
                    break;

                case ENetServerOpcode.Kick:
                    HandleKickCommand(command);
                    break;

                case ENetServerOpcode.KickAll:
                    HandleKickAllCommand(command);
                    break;
            }
        }
    }

    /// <summary>
    /// Disconnects all peers and cancels the worker token to begin shutdown.
    /// </summary>
    private void HandleStopCommand()
    {
        if (CTS.IsCancellationRequested)
        {
            Log("Server is in the middle of stopping");
            return;
        }

        DisconnectAllPeers(DisconnectOpcode.Stopping);
        CTS.Cancel();
    }

    /// <summary>
    /// Disconnects a single peer immediately using the specified opcode.
    /// </summary>
    private void HandleKickCommand(Cmd<ENetServerOpcode> command)
    {
        uint peerId = (uint)command.Data[0];
        DisconnectOpcode opcode = (DisconnectOpcode)command.Data[1];

        if (!_peers.TryGetValue(peerId, out Peer peer))
        {
            Log($"Tried to kick peer with id '{peerId}' but this peer does not exist");
            return;
        }

        DisconnectPeer(peerId, peer, opcode);
    }

    /// <summary>
    /// Disconnects all peers using the opcode carried in the command.
    /// </summary>
    private void HandleKickAllCommand(Cmd<ENetServerOpcode> command)
    {
        DisconnectOpcode opcode = (DisconnectOpcode)command.Data[0];
        DisconnectAllPeers(opcode);
    }

    /// <summary>
    /// Issues <c>DisconnectNow</c> to every tracked peer and clears all peer state.
    /// </summary>
    private void DisconnectAllPeers(DisconnectOpcode opcode)
    {
        List<Peer> peers = [.. _peers.Values];
        foreach (Peer peer in peers)
        {
            DisconnectPeer(peer.ID, peer, opcode);
        }

        _peers.Clear();
        _reassemblyBuffers.Clear();
    }

    /// <summary>
    /// Drains the incoming packet queue, reassembling fragments and dispatching complete payloads.
    /// </summary>
    private void ProcessIncomingPackets()
    {
        while (_incoming.TryDequeue(out IncomingPacket queued))
        {
            Interlocked.Decrement(ref _incomingDepth);

            // Copy bytes eagerly so we can inspect the opcode before any higher-level parsing.
            byte[] bytes = new byte[queued.Packet.Length];
            queued.Packet.CopyTo(bytes);
            queued.Packet.Dispose();

            if (PacketFragmenter.IsFragment(bytes))
            {
                HandleFragmentBytes(bytes, queued.Peer);
            }
            else
            {
                DispatchIncomingBytes(bytes, queued.Peer);
            }
        }
    }

    /// <summary>
    /// Accumulates a fragment into the per-peer reassembly buffer and dispatches the full payload when complete.
    /// </summary>
    private void HandleFragmentBytes(byte[] fragmentBytes, Peer peer)
    {
        if (!PacketFragmenter.TryReadHeader(fragmentBytes, out ushort streamId, out ushort fragIndex, out ushort totalFragments))
        {
            LogMalformedFragmentDrop(peer.ID, "Fragment header was truncated.");
            return;
        }

        if (!PacketFragmenter.IsValidHeader(fragIndex, totalFragments, GetMaxFragmentsPerPacket(), out string validationError))
        {
            LogMalformedFragmentDrop(peer.ID, $"stream={streamId}: {validationError}");
            return;
        }

        if (!_reassemblyBuffers.TryGetValue(peer.ID, out Dictionary<ushort, FragmentBuffer>? peerBuffers))
        {
            peerBuffers = [];
            _reassemblyBuffers[peer.ID] = peerBuffers;
        }

        if (!peerBuffers.TryGetValue(streamId, out FragmentBuffer? buffer))
        {
            buffer = new FragmentBuffer(totalFragments);
            peerBuffers[streamId] = buffer;
        }
        else if (buffer.TotalFragments != totalFragments)
        {
            peerBuffers.Remove(streamId);
            LogMalformedFragmentDrop(peer.ID, $"stream={streamId}: fragment count changed from {buffer.TotalFragments} to {totalFragments}.");
            return;
        }

        byte[] payload = PacketFragmenter.ExtractPayload(fragmentBytes);
        if (!buffer.Add(fragIndex, payload))
            return;

        peerBuffers.Remove(streamId);
        DispatchIncomingBytes(buffer.Assemble(), peer);
    }

    /// <summary>
    /// Decodes a wire opcode, looks up a registered handler, and invokes it for a complete incoming payload.
    /// </summary>
    private void DispatchIncomingBytes(byte[] bytes, Peer peer)
    {
        PacketReader reader = new(bytes);

        try
        {
            if (!TryGetPacketAndType(reader, out ClientPacket? packet, out Type? packetType))
            {
                return;
            }

            if (!TryReadPacket(packet!, reader, out string? errorMessage))
            {
                Log($"Received malformed packet: {errorMessage} (Ignoring)");
                return;
            }

            if (!_clientPacketHandlers.TryGetValue(packetType!, out Action<PacketFromPeer<ClientPacket>>? handler))
            {
                Log($"No handler registered for client packet {packetType!.Name} (Ignoring)");
                return;
            }

            if (!TryInvokePacketHandler(handler, new PacketFromPeer<ClientPacket> { Packet = packet!, PeerId = peer.ID }))
            {
                return;
            }

            LogPacketReceived(packetType!, peer.ID, packet!);
        }
        finally
        {
            reader.Dispose();
        }
    }

    /// <summary>
    /// Reads the wire opcode and resolves the matching <see cref="ClientPacket"/> type from the registry.
    /// </summary>
    private bool TryGetPacketAndType(PacketReader packetReader, out ClientPacket? clientPacket, out Type? packetType)
    {
        ushort opcode;
        try
        {
            opcode = PacketRegistry.ReadOpcodeFromReader(packetReader);
        }
        catch (EndOfStreamException exception)
        {
            Log($"Received malformed packet: {exception.Message} (Ignoring)");
            clientPacket = null;
            packetType = null;
            return false;
        }

        if (!PacketRegistry.ClientPacketTypesWire.TryGetValue(opcode, out packetType))
        {
            Log($"Received malformed opcode: {opcode} (Ignoring)");
            clientPacket = null;
            return false;
        }

        clientPacket = PacketRegistry.ClientPacketInfo[packetType].Instance;
        return true;
    }

    /// <summary>
    /// Deserializes a client packet from the reader. Returns <c>false</c> on malformed data.
    /// </summary>
    private static bool TryReadPacket(ClientPacket clientPacket, PacketReader packetReader, out string errorMessage)
    {
        try
        {
            clientPacket.Read(packetReader);
            errorMessage = string.Empty;
            return true;
        }
        catch (EndOfStreamException exception)
        {
            errorMessage = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// Invokes a registered client packet handler, catching and logging any exceptions.
    /// </summary>
    private bool TryInvokePacketHandler(Action<PacketFromPeer<ClientPacket>> handler, PacketFromPeer<ClientPacket> peer)
    {
        try
        {
            handler(peer);
            return true;
        }
        catch (ObjectDisposedException exception)
        {
            LoggerService.LogErr(exception, LogTag);
            return false;
        }
        catch (InvalidOperationException exception)
        {
            LoggerService.LogErr(exception, LogTag);
            return false;
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            LoggerService.LogErr(exception, LogTag);
            return false;
        }
    }

    /// <summary>
    /// Logs an incoming packet when packet-received logging is enabled.
    /// </summary>
    private void LogPacketReceived(Type packetType, uint clientId, ClientPacket packet)
    {
        if (!Options.PrintPacketReceived || IgnoredPackets.Contains(packetType))
            return;

        string packetData = string.Empty;
        if (Options.PrintPacketData)
        {
            packetData = $"\n{packet.ToFormattedString()}";
        }

        Log($"Received packet: {packetType.Name} from client {clientId}{packetData}");
    }

    /// <summary>
    /// Removes peer state and invokes <see cref="OnPeerDisconnected"/> for a disconnect or timeout event.
    /// </summary>
    private void HandlePeerDisconnected(Event netEvent, Action<uint> logEvent)
    {
        uint peerId = netEvent.Peer.ID;
        if (RemovePeerState(peerId))
            TryInvokePeerDisconnected(peerId);

        logEvent(peerId);
    }

    /// <summary>
    /// Safely invokes <see cref="OnPeerDisconnected"/>, catching and logging any exceptions.
    /// </summary>
    private void TryInvokePeerDisconnected(uint peerId)
    {
        try
        {
            OnPeerDisconnected(peerId);
        }
        catch (ObjectDisposedException exception)
        {
            LoggerService.LogErr(exception, LogTag);
        }
        catch (InvalidOperationException exception)
        {
            LoggerService.LogErr(exception, LogTag);
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            LoggerService.LogErr(exception, LogTag);
        }
    }

    /// <summary>
    /// Drains the outgoing queue and transmits each message over ENet.
    /// </summary>
    private void ProcessOutgoingPackets()
    {
        while (_outgoing.TryDequeue(out OutgoingMessage message))
        {
            Interlocked.Decrement(ref _outgoingDepth);

            try
            {
                if (message.IsBroadcast)
                {
                    SendBroadcast(message);
                }
                else
                {
                    SendUnicast(message);
                }
            }
            catch (ObjectDisposedException exception)
            {
                LoggerService.LogErr(exception, $"{LogTag}: outgoing packet target disposed");
            }
            catch (InvalidOperationException exception)
            {
                LoggerService.LogErr(exception, $"{LogTag}: invalid outgoing packet state");
            }
            catch (ArgumentException exception)
            {
                LoggerService.LogErr(exception, $"{LogTag}: invalid outgoing packet arguments");
            }
            catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
            {
                LoggerService.LogErr(exception, LogTag);
            }
        }
    }

    /// <summary>
    /// Sends a message to a single peer, fragmenting automatically when needed.
    /// </summary>
    private void SendUnicast(OutgoingMessage message)
    {
        if (!_peers.TryGetValue(message.TargetPeerId, out Peer peer))
            return;

        if (message.Data.Length > GamePacket.MaxSize)
        {
            foreach (byte[] fragment in PacketFragmenter.Fragment(message.Data, _streamCounter++, GetMaxFragmentsPerPacket()))
            {
                Packet fragPacket = CreateReliablePacket(fragment);
                peer.Send(DefaultChannelId, ref fragPacket);
            }

            return;
        }

        Packet enetPacket = CreateReliablePacket(message.Data);
        peer.Send(DefaultChannelId, ref enetPacket);
    }

    /// <summary>
    /// Broadcasts a message to all peers (or all except one), fragmenting when needed.
    /// </summary>
    private void SendBroadcast(OutgoingMessage message)
    {
        if (message.Data.Length > GamePacket.MaxSize)
        {
            foreach (byte[] fragment in PacketFragmenter.Fragment(message.Data, _streamCounter++, GetMaxFragmentsPerPacket()))
            {
                Packet fragPacket = CreateReliablePacket(fragment);

                if (message.HasExclusion && _peers.TryGetValue(message.ExcludePeerId, out Peer excludePeer))
                    Host.Broadcast(DefaultChannelId, ref fragPacket, excludePeer);
                else
                    Host.Broadcast(DefaultChannelId, ref fragPacket);
            }

            return;
        }

        Packet enetPacket = CreateReliablePacket(message.Data);

        if (message.HasExclusion && _peers.TryGetValue(message.ExcludePeerId, out Peer excl))
            Host.Broadcast(DefaultChannelId, ref enetPacket, excl);
        else
            Host.Broadcast(DefaultChannelId, ref enetPacket);
    }

    private void EnqueueCommand(Cmd<ENetServerOpcode> command)
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

    private void EnqueueIncomingPacket(Packet packet, Peer peer)
    {
        int limit = GetIncomingQueueLimit();
        if (TryReserveQueueSlot(ref _incomingDepth, limit, out int depth))
        {
            _incoming.Enqueue(new IncomingPacket { Packet = packet, Peer = peer });
            UpdateHighWaterMark(ref _incomingHighWaterMark, depth);
            return;
        }

        HandleIncomingQueueOverflow(packet, peer, limit);
    }

    private void EnqueueOutgoingMessage(OutgoingMessage message)
    {
        int limit = GetOutgoingQueueLimit();
        if (TryReserveQueueSlot(ref _outgoingDepth, limit, out int depth))
        {
            _outgoing.Enqueue(message);
            UpdateHighWaterMark(ref _outgoingHighWaterMark, depth);
            return;
        }

        HandleOutgoingQueueOverflow(message, limit);
    }

    private void HandleCommandQueueOverflow(Cmd<ENetServerOpcode> command, int limit)
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

    private void HandleIncomingQueueOverflow(Packet packet, Peer peer, int limit)
    {
        QueueOverflowPolicy policy = GetIncomingQueueOverflowPolicy();

        if (policy == QueueOverflowPolicy.DropOldest && _incoming.TryDequeue(out IncomingPacket droppedPacket))
        {
            Interlocked.Decrement(ref _incomingDepth);
            droppedPacket.Packet.Dispose();
            long dropped = Interlocked.Increment(ref _incomingDroppedCount);

            if (TryReserveQueueSlot(ref _incomingDepth, limit, out int depth))
            {
                _incoming.Enqueue(new IncomingPacket { Packet = packet, Peer = peer });
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
            DisconnectPeer(peer.ID, peer, DisconnectOpcode.Kicked);
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

    private void HandleOutgoingQueueOverflow(OutgoingMessage message, int limit)
    {
        QueueOverflowPolicy policy = GetOutgoingQueueOverflowPolicy();

        if (policy == QueueOverflowPolicy.DropOldest && _outgoing.TryDequeue(out _))
        {
            Interlocked.Decrement(ref _outgoingDepth);
            long dropped = Interlocked.Increment(ref _outgoingDroppedCount);

            if (TryReserveQueueSlot(ref _outgoingDepth, limit, out int depth))
            {
                _outgoing.Enqueue(message);
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
        }

        if (policy == QueueOverflowPolicy.DisconnectNoisyPeer && !message.IsBroadcast && _peers.TryGetValue(message.TargetPeerId, out Peer peer))
        {
            DisconnectPeer(peer.ID, peer, DisconnectOpcode.Kicked);
        }

        long droppedNewest = Interlocked.Increment(ref _outgoingDroppedCount);
        LogQueueOverflow(
            queueName: "outgoing",
            policy,
            action: policy == QueueOverflowPolicy.DisconnectNoisyPeer
                ? "Dropped newest message (and disconnected target peer for unicast)"
                : "Dropped newest message",
            droppedNewest,
            Volatile.Read(ref _outgoingHighWaterMark),
            limit);
    }

    private void DisconnectPeer(uint peerId, Peer peer, DisconnectOpcode opcode)
    {
        peer.DisconnectNow((uint)opcode);
        if (RemovePeerState(peerId))
            TryInvokePeerDisconnected(peerId);
    }

    private bool RemovePeerState(uint peerId)
    {
        bool removed = _peers.Remove(peerId);
        _reassemblyBuffers.Remove(peerId);

        if (removed)
            Interlocked.Decrement(ref _connectedPeerCount);

        return removed;
    }

    private void LogMalformedFragmentDrop(uint peerId, string reason)
    {
        int intervalMs = NormalizePositive(
            Options?.MalformedFragmentLogIntervalMs ?? DefaultMalformedFragmentLogIntervalMs,
            DefaultMalformedFragmentLogIntervalMs);

        string throttleKey = $"{peerId}:{reason}";
        if (!ShouldLogNow(_malformedFragmentLogTicks, throttleKey, intervalMs))
            return;

        Log($"Dropped malformed fragment from peer {peerId}. reason={reason}");
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
}
