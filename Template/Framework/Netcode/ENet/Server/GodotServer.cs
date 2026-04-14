using GodotUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Main-thread server facade. Provides packet handler registration and exposes
/// send and broadcast helpers callable from the Godot main thread.
/// </summary>
public abstract class GodotServer : ENetServer
{
    private const string LogTag = "Server";

    /// <summary>
    /// Initializes a new server facade.
    /// </summary>
    protected GodotServer()
    {
        // subclasses may register packet handlers in their own constructors
    }


    /// <summary>
    /// Starts the server on <paramref name="port"/>. Options control logging; types in ignoredPackets are excluded from log output.
    /// </summary>
    /// <param name="port">UDP port to bind.</param>
    /// <param name="maxClients">Maximum simultaneous connected peers.</param>
    /// <param name="options">ENet runtime options for logging and queue behavior.</param>
    /// <param name="ignoredPackets">Packet types excluded from verbose packet logging.</param>
    public void Start(ushort port, int maxClients, ENetOptions options, params Type[] ignoredPackets)
    {
        // Ignore duplicate start requests while server is already running.
        if (IsRunning)
        {
            Log("Server is running already");
            return;
        }

        Options = options ?? new ENetOptions();
        InitIgnoredPackets(ignoredPackets);
        CTS = new CancellationTokenSource();
        _ = StartWorkerThreadAsync(port, maxClients);
    }

    /// <summary>
    /// Starts the ENet worker thread via the thread pool and handles terminal exceptions.
    /// </summary>
    /// <param name="port">UDP port to bind.</param>
    /// <param name="maxClients">Maximum simultaneous connected peers.</param>
    /// <returns>A task representing the worker-thread lifecycle.</returns>
    private async Task StartWorkerThreadAsync(ushort port, int maxClients)
    {
        try
        {
            await Task.Factory.StartNew(
                () => WorkerThread(port, maxClients),
                CTS.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping the server.
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            string message = exception switch
            {
                ObjectDisposedException => $"{LogTag}: server worker failed because resources were disposed",
                InvalidOperationException => $"{LogTag}: server worker failed to start due to invalid state",
                _ => LogTag,
            };

            LoggerService.LogErr(exception, message);
        }
    }

    /// <summary>
    /// Ban someone by their ID.
    /// </summary>
    /// <param name="id">Peer identifier to ban.</param>
    public void Ban(uint id)
    {
        Kick(id, DisconnectOpcode.Banned);
    }

    /// <summary>
    /// Ban everyone on the server.
    /// </summary>
    public void BanAll()
    {
        KickAll(DisconnectOpcode.Banned);
    }

    /// <summary>
    /// Kick someone by their ID with a specified opcode.
    /// </summary>
    /// <param name="id">Peer identifier to disconnect.</param>
    /// <param name="opcode">Disconnect reason sent to the peer.</param>
    public void Kick(uint id, DisconnectOpcode opcode)
    {
        RequestKick(id, opcode);
    }

    /// <summary>
    /// Stop the server.
    /// </summary>
    public sealed override void Stop()
    {
        // Treat repeated stop requests as a no-op.
        if (!IsRunning)
        {
            Log("Server has stopped already");
            return;
        }

        RequestStop();
    }

    /// <summary>
    /// Send a packet to one client by peer ID.
    /// </summary>
    /// <param name="packet">Packet to serialize and send.</param>
    /// <param name="peerId">Target peer identifier.</param>
    public void Send(ServerPacket packet, uint peerId)
    {
        ArgumentNullException.ThrowIfNull(packet);

        packet.Write();
        LogSend(packet, $"to client {peerId}");
        EnqueueOutgoing(OutgoingMessage.Unicast(packet.GetData(), peerId));
    }

    /// <summary>
    /// Serializes <paramref name="packet"/> once and sends to each peer ID in the collection.
    /// </summary>
    /// <param name="packet">Packet to serialize and send.</param>
    /// <param name="peerIds">Target peer identifiers.</param>
    public void Send(ServerPacket packet, IEnumerable<uint> peerIds)
    {
        ArgumentNullException.ThrowIfNull(packet);

        // Ignore null peer-id collections.
        if (peerIds == null)
            return;

        packet.Write();
        LogSend(packet, $"to {nameof(peerIds)}");
        byte[] data = packet.GetData();

        foreach (uint peerId in peerIds)
            EnqueueOutgoing(OutgoingMessage.Unicast(data, peerId));
    }

    // Throttling state keyed by packet opcode.
    private readonly Dictionary<ushort, long> _lastThrottleTicks = [];

    /// <summary>
    /// Returns <c>true</c> and updates the last-send timestamp when the throttle interval has elapsed.
    /// </summary>
    /// <param name="key">Throttle key, typically the packet opcode.</param>
    /// <param name="intervalMs">Minimum interval in milliseconds between sends for the key.</param>
    /// <returns><see langword="true"/> when throttling allows a send now.</returns>
    private bool CanThrottle(ushort key, int intervalMs)
    {
        long now = Stopwatch.GetTimestamp();
        long threshold = (long)(intervalMs * (double)Stopwatch.Frequency / 1000.0);

        // Allow send when key is new or interval has elapsed.
        if (!_lastThrottleTicks.TryGetValue(key, out long last) || now - last >= threshold)
        {
            _lastThrottleTicks[key] = now;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Executes <paramref name="sendAction"/> when the throttle interval has elapsed or <paramref name="force"/> is true.
    /// </summary>
    /// <param name="packet">Packet used to derive throttle key.</param>
    /// <param name="intervalMs">Minimum interval in milliseconds between sends.</param>
    /// <param name="sendAction">Action that performs the actual send operation.</param>
    /// <param name="force">Whether to bypass throttle checks.</param>
    /// <returns><see langword="true"/> when a send action was executed.</returns>
    private bool Throttle(ServerPacket packet,
        int intervalMs,
        Action sendAction,
        bool force)
    {
        ArgumentNullException.ThrowIfNull(packet);

        ushort key = packet.GetOpcode();

        // Execute send when forced or throttle window allows it.
        if (force || CanThrottle(key, intervalMs))
        {
            sendAction();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sends to a peer collection, rate-limited to once per <paramref name="intervalMs"/>ms. Returns true if sent.
    /// </summary>
    /// <param name="packet">Packet to send.</param>
    /// <param name="peerIds">Target peer identifiers.</param>
    /// <param name="intervalMs">Throttle interval in milliseconds.</param>
    /// <param name="force">Whether to bypass throttle checks.</param>
    /// <returns><see langword="true"/> when packets were sent.</returns>
    public bool SendThrottled(ServerPacket packet,
        IEnumerable<uint> peerIds,
        int intervalMs,
        bool force = false)
    {
        // Skip throttled send when target collection is missing.
        if (peerIds == null)
            return false;

        return Throttle(packet, intervalMs, () => Send(packet, peerIds), force);
    }

    /// <summary>
    /// Broadcasts to all clients, rate-limited to once per <paramref name="intervalMs"/>ms.
    /// </summary>
    /// <param name="packet">Packet to broadcast.</param>
    /// <param name="intervalMs">Throttle interval in milliseconds.</param>
    /// <param name="force">Whether to bypass throttle checks.</param>
    /// <returns><see langword="true"/> when the packet was broadcast.</returns>
    public bool BroadcastThrottled(ServerPacket packet, int intervalMs, bool force = false)
    {
        return Throttle(packet, intervalMs, () => Broadcast(packet), force);
    }

    /// <summary>
    /// Broadcasts to all clients except <paramref name="excludePeerId"/>, rate-limited to once per <paramref name="intervalMs"/>ms.
    /// </summary>
    /// <param name="packet">Packet to broadcast.</param>
    /// <param name="excludePeerId">Peer identifier excluded from the broadcast.</param>
    /// <param name="intervalMs">Throttle interval in milliseconds.</param>
    /// <param name="force">Whether to bypass throttle checks.</param>
    /// <returns><see langword="true"/> when the packet was broadcast.</returns>
    public bool BroadcastThrottled(ServerPacket packet, uint excludePeerId, int intervalMs, bool force = false)
    {
        return Throttle(packet, intervalMs, () => Broadcast(packet, excludePeerId), force);
    }


    /// <summary>
    /// Serializes and sends each packet in the sequence to a single peer.
    /// </summary>
    /// <param name="peerId">Target peer identifier.</param>
    /// <param name="packets">Packets to serialize and send.</param>
    public void Send(uint peerId, IEnumerable<ServerPacket> packets)
    {
        // Ignore null packet sequences.
        if (packets == null)
            return;

        foreach (ServerPacket packet in packets)
            Send(packet, peerId);
    }

    /// <summary>
    /// Broadcasts each packet in the sequence to all clients.
    /// </summary>
    /// <param name="packets">Packets to serialize and broadcast.</param>
    public void Broadcast(IEnumerable<ServerPacket> packets)
    {
        // Ignore null packet sequences.
        if (packets == null)
            return;

        foreach (ServerPacket packet in packets)
            Broadcast(packet);
    }

    /// <summary>
    /// Broadcast several packets to all clients except one.
    /// </summary>
    /// <param name="packets">Packets to serialize and broadcast.</param>
    /// <param name="excludePeerId">Peer identifier excluded from the broadcast.</param>
    public void Broadcast(IEnumerable<ServerPacket> packets, uint excludePeerId)
    {
        // Ignore null packet sequences.
        if (packets == null)
            return;

        foreach (ServerPacket packet in packets)
            Broadcast(packet, excludePeerId);
    }

    /// <summary>
    /// Broadcast a packet to all connected clients.
    /// </summary>
    /// <param name="packet">Packet to serialize and broadcast.</param>
    public void Broadcast(ServerPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        packet.Write();
        LogSend(packet, "to everyone");
        EnqueueOutgoing(OutgoingMessage.Broadcast(packet.GetData()));
    }

    /// <summary>
    /// Broadcast a packet to all clients except the specified peer.
    /// </summary>
    /// <param name="packet">Packet to serialize and broadcast.</param>
    /// <param name="excludePeerId">Peer identifier excluded from the broadcast.</param>
    public void Broadcast(ServerPacket packet, uint excludePeerId)
    {
        ArgumentNullException.ThrowIfNull(packet);

        packet.Write();
        LogSend(packet, $"to everyone except {excludePeerId}");
        EnqueueOutgoing(OutgoingMessage.BroadcastExcept(packet.GetData(), excludePeerId));
    }

    /// <summary>
    /// Logs an outgoing packet when packet-sent logging is enabled.
    /// </summary>
    /// <param name="packet">Packet being sent.</param>
    /// <param name="targetDescription">Human-readable target description for log output.</param>
    private void LogSend(ServerPacket packet, string targetDescription)
    {
        Type packetType = packet.GetType();

        // Skip send logging when disabled or packet type is ignored.
        if (!Options.PrintPacketSent || IgnoredPackets.Contains(packetType))
            return;

        string byteSize = FormatByteSize(packet.GetSize());

        string packetData = string.Empty;

        // Include packet payload details only when verbose packet-data logging is enabled.
        if (Options.PrintPacketData)
            packetData = $"\n{packet.ToFormattedString()}";

        Log($"Sending packet {packetType.Name} {byteSize}{targetDescription}{packetData}");
    }
}
