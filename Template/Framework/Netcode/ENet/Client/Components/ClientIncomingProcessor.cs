using ENet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace __TEMPLATE__.Netcode.Client;

/// <summary>
/// Parses incoming ENet packets, reassembles fragments, and queues validated packets for main-thread handling.
/// </summary>
internal sealed class ClientIncomingProcessor
{
    private const int DefaultMalformedFragmentLogIntervalMs = 2000;

    private readonly ClientQueueManager _queues;
    private readonly ConcurrentQueue<PacketData> _mainThreadPackets;
    private readonly Func<ENetOptions?> _optionsProvider;
    private readonly Action<string> _log;
    private readonly Dictionary<ushort, FragmentBuffer> _reassemblyBuffers = [];
    private readonly ConcurrentDictionary<string, long> _malformedFragmentLogTicks = new();

    /// <summary>
    /// Creates a processor responsible for client inbound packet decoding and reassembly.
    /// </summary>
    /// <param name="queues">Queue manager that supplies incoming transport packets.</param>
    /// <param name="mainThreadPackets">Queue that receives decoded packets for gameplay processing.</param>
    /// <param name="optionsProvider">Callback that returns current ENet options.</param>
    /// <param name="log">Logger callback for malformed packet diagnostics.</param>
    public ClientIncomingProcessor(
        ClientQueueManager queues,
        ConcurrentQueue<PacketData> mainThreadPackets,
        Func<ENetOptions?> optionsProvider,
        Action<string> log)
    {
        _queues = queues;
        _mainThreadPackets = mainThreadPackets;
        _optionsProvider = optionsProvider;
        _log = log;
    }

    /// <summary>
    /// Drains incoming packets, handling fragmentation and packet type resolution.
    /// </summary>
    public void Process()
    {
        while (_queues.TryDequeueIncoming(out Packet packet))
        {
            // Copy unmanaged ENet packet bytes immediately so packet can be disposed safely.
            byte[] bytes = new byte[packet.Length];
            packet.CopyTo(bytes);
            packet.Dispose();

            // Fragments require reassembly before they can be decoded as packet payloads.
            if (PacketFragmenter.IsFragment(bytes))
            {
                HandleFragment(bytes);
                continue;
            }

            // Non-fragment packets can be decoded directly and queued for the main thread.
            if (TryCreatePacketData(bytes, out PacketData? packetData) && packetData != null)
                _mainThreadPackets.Enqueue(packetData);
        }
    }

    /// <summary>
    /// Clears all pending fragment reassembly state.
    /// </summary>
    public void ClearReassembly()
    {
        _reassemblyBuffers.Clear();
    }

    /// <summary>
    /// Reassembles a fragment packet and enqueues a complete packet when all parts arrive.
    /// </summary>
    /// <param name="bytes">Raw fragment packet bytes.</param>
    private void HandleFragment(byte[] bytes)
    {
        // Reject fragments that do not contain a full header.
        if (!PacketFragmenter.TryReadHeader(bytes, out ushort streamId, out ushort fragIndex, out ushort totalFragments))
        {
            LogMalformed("Fragment header was truncated.");
            return;
        }

        // Reject fragments with invalid index/count metadata.
        if (!PacketFragmenter.IsValidHeader(fragIndex, totalFragments, GetMaxFragmentsPerPacket(), out string validationError))
        {
            LogMalformed($"stream={streamId}: {validationError}");
            return;
        }

        // Create stream buffer when first fragment arrives.
        if (!_reassemblyBuffers.TryGetValue(streamId, out FragmentBuffer? buffer))
        {
            buffer = new FragmentBuffer(totalFragments);
            _reassemblyBuffers[streamId] = buffer;
        }

        // Reset stream when fragment count changes mid-stream.
        else if (buffer.TotalFragments != totalFragments)
        {
            // Reset the stream buffer when fragment shape changes to avoid mixed assemblies.
            _reassemblyBuffers.Remove(streamId);
            LogMalformed($"stream={streamId}: fragment count changed from {buffer.TotalFragments} to {totalFragments}.");
            return;
        }

        byte[] payload = PacketFragmenter.ExtractPayload(bytes);

        // Wait for remaining fragments until buffer is complete.
        if (!buffer.Add(fragIndex, payload))
            return;

        // Once complete, remove the buffer and process assembled bytes as a normal packet.
        _reassemblyBuffers.Remove(streamId);

        // Queue assembled packet when decoding succeeds.
        if (TryCreatePacketData(buffer.Assemble(), out PacketData? packetData) && packetData != null)
            _mainThreadPackets.Enqueue(packetData);
    }

    /// <summary>
    /// Attempts to decode packet bytes into a dispatchable packet data envelope.
    /// </summary>
    /// <param name="bytes">Raw packet bytes.</param>
    /// <param name="packetData">Decoded packet envelope on success.</param>
    /// <returns><see langword="true"/> when packet type resolution succeeds.</returns>
    private bool TryCreatePacketData(byte[] bytes, out PacketData? packetData)
    {
        packetData = null;
        PacketReader reader = new(bytes);

        // Dispose reader and fail when opcode cannot be resolved.
        if (!TryReadPacketType(reader, out Type? packetType))
        {
            reader.Dispose();
            return false;
        }

        ServerPacket packetInstance = PacketRegistry.ServerPacketInfo[packetType!].Instance;
        packetData = new PacketData
        {
            Type = packetType!,
            PacketReader = reader,
            HandlePacket = packetInstance
        };

        return true;
    }

    /// <summary>
    /// Reads and resolves the packet opcode to a registered server packet type.
    /// </summary>
    /// <param name="reader">Packet reader positioned at packet start.</param>
    /// <param name="packetType">Resolved packet type on success.</param>
    /// <returns><see langword="true"/> when opcode and type are valid.</returns>
    private bool TryReadPacketType(PacketReader reader, out Type? packetType)
    {
        packetType = null;

        ushort opcode;
        try
        {
            opcode = PacketRegistry.ReadOpcodeFromReader(reader);
        }
        catch (EndOfStreamException exception)
        {
            _log($"Received malformed packet: {exception.Message} (Ignoring)");
            return false;
        }

        // Reject packets with opcodes not present in registry.
        if (!PacketRegistry.ServerPacketTypesWire.TryGetValue(opcode, out packetType))
        {
            _log($"Received malformed opcode: {opcode} (Ignoring)");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Logs malformed fragment diagnostics with per-reason throttling.
    /// </summary>
    /// <param name="reason">Human-readable malformed fragment reason.</param>
    private void LogMalformed(string reason)
    {
        int intervalMs = NormalizePositive(_optionsProvider()?.MalformedFragmentLogIntervalMs ?? DefaultMalformedFragmentLogIntervalMs, DefaultMalformedFragmentLogIntervalMs);

        // Throttle repeated malformed-fragment logs by reason.
        if (!ShouldLogNow(_malformedFragmentLogTicks, reason, intervalMs))
            return;

        _log($"Dropped malformed fragment from server. reason={reason}");
    }

    /// <summary>
    /// Gets the configured maximum fragment count per packet with fallback normalization.
    /// </summary>
    /// <returns>Validated max fragment count.</returns>
    private ushort GetMaxFragmentsPerPacket() => NormalizePositive(_optionsProvider()?.MaxFragmentsPerPacket ?? 1024, (ushort)1024);

    /// <summary>
    /// Normalizes integer configuration values to positive numbers.
    /// </summary>
    /// <param name="configured">Configured value.</param>
    /// <param name="fallback">Fallback when configured is invalid.</param>
    /// <returns>Positive value to use.</returns>
    private static int NormalizePositive(int configured, int fallback) => configured > 0 ? configured : fallback;

    /// <summary>
    /// Normalizes unsigned short configuration values to positive numbers.
    /// </summary>
    /// <param name="configured">Configured value.</param>
    /// <param name="fallback">Fallback when configured is invalid.</param>
    /// <returns>Positive value to use.</returns>
    private static ushort NormalizePositive(ushort configured, ushort fallback) => configured > 0 ? configured : fallback;

    /// <summary>
    /// Applies interval-based throttling for repeated log messages keyed by reason.
    /// </summary>
    /// <param name="throttleMap">Map storing last-log timestamps per key.</param>
    /// <param name="key">Throttle key, usually a malformed reason text.</param>
    /// <param name="intervalMs">Minimum interval between log emissions.</param>
    /// <returns><see langword="true"/> when logging should proceed now.</returns>
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
