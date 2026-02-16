using ENet;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Framework.Netcode.Client;

// ENet API Reference: https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md
public abstract class ENetClient : ENetLow
{
    private const string LogTag = "Client";

    protected ConcurrentQueue<Cmd<ENetClientOpcode>> ENetCmds { get; } = new();
    protected ConcurrentQueue<Cmd<GodotOpcode>> GodotCmdsInternal { get; } = new();
    protected ConcurrentQueue<ClientPacket> Outgoing { get; } = new();
    protected ConcurrentQueue<PacketData> GodotPackets { get; } = new();

    protected Peer _peer;
    protected long _connected;

    private readonly ConcurrentQueue<Packet> _incoming = new();
    private static readonly ClientLogAggregator _logAggregator = new();
    private static int _activeClientWorkers;

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
        _logAggregator.Flush(force: false, message => Log(message));
    }

    /// <summary>
    /// Hook invoked after ENet reports a successful connection.
    /// </summary>
    protected virtual void OnConnect(Event netEvent)
    {
    }

    /// <summary>
    /// Hook invoked after ENet reports a disconnect.
    /// </summary>
    protected virtual void OnDisconnect(Event netEvent)
    {
    }

    /// <summary>
    /// Hook invoked after ENet reports a timeout.
    /// </summary>
    protected virtual void OnTimeout(Event netEvent)
    {
    }

    /// <summary>
    /// Internal connect handler that updates state and dispatches lifecycle callbacks.
    /// </summary>
    protected sealed override void OnConnectLow(Event netEvent)
    {
        Interlocked.Exchange(ref _connected, 1);
        GodotCmdsInternal.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Connected));
        _logAggregator.RecordConnect(netEvent.Peer.ID);
        TryInvoke(() => OnConnect(netEvent));
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
        TryInvoke(() => OnDisconnect(netEvent));
    }

    /// <summary>
    /// Internal timeout handler that updates state and dispatches lifecycle callbacks.
    /// </summary>
    protected sealed override void OnTimeoutLow(Event netEvent)
    {
        QueueDisconnectedCommand(DisconnectOpcode.Timeout);
        GodotCmdsInternal.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Timeout));

        OnDisconnectCleanup(netEvent.Peer);
        _logAggregator.RecordTimeout(netEvent.Peer.ID);
        TryInvoke(() => OnTimeout(netEvent));
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
            NotifyClientStopped();

            if (Interlocked.Decrement(ref _activeClientWorkers) == 0)
            {
                _logAggregator.Flush(force: true, message => Log(message));
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
        while (ENetCmds.TryDequeue(out Cmd<ENetClientOpcode> command))
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
        GodotCmdsInternal.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Disconnected, opcode));
    }

    private void ProcessIncomingPackets()
    {
        while (_incoming.TryDequeue(out Packet packet))
        {
            if (!TryCreatePacketData(packet, out PacketData packetData))
            {
                continue;
            }

            GodotPackets.Enqueue(packetData);
        }
    }

    private bool TryCreatePacketData(Packet packet, out PacketData packetData)
    {
        packetData = null;
        PacketReader reader = new(packet);

        if (!TryReadPacketType(reader, out Type packetType))
        {
            reader.Dispose();
            return false;
        }

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

        byte opcode;
        try
        {
            opcode = reader.ReadByte();
        }
        catch (EndOfStreamException exception)
        {
            Log($"Received malformed packet: {exception.Message} (Ignoring)");
            return false;
        }

        if (!PacketRegistry.ServerPacketTypes.TryGetValue(opcode, out packetType))
        {
            Log($"Received malformed opcode: {opcode} (Ignoring)");
            return false;
        }

        return true;
    }

    private void ProcessOutgoingPackets()
    {
        while (Outgoing.TryDequeue(out ClientPacket packet))
        {
            Type packetType = packet.GetType();

            try
            {
                LogOutgoingPacket(packetType, packet);
                packet.Send();
            }
            catch (Exception exception)
            {
                GameFramework.Logger.LogErr(exception, LogTag);
            }
        }
    }

    private void LogOutgoingPacket(Type packetType, ClientPacket packet)
    {
        if (!Options.PrintPacketSent || IgnoredPackets.Contains(packetType))
        {
            return;
        }

        string packetData = string.Empty;
        if (Options.PrintPacketData)
        {
            packetData = $"\n{packet.ToFormattedString()}";
        }

        Log($"Sent packet: {packetType.Name} {FormatByteSize(packet.GetSize())}{packetData}");
    }

    private static Address CreateAddress(string ip, ushort port)
    {
        Address address = new() { Port = port };
        address.SetHost(ip);
        return address;
    }

    /// <summary>
    /// Called when a client worker is about to start.
    /// </summary>
    protected static void NotifyClientStarting()
    {
        // Intentionally no-op to avoid noisy client lifecycle logs.
    }

    private static void NotifyClientStopped()
    {
        // Intentionally no-op to avoid noisy client lifecycle logs.
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

    private sealed class ClientLogAggregator
    {
        private const double QuietGapSeconds = 0.5;
        private const double MaxWindowSeconds = 5.0;

        private int _connectedCount;
        private int _disconnectedCount;
        private int _timeoutCount;

        private long _eventWindowStartTicks;
        private long _eventLastEventTicks;

        private long _lastConnectTicks;
        private long _lastDisconnectTicks;
        private long _lastTimeoutTicks;
        private long _lastConnectPeerId;
        private long _lastDisconnectPeerId;
        private long _lastTimeoutPeerId;

        /// <summary>
        /// Records a connect lifecycle event.
        /// </summary>
        public void RecordConnect(uint peerId)
        {
            Interlocked.Increment(ref _connectedCount);
            Interlocked.Exchange(ref _lastConnectPeerId, peerId);
            MarkEvent(ref _lastConnectTicks);
        }

        /// <summary>
        /// Records a disconnect lifecycle event.
        /// </summary>
        public void RecordDisconnect(uint peerId)
        {
            Interlocked.Increment(ref _disconnectedCount);
            Interlocked.Exchange(ref _lastDisconnectPeerId, peerId);
            MarkEvent(ref _lastDisconnectTicks);
        }

        /// <summary>
        /// Records a timeout lifecycle event.
        /// </summary>
        public void RecordTimeout(uint peerId)
        {
            Interlocked.Increment(ref _timeoutCount);
            Interlocked.Exchange(ref _lastTimeoutPeerId, peerId);
            MarkEvent(ref _lastTimeoutTicks);
        }

        /// <summary>
        /// Emits a coalesced lifecycle log report when burst thresholds are reached.
        /// </summary>
        public void Flush(bool force, Action<string> log)
        {
            int connectedSnapshot = Volatile.Read(ref _connectedCount);
            int disconnectedSnapshot = Volatile.Read(ref _disconnectedCount);
            int timeoutSnapshot = Volatile.Read(ref _timeoutCount);

            if (connectedSnapshot == 0 && disconnectedSnapshot == 0 && timeoutSnapshot == 0)
            {
                return;
            }

            long windowStartTicks = Interlocked.Read(ref _eventWindowStartTicks);
            long eventLastTicks = Interlocked.Read(ref _eventLastEventTicks);
            if (windowStartTicks == 0 || eventLastTicks == 0)
            {
                return;
            }

            long nowTicks = Stopwatch.GetTimestamp();
            double secondsSinceLast = (nowTicks - eventLastTicks) / (double)Stopwatch.Frequency;
            double windowSeconds = (eventLastTicks - windowStartTicks) / (double)Stopwatch.Frequency;

            if (!force && secondsSinceLast < QuietGapSeconds && windowSeconds < MaxWindowSeconds)
            {
                return;
            }

            if (!force && Interlocked.CompareExchange(ref _eventLastEventTicks, 0, eventLastTicks) != eventLastTicks)
            {
                return;
            }

            int connects = Interlocked.Exchange(ref _connectedCount, 0);
            int disconnects = Interlocked.Exchange(ref _disconnectedCount, 0);
            int timeouts = Interlocked.Exchange(ref _timeoutCount, 0);
            long lastConnectTicks = Interlocked.Exchange(ref _lastConnectTicks, 0);
            long lastDisconnectTicks = Interlocked.Exchange(ref _lastDisconnectTicks, 0);
            long lastTimeoutTicks = Interlocked.Exchange(ref _lastTimeoutTicks, 0);
            long lastConnectPeerId = Interlocked.Exchange(ref _lastConnectPeerId, 0);
            long lastDisconnectPeerId = Interlocked.Exchange(ref _lastDisconnectPeerId, 0);
            long lastTimeoutPeerId = Interlocked.Exchange(ref _lastTimeoutPeerId, 0);

            if (force)
            {
                Interlocked.Exchange(ref _eventLastEventTicks, 0);
            }

            Interlocked.CompareExchange(ref _eventWindowStartTicks, 0, windowStartTicks);

            double reportSeconds = Math.Max(windowSeconds, 0.01);
            List<(long Tick, Action LogAction)> logEntries = new(3);

            if (connects > 0)
            {
                logEntries.Add((lastConnectTicks, () => log(FormatConnectMessage(connects, lastConnectPeerId, reportSeconds))));
            }

            if (disconnects > 0)
            {
                logEntries.Add((lastDisconnectTicks, () => log(FormatDisconnectMessage(disconnects, lastDisconnectPeerId, reportSeconds))));
            }

            if (timeouts > 0)
            {
                logEntries.Add((lastTimeoutTicks, () => log(FormatTimeoutMessage(timeouts, lastTimeoutPeerId, reportSeconds))));
            }

            logEntries.Sort(static (left, right) => left.Tick.CompareTo(right.Tick));

            foreach ((long Tick, Action LogAction) in logEntries)
            {
                LogAction();
            }
        }

        private void MarkEvent(ref long eventTypeLastTicks)
        {
            long nowTicks = Stopwatch.GetTimestamp();
            if (Interlocked.CompareExchange(ref _eventWindowStartTicks, nowTicks, 0) == 0)
            {
                Interlocked.Exchange(ref _eventLastEventTicks, nowTicks);
            }
            else
            {
                Interlocked.Exchange(ref _eventLastEventTicks, nowTicks);
            }

            Interlocked.Exchange(ref eventTypeLastTicks, nowTicks);
        }

        private static string FormatCount(string singular, int count)
        {
            if (count == 1)
            {
                return $"1 {singular}";
            }

            return $"{count} {singular}s";
        }

        private static string FormatLastSuffix(int count, double seconds)
        {
            if (count == 1)
            {
                return string.Empty;
            }

            return $" (last {seconds:0.##}s)";
        }

        private static string FormatConnectMessage(int count, long peerId, double seconds)
        {
            if (count == 1)
            {
                if (peerId > 0)
                {
                    return $"Connected to server as peer {peerId}";
                }

                return "Connected to server";
            }

            return $"{FormatCount("connect event", count)}{FormatLastSuffix(count, seconds)}";
        }

        private static string FormatDisconnectMessage(int count, long peerId, double seconds)
        {
            if (count == 1)
            {
                if (peerId > 0)
                {
                    return $"Disconnected from server (peer {peerId})";
                }

                return "Disconnected from server";
            }

            return $"{FormatCount("disconnect event", count)}{FormatLastSuffix(count, seconds)}";
        }

        private static string FormatTimeoutMessage(int count, long peerId, double seconds)
        {
            if (count == 1)
            {
                if (peerId > 0)
                {
                    return $"Connection to server timed out (peer {peerId})";
                }

                return "Connection to server timed out";
            }

            return $"{FormatCount("timeout event", count)}{FormatLastSuffix(count, seconds)}";
        }
    }
}
