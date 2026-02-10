using Framework.Netcode;

namespace Template.Setup.Testing;

public partial class CPacketPrimitives : ClientPacket
{
    public bool BoolValue { get; set; }
    public byte ByteValue { get; set; }
    public sbyte SByteValue { get; set; }
    public short ShortValue { get; set; }
    public ushort UShortValue { get; set; }
    public int IntValue { get; set; }
    public uint UIntValue { get; set; }
    public long LongValue { get; set; }
    public ulong ULongValue { get; set; }
    public float FloatValue { get; set; }
    public double DoubleValue { get; set; }
    public decimal DecimalValue { get; set; }
    public char CharValue { get; set; }
    public string StringValue { get; set; }
}
