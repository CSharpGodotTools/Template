using System;
using System.Buffers.Binary;

namespace Framework.Netcode;

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
internal static class PacketFragmenter
{
    private static int HeaderSize => PacketRegistry.OpcodeSize + 6;

    /// <summary>
    /// Maximum application-layer bytes that fit inside a single fragment packet.
    /// </summary>
    public static int MaxPayloadPerFragment => GamePacket.MaxSize - HeaderSize;

    /// <summary>
    /// Returns true when <paramref name="bytes"/> begins with the fragment opcode.
    /// </summary>
    public static bool IsFragment(byte[] bytes) =>
        bytes.Length >= HeaderSize &&
        PacketRegistry.IsFragmentHeader(bytes);

    /// <summary>
    /// Splits <paramref name="data"/> into fragment packets, each ≤ <see cref="GamePacket.MaxSize"/>.
    /// </summary>
    public static byte[][] Fragment(byte[] data, ushort streamId)
    {
        int opcodeSize = PacketRegistry.OpcodeSize;
        int headerSize = HeaderSize;
        int maxPayload = MaxPayloadPerFragment;
        int totalFrags = (int)Math.Ceiling((double)data.Length / maxPayload);
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
    /// Parses the fragment header from <paramref name="bytes"/> (must begin with the fragment opcode).
    /// Returns false if the buffer is too small to contain a valid header.
    /// </summary>
    public static bool TryReadHeader(
        byte[] bytes,
        out ushort streamId,
        out ushort fragIndex,
        out ushort totalFragments)
    {
        if (bytes == null || bytes.Length < HeaderSize)
        {
            streamId = 0;
            fragIndex = 0;
            totalFragments = 0;
            return false;
        }

        int opcodeSize = PacketRegistry.OpcodeSize;
        streamId = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(opcodeSize));
        fragIndex = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(opcodeSize + 2));
        totalFragments = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(opcodeSize + 4));
        return true;
    }

    /// <summary>
    /// Returns the application-layer payload slice of a fragment packet (everything after the header).
    /// </summary>
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
internal sealed class FragmentBuffer(ushort totalFragments)
{
    private readonly byte[][] _payloads = new byte[totalFragments][];
    private int _received;

    public bool IsComplete => _received == _payloads.Length;

    /// <summary>
    /// Stores a fragment payload at the given index.
    /// Returns true when this addition completes the buffer.
    /// </summary>
    public bool Add(ushort index, byte[] payload)
    {
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
