using ENet;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Framework.Netcode.Server;

// ENet API Reference: https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md
/// <summary>
/// Base ENet server worker that owns peer tracking, incoming packet dispatch, and outgoing message queues.
/// Extend <see cref="GodotServer"/> for game-level packet registration.
/// </summary>
public abstract partial class ENetServer : ENetLow
{
    private const string LogTag = "Server";

    private readonly ConcurrentQueue<Cmd<ENetServerOpcode>> _enetCmds = new();
    private readonly ConcurrentQueue<IncomingPacket> _incoming = new();
    private readonly ConcurrentQueue<OutgoingMessage> _outgoing = new();
    private readonly ConcurrentDictionary<Type, Action<PacketFromPeer<ClientPacket>>> _clientPacketHandlers = new();

    /// <summary>
    /// Peer lookup. Only accessed on the ENet worker thread.
    /// </summary>
    private readonly Dictionary<uint, Peer> _peers = [];

    private readonly ServerLogAggregator _logAggregator = new();
    private ushort _streamCounter;
    private readonly Dictionary<uint, Dictionary<ushort, FragmentBuffer>> _reassemblyBuffers = [];
    private int _connectedPeerCount;

    /// <summary>
    /// Number of currently connected peers.
    /// </summary>
    public int ConnectedPeerCount => Interlocked.CompareExchange(ref _connectedPeerCount, 0, 0);

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
        GameFramework.Logger.Log($"{timestampPrefix}[Server] {message}", color);
    }

    /// <summary>
    /// Kick everyone on the server with a specified opcode.
    /// </summary>
    public void KickAll(DisconnectOpcode opcode)
    {
        _enetCmds.Enqueue(new Cmd<ENetServerOpcode>(ENetServerOpcode.KickAll, opcode));
    }

    /// <summary>
    /// Enqueues a send/broadcast command for the worker thread.
    /// </summary>
    protected void EnqueueOutgoing(OutgoingMessage message)
    {
        _outgoing.Enqueue(message);
    }

    /// <summary>
    /// Requests a graceful server shutdown from any thread.
    /// </summary>
    protected void RequestStop()
    {
        _enetCmds.Enqueue(new Cmd<ENetServerOpcode>(ENetServerOpcode.Stop));
    }

    /// <summary>
    /// Requests a peer kick from any thread.
    /// </summary>
    protected void RequestKick(uint peerId, DisconnectOpcode opcode)
    {
        _enetCmds.Enqueue(new Cmd<ENetServerOpcode>(ENetServerOpcode.Kick, peerId, opcode));
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

        _incoming.Enqueue(new IncomingPacket { Packet = packet, Peer = netEvent.Peer });
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
        if (_peers.Remove(peer.ID))
        {
            Interlocked.Decrement(ref _connectedPeerCount);
        }
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

        peer.DisconnectNow((uint)opcode);
        _peers.Remove(peerId);
        Interlocked.Decrement(ref _connectedPeerCount);
        _reassemblyBuffers.Remove(peerId);
        TryInvokePeerDisconnected(peerId);
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
        foreach (Peer peer in _peers.Values)
        {
            peer.DisconnectNow((uint)opcode);
            TryInvokePeerDisconnected(peer.ID);
        }

        Interlocked.Exchange(ref _connectedPeerCount, 0);
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
            return;

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
    private static bool TryInvokePacketHandler(Action<PacketFromPeer<ClientPacket>> handler, PacketFromPeer<ClientPacket> peer)
    {
        try
        {
            handler(peer);
            return true;
        }
        catch (Exception exception)
        {
            GameFramework.Logger.LogErr(exception, LogTag);
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
        _peers.Remove(peerId);
        _reassemblyBuffers.Remove(peerId);
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
        catch (Exception exception)
        {
            GameFramework.Logger.LogErr(exception, LogTag);
        }
    }

    /// <summary>
    /// Drains the outgoing queue and transmits each message over ENet.
    /// </summary>
    private void ProcessOutgoingPackets()
    {
        while (_outgoing.TryDequeue(out OutgoingMessage message))
        {
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
            catch (Exception exception)
            {
                GameFramework.Logger.LogErr(exception, LogTag);
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
            foreach (byte[] fragment in PacketFragmenter.Fragment(message.Data, _streamCounter++))
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
            foreach (byte[] fragment in PacketFragmenter.Fragment(message.Data, _streamCounter++))
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
}
