using __TEMPLATE__.Netcode;
using System.Collections.Generic;

namespace Template.Setup.Testing;

/// <summary>
/// Packet containing nested collection variants used to stress serializer support.
/// </summary>
public partial class CPacketNestedCollections : ClientPacket
{
    /// <summary>
    /// Gets or sets sample boolean array values.
    /// </summary>
    public bool[] BoolValues { get; set; }

    /// <summary>
    /// Gets or sets sample byte array values.
    /// </summary>
    public byte[] ByteValues { get; set; }

    /// <summary>
    /// Gets or sets sample signed byte array values.
    /// </summary>
    public sbyte[] SByteValues { get; set; }

    /// <summary>
    /// Gets or sets sample short array values.
    /// </summary>
    public short[] ShortValues { get; set; }

    /// <summary>
    /// Gets or sets sample unsigned short array values.
    /// </summary>
    public ushort[] UShortValues { get; set; }

    /// <summary>
    /// Gets or sets sample int array values.
    /// </summary>
    public int[] IntValues { get; set; }

    /// <summary>
    /// Gets or sets sample unsigned int array values.
    /// </summary>
    public uint[] UIntValues { get; set; }

    /// <summary>
    /// Gets or sets sample long array values.
    /// </summary>
    public long[] LongValues { get; set; }

    /// <summary>
    /// Gets or sets sample unsigned long array values.
    /// </summary>
    public ulong[] ULongValues { get; set; }

    /// <summary>
    /// Gets or sets sample float array values.
    /// </summary>
    public float[] FloatValues { get; set; }

    /// <summary>
    /// Gets or sets sample double array values.
    /// </summary>
    public double[] DoubleValues { get; set; }

    /// <summary>
    /// Gets or sets sample decimal array values.
    /// </summary>
    public decimal[] DecimalValues { get; set; }

    /// <summary>
    /// Gets or sets sample char array values.
    /// </summary>
    public char[] CharValues { get; set; }

    /// <summary>
    /// Gets or sets sample string array values.
    /// </summary>
    public string[] StringValues { get; set; }

    /// <summary>
    /// Gets or sets a list of integer values.
    /// </summary>
    public List<int> IntListValues { get; set; }

    /// <summary>
    /// Gets or sets a list of string values.
    /// </summary>
    public List<string> StringListValues { get; set; }

    /// <summary>
    /// Gets or sets a list containing integer arrays.
    /// </summary>
    public List<int[]> IntListOfArrays { get; set; }

    /// <summary>
    /// Gets or sets a list containing string arrays.
    /// </summary>
    public List<string[]> StringListOfArrays { get; set; }

    /// <summary>
    /// Gets or sets a list containing integer lists.
    /// </summary>
    public List<List<int>> IntListOfLists { get; set; }

    /// <summary>
    /// Gets or sets a list containing string lists.
    /// </summary>
    public List<List<string>> StringListOfLists { get; set; }

    /// <summary>
    /// Gets or sets a three-dimensional jagged int collection.
    /// </summary>
    public int[][][] IntJagged3 { get; set; }

    /// <summary>
    /// Gets or sets a list of jagged int arrays.
    /// </summary>
    public List<int[][]> IntListOfJagged { get; set; }

    /// <summary>
    /// Gets or sets a list of lists of int arrays.
    /// </summary>
    public List<List<int[]>> IntListOfListOfArrays { get; set; }

    /// <summary>
    /// Gets or sets an array of lists of int arrays.
    /// </summary>
    public List<int[]>[] ArrayOfListOfArrays { get; set; }

    /// <summary>
    /// Gets or sets an array of lists of lists of int arrays.
    /// </summary>
    public List<List<int[]>>[] ArrayOfListOfListOfArrays { get; set; }

    /// <summary>
    /// Gets or sets a list of lists of lists of float values.
    /// </summary>
    public List<List<List<float>>> FloatListOfListOfLists { get; set; }

    /// <summary>
    /// Gets or sets a list of lists containing decimal arrays.
    /// </summary>
    public List<List<decimal[]>> DecimalListOfListOfArrays { get; set; }
}
