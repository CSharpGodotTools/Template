using __TEMPLATE__.Netcode;

namespace Template.Setup.Testing;

/// <summary>
/// Packet containing primitive value fields for serializer round-trip tests.
/// </summary>
public partial class CPacketPrimitives : ClientPacket
{
    /// <summary>
    /// Gets or sets a boolean sample value.
    /// </summary>
    public bool BoolValue { get; set; }

    /// <summary>
    /// Gets or sets a byte sample value.
    /// </summary>
    public byte ByteValue { get; set; }

    /// <summary>
    /// Gets or sets a signed byte sample value.
    /// </summary>
    public sbyte SByteValue { get; set; }

    /// <summary>
    /// Gets or sets a short sample value.
    /// </summary>
    public short ShortValue { get; set; }

    /// <summary>
    /// Gets or sets an unsigned short sample value.
    /// </summary>
    public ushort UShortValue { get; set; }

    /// <summary>
    /// Gets or sets an int sample value.
    /// </summary>
    public int IntValue { get; set; }

    /// <summary>
    /// Gets or sets an unsigned int sample value.
    /// </summary>
    public uint UIntValue { get; set; }

    /// <summary>
    /// Gets or sets a long sample value.
    /// </summary>
    public long LongValue { get; set; }

    /// <summary>
    /// Gets or sets an unsigned long sample value.
    /// </summary>
    public ulong ULongValue { get; set; }

    /// <summary>
    /// Gets or sets a float sample value.
    /// </summary>
    public float FloatValue { get; set; }

    /// <summary>
    /// Gets or sets a double sample value.
    /// </summary>
    public double DoubleValue { get; set; }

    /// <summary>
    /// Gets or sets a decimal sample value.
    /// </summary>
    public decimal DecimalValue { get; set; }

    /// <summary>
    /// Gets or sets a character sample value.
    /// </summary>
    public char CharValue { get; set; }

    /// <summary>
    /// Gets or sets a string sample value.
    /// </summary>
    public string StringValue { get; set; }
}
