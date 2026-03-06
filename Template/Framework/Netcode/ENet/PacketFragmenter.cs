using System;
using System.Buffers.Binary;

namespace Framework.Netcode;

// Fragment wire format (per ENet packet):
//   [fragment opcode: 2 bytes]  PacketRegistry.FragmentOpcode (ushort LE) — never assigned by PacketGen
//   [streamId: 2 bytes]  identifies the source packet being fragmented
//   [fragIndex: 2 bytes]  0-based index of this fragment
//   [totalFrags: 2 bytes]  total number of fragments in the stream
//   [payload:  up to MaxPayloadPerFragment bytes]
//
// The fragment opcode equals the maximum value of the opcode type chosen via [PacketRegistry]:
//   byte  → 0x00FF (255)   ushort → 0xFFFF (65535)
// PacketGen reserves this value and never assigns it to a user-defined packet type.
//
// Normal wire packets use the same 2-byte ushort opcode, so fragment detection is
// a simple equality check against PacketRegistry.FragmentOpcode.
//
// A packet whose serialized size exceeds GamePacket.MaxSize is split by the sender
// into fragments, each within MaxSize. The receiver reassembles them before dispatch.
internal static class PacketFragmenter
{
    private const int HeaderSize = 8;

    /// <summary>
    /// Maximum application-layer bytes that fit inside a single fragment packet.
    /// </summary>
    public const int MaxPayloadPerFragment = GamePacket.MaxSize - HeaderSize;

    /// <summary>
    /// Returns true when <paramref name="bytes"/> begins with the fragment opcode.
    /// </summary>
    public static bool IsFragment(byte[] bytes) =>
        bytes.Length >= HeaderSize &&
        BinaryPrimitives.ReadUInt16LittleEndian(bytes) == PacketRegistry.FragmentOpcode;

    /// <summary>
    /// Splits <paramref name="data"/> into fragment packets, each ≤ <see cref="GamePacket.MaxSize"/>.
    /// </summary>
    public static byte[][] Fragment(byte[] data, ushort streamId)
    {
        int totalFrags = (int)Math.Ceiling((double)data.Length / MaxPayloadPerFragment);
        byte[][] fragments = new byte[totalFrags][];

        for (int i = 0; i < totalFrags; i++)
        {
            int offset = i * MaxPayloadPerFragment;
            int payloadSize = Math.Min(MaxPayloadPerFragment, data.Length - offset);

            byte[] frag = new byte[HeaderSize + payloadSize];
            BinaryPrimitives.WriteUInt16LittleEndian(frag.AsSpan(0), PacketRegistry.FragmentOpcode);
            BinaryPrimitives.WriteUInt16LittleEndian(frag.AsSpan(2), streamId);
            BinaryPrimitives.WriteUInt16LittleEndian(frag.AsSpan(4), (ushort)i);
            BinaryPrimitives.WriteUInt16LittleEndian(frag.AsSpan(6), (ushort)totalFrags);
            data.AsSpan(offset, payloadSize).CopyTo(frag.AsSpan(HeaderSize));
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

        streamId = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(2));
        fragIndex = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4));
        totalFragments = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(6));
        return true;
    }

    /// <summary>
    /// Returns the application-layer payload slice of a fragment packet (everything after the header).
    /// </summary>
    public static byte[] ExtractPayload(byte[] fragmentBytes)
    {
        int payloadSize = fragmentBytes.Length - HeaderSize;
        byte[] payload = new byte[payloadSize];
        fragmentBytes.AsSpan(HeaderSize, payloadSize).CopyTo(payload);
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
