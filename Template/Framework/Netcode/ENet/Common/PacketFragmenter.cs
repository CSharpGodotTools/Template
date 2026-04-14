using System;
using System.Buffers.Binary;

namespace __TEMPLATE__.Netcode;

// Fragment wire format (per ENet packet):
//   [fragment opcode: PacketRegistry.OpcodeSize bytes]  PacketRegistry.FragmentOpcode — never assigned by PacketGen
//   [streamId: 2 bytes]  identifies the source packet being fragmented
//   [fragIndex: 2 bytes]  0-based index of this fragment
//   [totalFrags: 2 bytes]  total number of fragments in the stream
//   [payload:  up to MaxPayloadPerFragment bytes]
//
// The opcode field width matches the type configured via [PacketRegistry]:
//   byte → 1 byte (FragmentOpcode = 255)   ushort → 2 bytes (FragmentOpcode = 65535)
// PacketGen reserves the maximum value of the backing type and never assigns it to a user-defined packet type.
//
// A packet whose serialized size exceeds GamePacket.MaxSize is split by the sender
// into fragments, each within MaxSize. The receiver reassembles them before dispatch.
/// <summary>
/// Utilities for fragmenting oversized packet payloads and parsing fragment headers.
/// </summary>
internal static class PacketFragmenter
{
    public const ushort ProtocolMaxFragments = ushort.MaxValue;

    private static int HeaderSize => PacketRegistry.OpcodeSize + 6;

    /// <summary>
    /// Maximum application-layer bytes that fit inside a single fragment packet.
    /// </summary>
    public static int MaxPayloadPerFragment => GamePacket.MaxSize - HeaderSize;

    /// <summary>
    /// Returns true when <paramref name="bytes"/> begins with the fragment opcode.
    /// </summary>
    /// <param name="bytes">Serialized packet bytes to inspect.</param>
    /// <returns><see langword="true"/> when the buffer starts with a valid fragment header opcode.</returns>
    public static bool IsFragment(byte[] bytes) =>
        bytes.Length >= HeaderSize &&
        PacketRegistry.IsFragmentHeader(bytes);

    /// <summary>
    /// Splits <paramref name="data"/> into fragment packets, each ≤ <see cref="GamePacket.MaxSize"/>.
    /// </summary>
    /// <param name="data">Serialized payload bytes to fragment.</param>
    /// <param name="streamId">Stream identifier shared by all generated fragments.</param>
    /// <returns>Fragment packets containing headers plus payload slices.</returns>
    public static byte[][] Fragment(byte[] data, ushort streamId)
    {
        return Fragment(data, streamId, ProtocolMaxFragments);
    }

    /// <summary>
    /// Splits <paramref name="data"/> into fragment packets, enforcing a caller-defined max fragment count.
    /// </summary>
    /// <param name="data">Serialized payload bytes to fragment.</param>
    /// <param name="streamId">Stream identifier shared by all generated fragments.</param>
    /// <param name="maxFragments">Maximum fragment count allowed for this payload.</param>
    /// <returns>Fragment packets containing headers plus payload slices.</returns>
    public static byte[][] Fragment(byte[] data, ushort streamId, ushort maxFragments)
    {
        ArgumentNullException.ThrowIfNull(data);

        // Require a positive configured fragment limit.
        if (maxFragments == 0)
            throw new ArgumentOutOfRangeException(nameof(maxFragments), "maxFragments must be greater than zero.");

        const int opcodeSize = PacketRegistry.OpcodeSize;
        int headerSize = HeaderSize;
        int maxPayload = MaxPayloadPerFragment;
        int totalFrags = (int)Math.Ceiling((double)data.Length / maxPayload);

        // Guard against invalid fragment-count calculation.
        if (totalFrags <= 0)
            throw new InvalidOperationException("Fragmentation requires at least one fragment.");

        // Enforce protocol-level fragment-count ceiling.
        if (totalFrags > ProtocolMaxFragments)
            throw new InvalidOperationException($"Payload requires {totalFrags} fragments which exceeds protocol max {ProtocolMaxFragments}.");

        // Enforce caller-configured fragment-count ceiling.
        if (totalFrags > maxFragments)
            throw new InvalidOperationException($"Payload requires {totalFrags} fragments which exceeds configured max {maxFragments}.");

        byte[][] fragments = new byte[totalFrags][];

        for (int i = 0; i < totalFrags; i++)
        {
            int offset = i * maxPayload;
            int payloadSize = Math.Min(maxPayload, data.Length - offset);

            byte[] frag = new byte[headerSize + payloadSize];
            PacketRegistry.WriteFragmentOpcodeToSpan(frag.AsSpan(0));
            BinaryPrimitives.WriteUInt16LittleEndian(frag.AsSpan(opcodeSize), streamId);
            BinaryPrimitives.WriteUInt16LittleEndian(frag.AsSpan(opcodeSize + 2), (ushort)i);
            BinaryPrimitives.WriteUInt16LittleEndian(frag.AsSpan(opcodeSize + 4), (ushort)totalFrags);
            data.AsSpan(offset, payloadSize).CopyTo(frag.AsSpan(headerSize));
            fragments[i] = frag;
        }

        return fragments;
    }

    /// <summary>
    /// Validates fragment header values extracted from untrusted network data.
    /// </summary>
    /// <param name="fragIndex">Zero-based index of the current fragment.</param>
    /// <param name="totalFragments">Declared total fragment count in the stream.</param>
    /// <param name="maxFragments">Maximum allowed fragments configured by runtime policy.</param>
    /// <param name="reason">Failure reason when validation returns <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when header values are valid for reassembly.</returns>
    public static bool IsValidHeader(ushort fragIndex, ushort totalFragments, ushort maxFragments, out string reason)
    {
        // Reject configurations that disallow all fragments.
        if (maxFragments == 0)
        {
            reason = "Configured max fragments is zero.";
            return false;
        }

        // Reject headers that claim zero fragments.
        if (totalFragments == 0)
        {
            reason = "totalFragments was zero.";
            return false;
        }

        // Reject headers that exceed configured fragment ceiling.
        if (totalFragments > maxFragments)
        {
            reason = $"totalFragments {totalFragments} exceeds configured max {maxFragments}.";
            return false;
        }

        // Reject fragment indices outside the declared fragment range.
        if (fragIndex >= totalFragments)
        {
            reason = $"fragIndex {fragIndex} was outside totalFragments {totalFragments}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    /// <summary>
    /// Parses the fragment header from <paramref name="bytes"/> (must begin with the fragment opcode).
    /// Returns false if the buffer is too small to contain a valid header.
    /// </summary>
    /// <param name="bytes">Serialized fragment bytes containing opcode and header fields.</param>
    /// <param name="streamId">Parsed stream identifier.</param>
    /// <param name="fragIndex">Parsed zero-based fragment index.</param>
    /// <param name="totalFragments">Parsed total fragment count.</param>
    /// <returns><see langword="true"/> when the header was parsed successfully.</returns>
    public static bool TryReadHeader(
        byte[] bytes,
        out ushort streamId,
        out ushort fragIndex,
        out ushort totalFragments)
    {
        // Fail when buffer is null or too small for a full fragment header.
        if (bytes == null || bytes.Length < HeaderSize)
        {
            streamId = 0;
            fragIndex = 0;
            totalFragments = 0;
            return false;
        }

        const int opcodeSize = PacketRegistry.OpcodeSize;
        streamId = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(opcodeSize));
        fragIndex = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(opcodeSize + 2));
        totalFragments = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(opcodeSize + 4));
        return true;
    }

    /// <summary>
    /// Returns the application-layer payload slice of a fragment packet (everything after the header).
    /// </summary>
    /// <param name="fragmentBytes">Serialized fragment packet bytes.</param>
    /// <returns>Payload bytes after removing the fragment header.</returns>
    public static byte[] ExtractPayload(byte[] fragmentBytes)
    {
        int headerSize = HeaderSize;
        int payloadSize = fragmentBytes.Length - headerSize;
        byte[] payload = new byte[payloadSize];
        fragmentBytes.AsSpan(headerSize, payloadSize).CopyTo(payload);
        return payload;
    }
}

/// <summary>
/// Accumulates individual fragments of a fragmented transmission and reassembles them
/// into the original byte sequence once all pieces have arrived.
/// </summary>
/// <param name="totalFragments">Expected number of fragments in the transmission.</param>
internal sealed class FragmentBuffer(ushort totalFragments)
{
    private readonly byte[][] _payloads = new byte[totalFragments][];
    private int _received;

    /// <summary>
    /// Gets expected fragment count for this buffer.
    /// </summary>
    public int TotalFragments => _payloads.Length;

    /// <summary>
    /// Gets whether all expected fragments have been stored.
    /// </summary>
    public bool IsComplete => _received == _payloads.Length;

    /// <summary>
    /// Stores a fragment payload at the given index.
    /// Returns true when this addition completes the buffer.
    /// </summary>
    /// <param name="index">Zero-based fragment index.</param>
    /// <param name="payload">Fragment payload bytes.</param>
    /// <returns><see langword="true"/> when all expected fragments have been received.</returns>
    public bool Add(ushort index, byte[] payload)
    {
        // Reject indices outside allocated fragment slots.
        if (index >= _payloads.Length)
            return false;

        ArgumentNullException.ThrowIfNull(payload);

        // Ignore duplicate fragments while preserving completion state.
        if (_payloads[index] != null)
            return IsComplete;

        _payloads[index] = payload;
        _received++;
        return IsComplete;
    }

    /// <summary>
    /// Concatenates all stored payloads into the original data sequence.
    /// Only call when <see cref="IsComplete"/> is true.
    /// </summary>
    /// <returns>Reassembled payload bytes in original fragment order.</returns>
    public byte[] Assemble()
    {
        int totalSize = 0;
        foreach (byte[] payload in _payloads)
            totalSize += payload.Length;

        byte[] result = new byte[totalSize];
        int offset = 0;

        foreach (byte[] payload in _payloads)
        {
            payload.CopyTo(result, offset);
            offset += payload.Length;
        }

        return result;
    }
}
