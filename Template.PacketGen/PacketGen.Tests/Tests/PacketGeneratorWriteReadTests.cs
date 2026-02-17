using Godot;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PacketGen.Tests;

[TestFixture]
internal sealed class PacketGeneratorWriteReadTests
{
    private static readonly byte[] _bytes1234 = [1, 2, 3, 4];
    private static readonly byte[] _bytes56 = [5, 6];
    private static readonly sbyte[] _sbytesMinus1_2 = [-1, 2];
    private static readonly char[] _charsAb = ['a', 'b'];
    private static readonly string[] _stringsOneTwo = ["one", "two"];
    private static readonly bool[] _boolsTrueFalseTrue = [true, false, true];
    private static readonly short[] _shortsMinus2_3 = [-2, 3];
    private static readonly ushort[] _ushorts1_2 = [1, 2];
    private static readonly int[] _ints123 = [1, 2, 3];
    private static readonly uint[] _uints1_2 = [1u, 2u];
    private static readonly float[] _floats1_1_2_2 = [1.1f, 2.2f];
    private static readonly double[] _doubles1_1_2_2 = [1.1, 2.2];
    private static readonly decimal[] _decimals1_1_2_2 = [1.1m, 2.2m];
    private static readonly long[] _longsMinus1_2 = [-1L, 2L];
    private static readonly ulong[] _ulongs1_2 = [1UL, 2UL];
    private static readonly Vector2[] _vector2Pair = [new Vector2(1f, 1f), new Vector2(2f, 2f)];
    private static readonly Vector3[] _vector3Pair = [new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f)];
    private static readonly byte[] _bytes78 = [7, 8];
    private static readonly byte[] _bytes9 = [9];

    private static readonly int[] _ints12 = [1, 2];
    private static readonly int[] _ints3 = [3];
    private static readonly int[] _ints4 = [4];
    private static readonly int[] _ints1 = [1];
    private static readonly int[] _ints23 = [2, 3];
    private static readonly int[] _ints45 = [4, 5];
    private static readonly string[] _stringsX = ["x"];
    private static readonly string[] _stringsYZ = ["y", "z"];

    [TestCaseSource(nameof(GetRoundTripCases))]
    public void WriteRead_RoundTrip(RoundTripCase roundTripCase)
    {
        GeneratedAssemblyHarness harness = GeneratedAssemblyHarness.Build<PacketGenerator>(roundTripCase.Source, roundTripCase.GeneratedFile);
        Type packetType = harness.GetTypeOrFail(roundTripCase.PacketTypeName);

        object packet = PacketReflectionHelper.CreatePacketInstance(packetType);
        AssignProperties(packet, roundTripCase.Properties);

        object writer = harness.CreateWriter();
        Type readerType = harness.GetTypeOrFail("Framework.Netcode.PacketReader");
        PacketReflectionHelper.AssertHasWriteReadMethods(packetType, writer.GetType(), readerType);

        PacketReflectionHelper.InvokeWrite(packet, writer);

        object reader = harness.CreateReader(GeneratedAssemblyHarness.GetWriterValues(writer));
        object roundTripPacket = PacketReflectionHelper.CreatePacketInstance(packetType);
        PacketReflectionHelper.InvokeRead(roundTripPacket, reader);

        AssertRoundTripValues(roundTripPacket, roundTripCase.Properties);
        AssertGeneratedShape(harness.Result.GeneratedSource);
        AssertNoGeneratorErrors(harness.Result);
    }

    private static IEnumerable<RoundTripCase> GetRoundTripCases()
    {
        yield return new RoundTripCase(
            source: BuildPrimitivesAndArraysSource(),
            generatedFile: "CPacketPrimitivesArrays.g.cs",
            packetTypeName: "TestPackets.CPacketPrimitivesArrays",
            properties: BuildPrimitiveArrayCases());

        yield return new RoundTripCase(
            source: BuildGenericsSource(),
            generatedFile: "CPacketGenerics.g.cs",
            packetTypeName: "TestPackets.CPacketGenerics",
            properties: BuildGenericCases());

        yield return new RoundTripCase(
            source: BuildEmptyCollectionSource(),
            generatedFile: "CPacketEmptyCollections.g.cs",
            packetTypeName: "TestPackets.CPacketEmptyCollections",
            properties: BuildEmptyCollectionCases());
    }

    private static void AssignProperties(object target, IReadOnlyList<PropertyCase> properties)
    {
        foreach (PropertyCase property in properties)
        {
            PacketReflectionHelper.SetProperty(target, property.Name, property.Value);
        }
    }

    private static void AssertRoundTripValues(object actualPacket, IReadOnlyList<PropertyCase> expectedProperties)
    {
        foreach (PropertyCase property in expectedProperties)
        {
            object? actual = PacketReflectionHelper.GetProperty(actualPacket, property.Name);
            DeepAssert.AreEqual(property.Value, actual, property.Name);
        }
    }

    private static void AssertGeneratedShape(string generatedSource)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(generatedSource, Does.Contain("public override void Write(PacketWriter writer)"));
            Assert.That(generatedSource, Does.Contain("public override void Read(PacketReader reader)"));
            Assert.That(generatedSource, Does.Contain("public override bool Equals(object obj)"));
            Assert.That(generatedSource, Does.Contain("public override int GetHashCode()"));
        }
    }

    private static void AssertNoGeneratorErrors(GeneratorTestRunResult result)
    {
        IEnumerable<Diagnostic> errors = result.GeneratorDiagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error);
        Assert.That(errors, Is.Empty, "Generator produced error diagnostics for a supported packet shape.");
    }

    private static string BuildPrimitivesAndArraysSource()
    {
        return $$"""
        using System.Collections.Generic;
        using Godot;
        using Framework.Netcode;

        namespace TestPackets;

        public partial class CPacketPrimitivesArrays : ClientPacket
        {
            public byte ByteValue { get; set; }
            public sbyte SByteValue { get; set; }
            public char CharValue { get; set; }
            public string StringValue { get; set; }
            public bool BoolValue { get; set; }
            public short ShortValue { get; set; }
            public ushort UShortValue { get; set; }
            public int IntValue { get; set; }
            public uint UIntValue { get; set; }
            public float FloatValue { get; set; }
            public double DoubleValue { get; set; }
            public decimal DecimalValue { get; set; }
            public long LongValue { get; set; }
            public ulong ULongValue { get; set; }
            public byte[] BytesValue { get; set; }
            public Vector2 Vector2Value { get; set; }
            public Vector3 Vector3Value { get; set; }

            public byte[] ByteArray { get; set; }
            public sbyte[] SByteArray { get; set; }
            public char[] CharArray { get; set; }
            public string[] StringArray { get; set; }
            public bool[] BoolArray { get; set; }
            public short[] ShortArray { get; set; }
            public ushort[] UShortArray { get; set; }
            public int[] IntArray { get; set; }
            public uint[] UIntArray { get; set; }
            public float[] FloatArray { get; set; }
            public double[] DoubleArray { get; set; }
            public decimal[] DecimalArray { get; set; }
            public long[] LongArray { get; set; }
            public ulong[] ULongArray { get; set; }
            public Vector2[] Vector2Array { get; set; }
            public Vector3[] Vector3Array { get; set; }
            public byte[][] ByteArrayArray { get; set; }
        }
        """;
    }

    private static string BuildGenericsSource()
    {
        return $$"""
        using System.Collections.Generic;
        using Godot;
        using Framework.Netcode;

        namespace TestPackets;

        public partial class CPacketGenerics : ClientPacket
        {
            public List<int> IntList { get; set; }
            public List<string> StringList { get; set; }
            public List<Vector3> Vector3List { get; set; }
            public List<int[]> IntArrayList { get; set; }
            public List<int[][]> ListOfJaggedArrays { get; set; }
            public List<int[]>[] ArrayOfListOfArrays { get; set; }
            public List<List<int[]>>[] ArrayOfListOfListOfArrays { get; set; }

            public Dictionary<int, string> IntStringDict { get; set; }
            public Dictionary<string, int> StringIntDict { get; set; }
            public Dictionary<int, Vector2> IntVector2Dict { get; set; }
            public Dictionary<string, int[]> StringIntArrayDict { get; set; }

            public List<Dictionary<int, string>> ListDict { get; set; }
            public Dictionary<int, List<string[]>> DictListArray { get; set; }
            public List<Dictionary<int, List<int[]>>> ComplexListDict { get; set; }
            public Dictionary<string, List<Dictionary<int, int[]>>> ComplexDict { get; set; }
        }
        """;
    }

    private static string BuildEmptyCollectionSource()
    {
        return $$"""
        using System.Collections.Generic;
        using Framework.Netcode;

        namespace TestPackets;

        public partial class CPacketEmptyCollections : ClientPacket
        {
            public int[] IntArray { get; set; }
            public List<string> Names { get; set; }
            public Dictionary<int, int[]> IndexedValues { get; set; }
            public List<Dictionary<int, List<int[]>>> Nested { get; set; }
        }
        """;
    }

    private static List<PropertyCase> BuildPrimitiveArrayCases()
    {
        return
        [
            new("ByteValue", (byte)42),
            new("SByteValue", (sbyte)-42),
            new("CharValue", 'Z'),
            new("StringValue", "hello"),
            new("BoolValue", true),
            new("ShortValue", (short)-1234),
            new("UShortValue", (ushort)54321),
            new("IntValue", -123456),
            new("UIntValue", 123456u),
            new("FloatValue", 3.14f),
            new("DoubleValue", -2.71828),
            new("DecimalValue", 123.456m),
            new("LongValue", -9876543210L),
            new("ULongValue", 9876543210UL),
            new("BytesValue", CloneArray(_bytes1234)),
            new("Vector2Value", new Vector2(1.5f, -2.5f)),
            new("Vector3Value", new Vector3(1f, 2f, 3f)),

            new("ByteArray", CloneArray(_bytes56)),
            new("SByteArray", CloneArray(_sbytesMinus1_2)),
            new("CharArray", CloneArray(_charsAb)),
            new("StringArray", CloneArray(_stringsOneTwo)),
            new("BoolArray", CloneArray(_boolsTrueFalseTrue)),
            new("ShortArray", CloneArray(_shortsMinus2_3)),
            new("UShortArray", CloneArray(_ushorts1_2)),
            new("IntArray", CloneArray(_ints123)),
            new("UIntArray", CloneArray(_uints1_2)),
            new("FloatArray", CloneArray(_floats1_1_2_2)),
            new("DoubleArray", CloneArray(_doubles1_1_2_2)),
            new("DecimalArray", CloneArray(_decimals1_1_2_2)),
            new("LongArray", CloneArray(_longsMinus1_2)),
            new("ULongArray", CloneArray(_ulongs1_2)),
            new("Vector2Array", CloneArray(_vector2Pair)),
            new("Vector3Array", CloneArray(_vector3Pair)),
            new("ByteArrayArray", new byte[][]
            {
                CloneArray(_bytes78),
                CloneArray(_bytes9)
            })
        ];
    }

    private static List<PropertyCase> BuildGenericCases()
    {
        return
        [
            new("IntList", new List<int> { 1, 2, 3 }),
            new("StringList", new List<string> { "a", "b" }),
            new("Vector3List", new List<Vector3> { new(1f, 0f, 0f), new(0f, 1f, 0f) }),
            new("IntArrayList", new List<int[]> { CloneArray(_ints12), CloneArray(_ints3) }),
            new("ListOfJaggedArrays", new List<int[][]>
            {
                new int[][] { CloneArray(_ints12), CloneArray(_ints3) },
                new int[][] { CloneArray(_ints4) }
            }),
            new("ArrayOfListOfArrays", new List<int[]>[]
            {
                [CloneArray(_ints12), CloneArray(_ints3)],
                [CloneArray(_ints4)]
            }),
            new("ArrayOfListOfListOfArrays", new List<List<int[]>>[]
            {
                [
                    [CloneArray(_ints1), CloneArray(_ints23)]
                ]
            }),

            new("IntStringDict", new Dictionary<int, string> { { 1, "one" }, { 2, "two" } }),
            new("StringIntDict", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } }),
            new("IntVector2Dict", new Dictionary<int, Vector2> { { 1, new Vector2(1f, 2f) }, { 2, new Vector2(3f, 4f) } }),
            new("StringIntArrayDict", new Dictionary<string, int[]>
            {
                { "x", CloneArray(_ints12) },
                { "y", CloneArray(_ints3) }
            }),

            new("ListDict", new List<Dictionary<int, string>>
            {
                new() { { 1, "a" } },
                new() { { 2, "b" } }
            }),

            new("DictListArray", new Dictionary<int, List<string[]>>
            {
                { 1, new List<string[]> { CloneArray(_stringsX), CloneArray(_stringsYZ) } }
            }),

            new("ComplexListDict", new List<Dictionary<int, List<int[]>>>
            {
                new() { { 1, new List<int[]> { CloneArray(_ints12), CloneArray(_ints3) } } },
                new() { { 2, new List<int[]> { CloneArray(_ints45) } } }
            }),

            new("ComplexDict", new Dictionary<string, List<Dictionary<int, int[]>>>
            {
                { "alpha", new List<Dictionary<int, int[]>>
                    {
                        new() { { 1, CloneArray(_ints1) }, { 2, CloneArray(_ints23) } }
                    }
                },
                { "beta", new List<Dictionary<int, int[]>>
                    {
                        new() { { 3, CloneArray(_ints4) } }
                    }
                }
            })
        ];
    }

    private static IReadOnlyList<PropertyCase> BuildEmptyCollectionCases()
    {
        return
        [
            new("IntArray", Array.Empty<int>()),
            new("Names", new List<string>()),
            new("IndexedValues", new Dictionary<int, int[]>()),
            new("Nested", new List<Dictionary<int, List<int[]>>>()),
        ];
    }

    internal sealed class RoundTripCase(string source, string generatedFile, string packetTypeName, IReadOnlyList<PropertyCase> properties)
    {
        public string Source { get; } = source;
        public string GeneratedFile { get; } = generatedFile;
        public string PacketTypeName { get; } = packetTypeName;
        public IReadOnlyList<PropertyCase> Properties { get; } = properties;
    }

    private static T[] CloneArray<T>(T[] source)
    {
        return (T[])source.Clone();
    }
}
