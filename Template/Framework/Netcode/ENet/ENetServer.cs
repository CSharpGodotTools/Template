using ENet;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Framework.Netcode.Server;

// ENet API Reference: https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md
public abstract class ENetServer : ENetLow
{
    private const string LogTag = "Server";

    protected ConcurrentQueue<Cmd<ENetServerOpcode>> ENetCmds { get; } = new();

    private readonly ConcurrentQueue<(Packet Packet, Peer Peer)> _incoming = new();
    private readonly ConcurrentQueue<ServerPacket> _outgoing = new();
    private readonly ConcurrentDictionary<Type, Action<ClientPacket, Peer>> _clientPacketHandlers = new();

    /// <summary>
    /// This dictionary is only accessed on the ENet worker thread.
    /// </summary>
    private readonly Dictionary<uint, Peer> _peers = [];

    private readonly ServerLogAggregator _logAggregator = new();

    protected void RegisterPacketHandler<TPacket>(Action<TPacket, Peer> handler)
        where TPacket : ClientPacket
    {
        ArgumentNullException.ThrowIfNull(handler);

        _clientPacketHandlers[typeof(TPacket)] = (packet, peer) => handler((TPacket)packet, peer);
    }

    /// <summary>
    /// Log a message as the server. This function is thread safe.
    /// </summary>
    public sealed override void Log(object message, BBColor color = BBColor.Gray)
    {
        string timestampPrefix = BuildTimestampPrefix();
        GameFramework.Logger.Log($"{timestampPrefix}[Server] {message}", color);
    }

    /// <summary>
    /// Kick everyone on the server with a specified opcode. Thread safe.
    /// </summary>
    public void KickAll(DisconnectOpcode opcode)
    {
        ENetCmds.Enqueue(new Cmd<ENetServerOpcode>(ENetServerOpcode.KickAll, opcode));
    }

    /// <summary>
    /// Enqueues a server packet for sending on the worker thread.
    /// </summary>
    protected void EnqueuePacket(ServerPacket packet)
    {
        _outgoing.Enqueue(packet);
    }

    /// <summary>
    /// Processes server worker queues each network tick.
    /// </summary>
    protected sealed override void ConcurrentQueues()
    {
        ProcessEnetCommands();
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
        _logAggregator.RecordConnect(netEvent.Peer.ID);
    }

    /// <summary>
    /// Hook invoked when a connected peer disconnects or times out.
    /// </summary>
    protected virtual void OnPeerDisconnect(Event netEvent)
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

        _incoming.Enqueue((packet, netEvent.Peer));
    }

    /// <summary>
    /// Runs the ENet server worker loop for the configured listen port.
    /// </summary>
    protected void WorkerThread(ushort port, int maxClients)
    {
        Host host = TryCreateServerHost(port, maxClients);
        if (host == null)
        {
            return;
        }

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
        _peers.Remove(peer.ID);
    }

    private string BuildTimestampPrefix()
    {
        if (Options == null || !Options.ShowLogTimestamps)
        {
            return string.Empty;
        }

        return $"[{DateTime.Now:HH:mm:ss}] ";
    }

    private Host TryCreateServerHost(ushort port, int maxClients)
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

    private void ProcessEnetCommands()
    {
        while (ENetCmds.TryDequeue(out Cmd<ENetServerOpcode> command))
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
    }

    private void HandleKickAllCommand(Cmd<ENetServerOpcode> command)
    {
        DisconnectOpcode opcode = (DisconnectOpcode)command.Data[0];
        DisconnectAllPeers(opcode);
    }

    private void DisconnectAllPeers(DisconnectOpcode opcode)
    {
        foreach (Peer peer in _peers.Values)
        {
            peer.DisconnectNow((uint)opcode);
        }

        _peers.Clear();
    }

    private void ProcessIncomingPackets()
    {
        while (_incoming.TryDequeue(out (Packet Packet, Peer Peer) queuedPacket))
        {
            HandleIncomingPacket(queuedPacket.Packet, queuedPacket.Peer);
        }
    }

    private void HandleIncomingPacket(Packet enetPacket, Peer peer)
    {
        PacketReader reader = new(enetPacket);

        try
        {
            if (!TryGetPacketAndType(reader, out ClientPacket packet, out Type packetType))
            {
                return;
            }

            if (!TryReadPacket(packet, reader, out string errorMessage))
            {
                Log($"Received malformed packet: {errorMessage} (Ignoring)");
                return;
            }

            if (!_clientPacketHandlers.TryGetValue(packetType, out Action<ClientPacket, Peer> handler))
            {
                Log($"No handler registered for client packet {packetType.Name} (Ignoring)");
                return;
            }

            if (!TryInvokePacketHandler(handler, packet, peer))
            {
                return;
            }

            LogPacketReceived(packetType, peer.ID, packet);
        }
        finally
        {
            reader.Dispose();
        }
    }

    private bool TryGetPacketAndType(PacketReader packetReader, out ClientPacket clientPacket, out Type packetType)
    {
        byte opcode;
        try
        {
            opcode = packetReader.ReadByte();
        }
        catch (EndOfStreamException exception)
        {
            Log($"Received malformed packet: {exception.Message} (Ignoring)");
            clientPacket = null;
            packetType = null;
            return false;
        }

        if (!PacketRegistry.ClientPacketTypes.TryGetValue(opcode, out packetType))
        {
            Log($"Received malformed opcode: {opcode} (Ignoring)");
            clientPacket = null;
            return false;
        }

        clientPacket = PacketRegistry.ClientPacketInfo[packetType].Instance;
        return true;
    }

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

    private static bool TryInvokePacketHandler(Action<ClientPacket, Peer> handler, ClientPacket packet, Peer peer)
    {
        try
        {
            handler(packet, peer);
            return true;
        }
        catch (Exception exception)
        {
            GameFramework.Logger.LogErr(exception, LogTag);
            return false;
        }
    }

    private void LogPacketReceived(Type packetType, uint clientId, ClientPacket packet)
    {
        if (!Options.PrintPacketReceived || IgnoredPackets.Contains(packetType))
        {
            return;
        }

        string packetData = string.Empty;
        if (Options.PrintPacketData)
        {
            packetData = $"\n{packet.ToFormattedString()}";
        }

        Log($"Received packet: {packetType.Name} from client {clientId}{packetData}");
    }

    private void HandlePeerDisconnected(Event netEvent, Action<uint> logEvent)
    {
        _peers.Remove(netEvent.Peer.ID);
        TryInvokePeerDisconnect(netEvent);
        logEvent(netEvent.Peer.ID);
    }

    private void TryInvokePeerDisconnect(Event netEvent)
    {
        try
        {
            OnPeerDisconnect(netEvent);
        }
        catch (Exception exception)
        {
            GameFramework.Logger.LogErr(exception, LogTag);
        }
    }

    private void ProcessOutgoingPackets()
    {
        while (_outgoing.TryDequeue(out ServerPacket packet))
        {
            try
            {
                SendType sendType = packet.GetSendType();
                switch (sendType)
                {
                    case SendType.Peer:
                        packet.Send();
                        break;

                    case SendType.Broadcast:
                        packet.Broadcast(Host);
                        break;
                }
            }
            catch (Exception exception)
            {
                GameFramework.Logger.LogErr(exception, LogTag);
            }
        }
    }

    private sealed class ServerLogAggregator
    {
        private const double QuietGapSeconds = 0.5;
        private const double MaxWindowSeconds = 5.0;

        private int _connectedCount;
        private int _disconnectedCount;
        private int _timeoutCount;

        private long _windowStartTicks;
        private long _lastEventTicks;

        private long _lastConnectTicks;
        private long _lastDisconnectTicks;
        private long _lastTimeoutTicks;
        private uint _lastConnectPeerId;
        private uint _lastDisconnectPeerId;
        private uint _lastTimeoutPeerId;

        /// <summary>
        /// Records a connect lifecycle event.
        /// </summary>
        public void RecordConnect(uint peerId)
        {
            _connectedCount++;
            _lastConnectPeerId = peerId;
            MarkEvent(ref _lastConnectTicks);
        }

        /// <summary>
        /// Records a disconnect lifecycle event.
        /// </summary>
        public void RecordDisconnect(uint peerId)
        {
            _disconnectedCount++;
            _lastDisconnectPeerId = peerId;
            MarkEvent(ref _lastDisconnectTicks);
        }

        /// <summary>
        /// Records a timeout lifecycle event.
        /// </summary>
        public void RecordTimeout(uint peerId)
        {
            _timeoutCount++;
            _lastTimeoutPeerId = peerId;
            MarkEvent(ref _lastTimeoutTicks);
        }

        /// <summary>
        /// Emits a coalesced lifecycle log report when burst thresholds are reached.
        /// </summary>
        public void Flush(Action<string> log)
        {
            if (_connectedCount == 0 && _disconnectedCount == 0 && _timeoutCount == 0)
            {
                return;
            }

            if (_windowStartTicks == 0 || _lastEventTicks == 0)
            {
                return;
            }

            long nowTicks = Stopwatch.GetTimestamp();
            double sinceLast = (nowTicks - _lastEventTicks) / (double)Stopwatch.Frequency;
            double windowSeconds = (_lastEventTicks - _windowStartTicks) / (double)Stopwatch.Frequency;

            if (sinceLast < QuietGapSeconds && windowSeconds < MaxWindowSeconds)
            {
                return;
            }

            int connects = _connectedCount;
            int disconnects = _disconnectedCount;
            int timeouts = _timeoutCount;
            long lastConnectTicks = _lastConnectTicks;
            long lastDisconnectTicks = _lastDisconnectTicks;
            long lastTimeoutTicks = _lastTimeoutTicks;
            uint lastConnectPeerId = _lastConnectPeerId;
            uint lastDisconnectPeerId = _lastDisconnectPeerId;
            uint lastTimeoutPeerId = _lastTimeoutPeerId;

            _connectedCount = 0;
            _disconnectedCount = 0;
            _timeoutCount = 0;
            _windowStartTicks = 0;
            _lastEventTicks = 0;
            _lastConnectTicks = 0;
            _lastDisconnectTicks = 0;
            _lastTimeoutTicks = 0;
            _lastConnectPeerId = 0;
            _lastDisconnectPeerId = 0;
            _lastTimeoutPeerId = 0;

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
            if (_windowStartTicks == 0)
            {
                _windowStartTicks = nowTicks;
            }

            _lastEventTicks = nowTicks;
            eventTypeLastTicks = nowTicks;
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

        private static string FormatConnectMessage(int count, uint peerId, double seconds)
        {
            if (count == 1)
            {
                return $"Client with id {peerId} connected";
            }

            return $"{FormatCount("client", count)} connected{FormatLastSuffix(count, seconds)}";
        }

        private static string FormatDisconnectMessage(int count, uint peerId, double seconds)
        {
            if (count == 1)
            {
                return $"Client with id {peerId} disconnected";
            }

            return $"{FormatCount("client", count)} disconnected{FormatLastSuffix(count, seconds)}";
        }

        private static string FormatTimeoutMessage(int count, uint peerId, double seconds)
        {
            if (count == 1)
            {
                return $"Client with id {peerId} timed out";
            }

            return $"{FormatCount("client", count)} timed out{FormatLastSuffix(count, seconds)}";
        }
    }
}
