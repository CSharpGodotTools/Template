using ENet;
using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace __TEMPLATE__.Netcode.Server;

/// <summary>
/// Decodes incoming client packets, reassembles fragments, and dispatches typed handlers.
/// </summary>
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

    /// <summary>
    /// Creates an incoming packet processor for server worker threads.
    /// </summary>
    /// <param name="queues">Queue manager supplying incoming packets.</param>
    /// <param name="peers">Peer store with reassembly state.</param>
    /// <param name="optionsProvider">Callback returning current ENet options.</param>
    /// <param name="ignoredPacketsProvider">Callback returning packet types excluded from logs.</param>
    /// <param name="log">Logger callback for diagnostics.</param>
    /// <param name="logError">Error logger callback for handler failures.</param>
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

    /// <summary>
    /// Registers a typed client-packet handler.
    /// </summary>
    /// <typeparam name="TPacket">Client packet type to handle.</typeparam>
    /// <param name="handler">Handler callback receiving packet and peer metadata.</param>
    public void RegisterHandler<TPacket>(Action<PacketFromPeer<TPacket>> handler) where TPacket : ClientPacket
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handlers[typeof(TPacket)] = peer => handler(new PacketFromPeer<TPacket> { Packet = (TPacket)peer.Packet, PeerId = peer.PeerId });
    }

    /// <summary>
    /// Processes all queued incoming packets for this worker tick.
    /// </summary>
    public void Process()
    {
        while (_queues.TryDequeueIncoming(out IncomingPacket? queued))
        {
            // Ignore null queue entries defensively.
            if (queued == null)
                continue;

            // Copy bytes first so the ENet packet can be disposed immediately.
            byte[] bytes = new byte[queued.Packet.Length];
            queued.Packet.CopyTo(bytes);
            queued.Packet.Dispose();

            // Fragmented payloads need reassembly before packet decoding.
            if (PacketFragmenter.IsFragment(bytes)) { HandleFragment(bytes, queued.Peer); continue; }
            Dispatch(bytes, queued.Peer);
        }
    }

    /// <summary>
    /// Reassembles a fragment packet and dispatches when all fragments are present.
    /// </summary>
    /// <param name="bytes">Raw fragment bytes.</param>
    /// <param name="peer">Peer that sent the fragment.</param>
    private void HandleFragment(byte[] bytes, Peer peer)
    {
        // Reject fragments that cannot provide a complete header.
        if (!PacketFragmenter.TryReadHeader(bytes, out ushort streamId, out ushort fragIndex, out ushort totalFragments))
        {
            LogMalformed(peer.ID, "Fragment header was truncated.");
            return;
        }

        // Reject fragments whose indices violate configured bounds.
        if (!PacketFragmenter.IsValidHeader(fragIndex, totalFragments, GetMaxFragmentsPerPacket(), out string validationError))
        {
            LogMalformed(peer.ID, $"stream={streamId}: {validationError}");
            return;
        }

        System.Collections.Generic.Dictionary<ushort, FragmentBuffer> buffers = _peers.GetOrCreateReassembly(peer.ID);

        // Create reassembly buffer on first fragment for this stream id.
        if (!buffers.TryGetValue(streamId, out FragmentBuffer? buffer))
        {
            buffer = new FragmentBuffer(totalFragments);
            buffers[streamId] = buffer;
        }

        // Reset stream when peer sends conflicting fragment-count metadata.
        else if (buffer.TotalFragments != totalFragments)
        {
            // Reset mismatched streams to avoid combining incompatible fragment sequences.
            buffers.Remove(streamId);
            LogMalformed(peer.ID, $"stream={streamId}: fragment count changed from {buffer.TotalFragments} to {totalFragments}.");
            return;
        }

        byte[] payload = PacketFragmenter.ExtractPayload(bytes);

        // Wait for remaining fragments when stream is still incomplete.
        if (!buffer.Add(fragIndex, payload)) return;
        buffers.Remove(streamId);

        // A complete stream is dispatched as a normal packet payload.
        Dispatch(buffer.Assemble(), peer);
    }

    /// <summary>
    /// Resolves packet type, reads packet payload, and invokes a registered handler.
    /// </summary>
    /// <param name="bytes">Raw packet bytes.</param>
    /// <param name="peer">Peer that sent the packet.</param>
    private void Dispatch(byte[] bytes, Peer peer)
    {
        using PacketReader reader = new(bytes);

        // Stop processing when opcode cannot be resolved to a packet type.
        if (!TryResolvePacket(reader, out ClientPacket? packet, out Type? packetType)) return;

        // Reject malformed payloads that fail packet-specific deserialization.
        if (!TryReadPacket(packet!, reader, out string error))
        {
            _log($"Received malformed packet: {error} (Ignoring)");
            return;
        }

        // Ignore packets without a registered handler callback.
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

    /// <summary>
    /// Resolves packet opcode to a registered client packet instance.
    /// </summary>
    /// <param name="reader">Packet reader positioned at payload start.</param>
    /// <param name="packet">Resolved client packet instance on success.</param>
    /// <param name="packetType">Resolved packet type on success.</param>
    /// <returns><see langword="true"/> when packet opcode resolution succeeds.</returns>
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

        // Reject packets with opcodes not present in the generated registry.
        if (!PacketRegistry.ClientPacketTypesWire.TryGetValue(opcode, out Type? resolvedType))
        {
            _log($"Received malformed opcode: {opcode} (Ignoring)");
            return false;
        }

        packetType = resolvedType;
        packet = PacketRegistry.ClientPacketInfo[resolvedType].Instance;
        return true;
    }

    /// <summary>
    /// Reads a client packet payload from a packet reader.
    /// </summary>
    /// <param name="packet">Client packet instance to populate.</param>
    /// <param name="reader">Packet reader.</param>
    /// <param name="error">Error message when read fails.</param>
    /// <returns><see langword="true"/> when packet payload is valid.</returns>
    private static bool TryReadPacket(ClientPacket packet, PacketReader reader, out string error)
    {
        try { packet.Read(reader); error = string.Empty; return true; }
        catch (EndOfStreamException exception) { error = exception.Message; return false; }
    }

    /// <summary>
    /// Logs packet-received diagnostics when enabled by options.
    /// </summary>
    /// <param name="packetType">Resolved packet type.</param>
    /// <param name="peerId">Sending peer id.</param>
    /// <param name="packet">Decoded packet instance.</param>
    private void LogPacketReceived(Type packetType, uint peerId, ClientPacket packet)
    {
        ENetOptions? options = _optionsProvider();

        // Skip receive logging when disabled or explicitly filtered.
        if (options?.PrintPacketReceived != true || _ignoredPacketsProvider().Contains(packetType)) return;
        string packetData = options.PrintPacketData ? $"\n{packet.ToFormattedString()}" : string.Empty;
        _log($"Received packet: {packetType.Name} from client {peerId}{packetData}");
    }

    /// <summary>
    /// Logs malformed fragment diagnostics with throttling.
    /// </summary>
    /// <param name="peerId">Peer id tied to malformed fragment input.</param>
    /// <param name="reason">Malformed reason text.</param>
    private void LogMalformed(uint peerId, string reason)
    {
        int intervalMs = NormalizePositive(_optionsProvider()?.MalformedFragmentLogIntervalMs ?? DefaultMalformedFragmentLogIntervalMs, DefaultMalformedFragmentLogIntervalMs);
        string key = $"{peerId}:{reason}";

        // Throttle repeated malformed-fragment logs per peer and reason.
        if (!ShouldLogNow(_malformedFragmentLogTicks, key, intervalMs)) return;
        _log($"Dropped malformed fragment from peer {peerId}. reason={reason}");
    }

    /// <summary>
    /// Resolves max fragment count from options with fallback normalization.
    /// </summary>
    /// <returns>Validated max fragment count.</returns>
    private ushort GetMaxFragmentsPerPacket() => NormalizePositive(_optionsProvider()?.MaxFragmentsPerPacket ?? (ushort)1024, (ushort)1024);

    /// <summary>
    /// Normalizes positive integer configuration values.
    /// </summary>
    /// <param name="configured">Configured value.</param>
    /// <param name="fallback">Fallback value.</param>
    /// <returns>Positive value to use.</returns>
    private static int NormalizePositive(int configured, int fallback) => configured > 0 ? configured : fallback;

    /// <summary>
    /// Normalizes positive unsigned short configuration values.
    /// </summary>
    /// <param name="configured">Configured value.</param>
    /// <param name="fallback">Fallback value.</param>
    /// <returns>Positive value to use.</returns>
    private static ushort NormalizePositive(ushort configured, ushort fallback) => configured > 0 ? configured : fallback;

    /// <summary>
    /// Applies interval-based throttling for repeated log keys.
    /// </summary>
    /// <param name="throttleMap">Map of key to last emission timestamp.</param>
    /// <param name="key">Throttle key.</param>
    /// <param name="intervalMs">Minimum interval between emissions.</param>
    /// <returns><see langword="true"/> when a log should be emitted now.</returns>
    private static bool ShouldLogNow(ConcurrentDictionary<string, long> throttleMap, string key, int intervalMs)
    {
        long now = Stopwatch.GetTimestamp();
        long intervalTicks = (long)(intervalMs * (double)Stopwatch.Frequency / 1000.0);

        // Allow logging when key is new or throttle interval has elapsed.
        if (!throttleMap.TryGetValue(key, out long lastLogged) || now - lastLogged >= intervalTicks)
        {
            throttleMap[key] = now;
            return true;
        }

        return false;
    }
}
