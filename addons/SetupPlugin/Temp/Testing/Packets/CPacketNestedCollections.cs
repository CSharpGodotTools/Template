using Framework.Netcode;
using System.Collections.Generic;

namespace Template.Setup.Testing;

public partial class CPacketNestedCollections : ClientPacket
{
    public bool[] BoolValues { get; set; }
    public byte[] ByteValues { get; set; }
    public sbyte[] SByteValues { get; set; }
    public short[] ShortValues { get; set; }
    public ushort[] UShortValues { get; set; }
    public int[] IntValues { get; set; }
    public uint[] UIntValues { get; set; }
    public long[] LongValues { get; set; }
    public ulong[] ULongValues { get; set; }
    public float[] FloatValues { get; set; }
    public double[] DoubleValues { get; set; }
    public decimal[] DecimalValues { get; set; }
    public char[] CharValues { get; set; }
    public string[] StringValues { get; set; }

    public List<int> IntListValues { get; set; }
    public List<string> StringListValues { get; set; }

    public List<int[]> IntListOfArrays { get; set; }
    public List<string[]> StringListOfArrays { get; set; }
    public List<List<int>> IntListOfLists { get; set; }
    public List<List<string>> StringListOfLists { get; set; }

    public int[][][] IntJagged3 { get; set; }
    public List<int[][]> IntListOfJagged { get; set; }
    public List<List<int[]>> IntListOfListOfArrays { get; set; }
    public List<int[]>[] ArrayOfListOfArrays { get; set; }
    public List<List<int[]>>[] ArrayOfListOfListOfArrays { get; set; }
    public List<List<List<float>>> FloatListOfListOfLists { get; set; }
    public List<List<decimal[]>> DecimalListOfListOfArrays { get; set; }
}
