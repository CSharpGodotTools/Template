using ENet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace __TEMPLATE__.Netcode.Client;

internal sealed class ClientIncomingProcessor
{
    private const int DefaultMalformedFragmentLogIntervalMs = 2000;

    private readonly ClientQueueManager _queues;
    private readonly ConcurrentQueue<PacketData> _mainThreadPackets;
    private readonly Func<ENetOptions?> _optionsProvider;
    private readonly Action<string> _log;
    private readonly Dictionary<ushort, FragmentBuffer> _reassemblyBuffers = [];
    private readonly ConcurrentDictionary<string, long> _malformedFragmentLogTicks = new();

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

    public void Process()
    {
        while (_queues.TryDequeueIncoming(out Packet packet))
        {
            byte[] bytes = new byte[packet.Length];
            packet.CopyTo(bytes);
            packet.Dispose();

            if (PacketFragmenter.IsFragment(bytes))
            {
                HandleFragment(bytes);
                continue;
            }

            if (TryCreatePacketData(bytes, out PacketData? packetData) && packetData != null)
                _mainThreadPackets.Enqueue(packetData);
        }
    }

    public void ClearReassembly()
    {
        _reassemblyBuffers.Clear();
    }

    private void HandleFragment(byte[] bytes)
    {
        if (!PacketFragmenter.TryReadHeader(bytes, out ushort streamId, out ushort fragIndex, out ushort totalFragments))
        {
            LogMalformed("Fragment header was truncated.");
            return;
        }

        if (!PacketFragmenter.IsValidHeader(fragIndex, totalFragments, GetMaxFragmentsPerPacket(), out string validationError))
        {
            LogMalformed($"stream={streamId}: {validationError}");
            return;
        }

        if (!_reassemblyBuffers.TryGetValue(streamId, out FragmentBuffer? buffer))
        {
            buffer = new FragmentBuffer(totalFragments);
            _reassemblyBuffers[streamId] = buffer;
        }
        else if (buffer.TotalFragments != totalFragments)
        {
            _reassemblyBuffers.Remove(streamId);
            LogMalformed($"stream={streamId}: fragment count changed from {buffer.TotalFragments} to {totalFragments}.");
            return;
        }

        byte[] payload = PacketFragmenter.ExtractPayload(bytes);
        if (!buffer.Add(fragIndex, payload))
            return;

        _reassemblyBuffers.Remove(streamId);
        if (TryCreatePacketData(buffer.Assemble(), out PacketData? packetData) && packetData != null)
            _mainThreadPackets.Enqueue(packetData);
    }

    private bool TryCreatePacketData(byte[] bytes, out PacketData? packetData)
    {
        packetData = null;
        PacketReader reader = new(bytes);

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

        if (!PacketRegistry.ServerPacketTypesWire.TryGetValue(opcode, out packetType))
        {
            _log($"Received malformed opcode: {opcode} (Ignoring)");
            return false;
        }

        return true;
    }

    private void LogMalformed(string reason)
    {
        int intervalMs = NormalizePositive(_optionsProvider()?.MalformedFragmentLogIntervalMs ?? DefaultMalformedFragmentLogIntervalMs, DefaultMalformedFragmentLogIntervalMs);
        if (!ShouldLogNow(_malformedFragmentLogTicks, reason, intervalMs))
            return;

        _log($"Dropped malformed fragment from server. reason={reason}");
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
