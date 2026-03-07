using ENet;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Framework.Netcode.Client;

// ENet API Reference: https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md
/// <summary>
/// Base ENet client worker that owns the connection lifecycle, send queue, and packet dispatch.
/// Extend <see cref="GodotClient"/> for game-level packet registration.
/// </summary>
public abstract class ENetClient : ENetLow
{
    private const string LogTag = "Client";

    private readonly ConcurrentQueue<Cmd<ENetClientOpcode>> _enetCmds = new();
    private readonly ConcurrentQueue<byte[]> _outgoing = new();
    private readonly ConcurrentQueue<Packet> _incoming = new();
    private static readonly ClientLogAggregator _logAggregator = new();
    private static int _activeClientWorkers;
    private Peer _peer;
    private ushort _streamCounter;
    private readonly Dictionary<ushort, FragmentBuffer> _reassemblyBuffers = [];

    protected ConcurrentQueue<Cmd<GodotOpcode>> MainThreadCommands { get; } = new();
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

    public uint PeerId => _peer.ID;

    /// <summary>
    /// Log messages as the client. Thread safe.
    /// </summary>
    public sealed override void Log(object message, BBColor color = BBColor.Gray)
    {
        string timestampPrefix = BuildTimestampPrefix();
        GameFramework.Logger.Log($"{timestampPrefix}[Client] {message}", color);
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
    /// Called on the worker thread when the connection is established.
    /// Use this to send initial packets such as join requests.
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
        TryInvoke(() => OnConnected());
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
        TryInvoke(() => OnDisconnected());
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
        TryInvoke(() => OnTimedOut());
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

        _incoming.Enqueue(packet);
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

    private string BuildTimestampPrefix()
    {
        if (Options == null || !Options.ShowLogTimestamps)
        {
            return string.Empty;
        }

        return $"[{DateTime.Now:HH:mm:ss}] ";
    }

    private void ProcessENetCommands()
    {
        while (_enetCmds.TryDequeue(out Cmd<ENetClientOpcode> command))
        {
            switch (command.Opcode)
            {
                case ENetClientOpcode.Disconnect:
                    HandleDisconnectCommand();
                    break;
            }
        }
    }

    private void HandleDisconnectCommand()
    {
        if (CTS.IsCancellationRequested)
        {
            Log("Client is in the middle of stopping");
            return;
        }

        _peer.Disconnect((uint)DisconnectOpcode.Disconnected);
    }

    private void QueueDisconnectedCommand(DisconnectOpcode opcode)
    {
        MainThreadCommands.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Disconnected, opcode));
    }

    private void ProcessIncomingPackets()
    {
        while (_incoming.TryDequeue(out Packet packet))
        {
            // Copy bytes eagerly so we can inspect the opcode before any higher-level parsing.
            byte[] bytes = new byte[packet.Length];
            packet.CopyTo(bytes);
            packet.Dispose();

            if (PacketFragmenter.IsFragment(bytes))
            {
                HandleFragmentBytes(bytes);
                continue;
            }

            if (!TryCreatePacketData(bytes, out PacketData packetData))
                continue;

            MainThreadPackets.Enqueue(packetData);
        }
    }

    private void HandleFragmentBytes(byte[] fragmentBytes)
    {
        if (!PacketFragmenter.TryReadHeader(fragmentBytes, out ushort streamId, out ushort fragIndex, out ushort totalFragments))
            return;

        if (!_reassemblyBuffers.TryGetValue(streamId, out FragmentBuffer buffer))
        {
            buffer = new FragmentBuffer(totalFragments);
            _reassemblyBuffers[streamId] = buffer;
        }

        byte[] payload = PacketFragmenter.ExtractPayload(fragmentBytes);
        if (!buffer.Add(fragIndex, payload))
            return;

        _reassemblyBuffers.Remove(streamId);

        if (!TryCreatePacketData(buffer.Assemble(), out PacketData packetData))
            return;

        MainThreadPackets.Enqueue(packetData);
    }

    private bool TryCreatePacketData(byte[] bytes, out PacketData packetData)
    {
        packetData = null;
        PacketReader reader = new(bytes);

        if (!TryReadPacketType(reader, out Type packetType))
        {
            reader.Dispose();
            return false;
        }

        // The packet registry vends a shared singleton per packet type. Handlers must not
        // retain a reference to this instance across processing boundaries — the same object
        // is reused and mutated for every subsequent packet of the same type.
        ServerPacket handlerPacket = PacketRegistry.ServerPacketInfo[packetType].Instance;
        packetData = new PacketData
        {
            Type = packetType,
            PacketReader = reader,
            HandlePacket = handlerPacket
        };

        return true;
    }

    private bool TryReadPacketType(PacketReader reader, out Type packetType)
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

    private void ProcessOutgoingPackets()
    {
        while (_outgoing.TryDequeue(out byte[] data))
        {
            try
            {
                if (data.Length > GamePacket.MaxSize)
                {
                    foreach (byte[] fragment in PacketFragmenter.Fragment(data, _streamCounter++))
                    {
                        Packet fragPacket = CreateReliablePacket(fragment);
                        _peer.Send(DefaultChannelId, ref fragPacket);
                    }

                    continue;
                }

                Packet enetPacket = CreateReliablePacket(data);
                _peer.Send(DefaultChannelId, ref enetPacket);
            }
            catch (Exception exception)
            {
                GameFramework.Logger.LogErr(exception, LogTag);
            }
        }
    }

    /// <summary>
    /// Enqueues serialized packet data for sending on the worker thread. Thread safe.
    /// </summary>
    protected void EnqueueOutgoing(byte[] data)
    {
        _outgoing.Enqueue(data);
    }

    /// <summary>
    /// Requests a graceful disconnect from the worker thread. Thread safe.
    /// </summary>
    protected void RequestDisconnect()
    {
        _enetCmds.Enqueue(new Cmd<ENetClientOpcode>(ENetClientOpcode.Disconnect));
    }

    private static Address CreateAddress(string ip, ushort port)
    {
        Address address = new() { Port = port };
        address.SetHost(ip);
        return address;
    }

    private static void TryInvoke(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            GameFramework.Logger.LogErr(exception, LogTag);
        }
    }
}
