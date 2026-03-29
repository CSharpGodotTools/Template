using ENet;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace __TEMPLATE__.Netcode.Server;

internal sealed class ServerIncomingProcessor
{
    private const int DefaultMalformedFragmentLogIntervalMs = 2000;

    private readonly ServerQueueManager _queues;
    private readonly ServerPeerStore _peers;
    private readonly Func<ENetOptions?> _optionsProvider;
    private readonly Func<System.Collections.Generic.HashSet<Type>> _ignoredPacketsProvider;
    private readonly Action<string> _log;
    private readonly Action<Exception> _logError;
    private readonly ConcurrentDictionary<Type, Action<PacketFromPeer<ClientPacket>>> _handlers = new();
    private readonly ConcurrentDictionary<string, long> _malformedFragmentLogTicks = new();

    public ServerIncomingProcessor(
        ServerQueueManager queues,
        ServerPeerStore peers,
        Func<ENetOptions?> optionsProvider,
        Func<System.Collections.Generic.HashSet<Type>> ignoredPacketsProvider,
        Action<string> log,
        Action<Exception> logError)
    {
        _queues = queues;
        _peers = peers;
        _optionsProvider = optionsProvider;
        _ignoredPacketsProvider = ignoredPacketsProvider;
        _log = log;
        _logError = logError;
    }

    public void RegisterHandler<TPacket>(Action<PacketFromPeer<TPacket>> handler) where TPacket : ClientPacket
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handlers[typeof(TPacket)] = peer => handler(new PacketFromPeer<TPacket> { Packet = (TPacket)peer.Packet, PeerId = peer.PeerId });
    }

    public void Process()
    {
        while (_queues.TryDequeueIncoming(out IncomingPacket? queued))
        {
            if (queued == null)
                continue;

            byte[] bytes = new byte[queued.Packet.Length];
            queued.Packet.CopyTo(bytes);
            queued.Packet.Dispose();
            if (PacketFragmenter.IsFragment(bytes)) { HandleFragment(bytes, queued.Peer); continue; }
            Dispatch(bytes, queued.Peer);
        }
    }

    private void HandleFragment(byte[] bytes, Peer peer)
    {
        if (!PacketFragmenter.TryReadHeader(bytes, out ushort streamId, out ushort fragIndex, out ushort totalFragments))
        {
            LogMalformed(peer.ID, "Fragment header was truncated.");
            return;
        }

        if (!PacketFragmenter.IsValidHeader(fragIndex, totalFragments, GetMaxFragmentsPerPacket(), out string validationError))
        {
            LogMalformed(peer.ID, $"stream={streamId}: {validationError}");
            return;
        }

        System.Collections.Generic.Dictionary<ushort, FragmentBuffer> buffers = _peers.GetOrCreateReassembly(peer.ID);
        if (!buffers.TryGetValue(streamId, out FragmentBuffer? buffer))
        {
            buffer = new FragmentBuffer(totalFragments);
            buffers[streamId] = buffer;
        }
        else if (buffer.TotalFragments != totalFragments)
        {
            buffers.Remove(streamId);
            LogMalformed(peer.ID, $"stream={streamId}: fragment count changed from {buffer.TotalFragments} to {totalFragments}.");
            return;
        }

        byte[] payload = PacketFragmenter.ExtractPayload(bytes);
        if (!buffer.Add(fragIndex, payload)) return;
        buffers.Remove(streamId);
        Dispatch(buffer.Assemble(), peer);
    }

    private void Dispatch(byte[] bytes, Peer peer)
    {
        using PacketReader reader = new(bytes);
        if (!TryResolvePacket(reader, out ClientPacket? packet, out Type? packetType)) return;
        if (!TryReadPacket(packet!, reader, out string error))
        {
            _log($"Received malformed packet: {error} (Ignoring)");
            return;
        }

        if (!_handlers.TryGetValue(packetType!, out Action<PacketFromPeer<ClientPacket>>? handler))
        {
            _log($"No handler registered for client packet {packetType!.Name} (Ignoring)");
            return;
        }

        try
        {
            handler(new PacketFromPeer<ClientPacket> { Packet = packet!, PeerId = peer.ID });
            LogPacketReceived(packetType!, peer.ID, packet!);
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            _logError(exception);
        }
    }

    private bool TryResolvePacket(PacketReader reader, out ClientPacket? packet, out Type? packetType)
    {
        packet = null;
        packetType = null;

        ushort opcode;
        try { opcode = PacketRegistry.ReadOpcodeFromReader(reader); }
        catch (EndOfStreamException exception)
        {
            _log($"Received malformed packet: {exception.Message} (Ignoring)");
            return false;
        }

        if (!PacketRegistry.ClientPacketTypesWire.TryGetValue(opcode, out Type? resolvedType))
        {
            _log($"Received malformed opcode: {opcode} (Ignoring)");
            return false;
        }

        packetType = resolvedType;
        packet = PacketRegistry.ClientPacketInfo[resolvedType].Instance;
        return true;
    }

    private static bool TryReadPacket(ClientPacket packet, PacketReader reader, out string error)
    {
        try { packet.Read(reader); error = string.Empty; return true; }
        catch (EndOfStreamException exception) { error = exception.Message; return false; }
    }

    private void LogPacketReceived(Type packetType, uint peerId, ClientPacket packet)
    {
        ENetOptions? options = _optionsProvider();
        if (options == null || !options.PrintPacketReceived || _ignoredPacketsProvider().Contains(packetType)) return;
        string packetData = options.PrintPacketData ? $"\n{packet.ToFormattedString()}" : string.Empty;
        _log($"Received packet: {packetType.Name} from client {peerId}{packetData}");
    }

    private void LogMalformed(uint peerId, string reason)
    {
        int intervalMs = NormalizePositive(_optionsProvider()?.MalformedFragmentLogIntervalMs ?? DefaultMalformedFragmentLogIntervalMs, DefaultMalformedFragmentLogIntervalMs);
        string key = $"{peerId}:{reason}";
        if (!ShouldLogNow(_malformedFragmentLogTicks, key, intervalMs)) return;
        _log($"Dropped malformed fragment from peer {peerId}. reason={reason}");
    }

    private ushort GetMaxFragmentsPerPacket() => NormalizePositive(_optionsProvider()?.MaxFragmentsPerPacket ?? (ushort)1024, (ushort)1024);
    private static int NormalizePositive(int configured, int fallback) => configured > 0 ? configured : fallback;
    private static ushort NormalizePositive(ushort configured, ushort fallback) => configured > 0 ? configured : fallback;

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
