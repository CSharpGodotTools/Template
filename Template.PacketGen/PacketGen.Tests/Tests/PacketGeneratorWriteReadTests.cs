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

        object reader = harness.CreateReader(harness.GetWriterValues(writer));
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

    private static IReadOnlyList<PropertyCase> BuildPrimitiveArrayCases()
    {
        return new List<PropertyCase>
        {
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
            new("BytesValue", new byte[] { 1, 2, 3, 4 }),
            new("Vector2Value", new Vector2(1.5f, -2.5f)),
            new("Vector3Value", new Vector3(1f, 2f, 3f)),

            new("ByteArray", new byte[] { 5, 6 }),
            new("SByteArray", new sbyte[] { -1, 2 }),
            new("CharArray", new[] { 'a', 'b' }),
            new("StringArray", new[] { "one", "two" }),
            new("BoolArray", new[] { true, false, true }),
            new("ShortArray", new short[] { -2, 3 }),
            new("UShortArray", new ushort[] { 1, 2 }),
            new("IntArray", new[] { 1, 2, 3 }),
            new("UIntArray", new uint[] { 1u, 2u }),
            new("FloatArray", new[] { 1.1f, 2.2f }),
            new("DoubleArray", new[] { 1.1, 2.2 }),
            new("DecimalArray", new[] { 1.1m, 2.2m }),
            new("LongArray", new long[] { -1L, 2L }),
            new("ULongArray", new ulong[] { 1UL, 2UL }),
            new("Vector2Array", new[] { new Vector2(1f, 1f), new Vector2(2f, 2f) }),
            new("Vector3Array", new[] { new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f) }),
            new("ByteArrayArray", new[] { new byte[] { 7, 8 }, new byte[] { 9 } })
        };
    }

    private static IReadOnlyList<PropertyCase> BuildGenericCases()
    {
        return new List<PropertyCase>
        {
            new("IntList", new List<int> { 1, 2, 3 }),
            new("StringList", new List<string> { "a", "b" }),
            new("Vector3List", new List<Vector3> { new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f) }),
            new("IntArrayList", new List<int[]> { new[] { 1, 2 }, new[] { 3 } }),
            new("ListOfJaggedArrays", new List<int[][]>
            {
                new[] { new[] { 1, 2 }, new[] { 3 } },
                new[] { new[] { 4 } }
            }),
            new("ArrayOfListOfArrays", new List<int[]>[]
            {
                new List<int[]> { new[] { 1, 2 }, new[] { 3 } },
                new List<int[]> { new[] { 4 } }
            }),
            new("ArrayOfListOfListOfArrays", new List<List<int[]>>[]
            {
                new List<List<int[]>>
                {
                    new List<int[]> { new[] { 1 }, new[] { 2, 3 } }
                }
            }),

            new("IntStringDict", new Dictionary<int, string> { { 1, "one" }, { 2, "two" } }),
            new("StringIntDict", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } }),
            new("IntVector2Dict", new Dictionary<int, Vector2> { { 1, new Vector2(1f, 2f) }, { 2, new Vector2(3f, 4f) } }),
            new("StringIntArrayDict", new Dictionary<string, int[]> { { "x", new[] { 1, 2 } }, { "y", new[] { 3 } } }),

            new("ListDict", new List<Dictionary<int, string>>
            {
                new Dictionary<int, string> { { 1, "a" } },
                new Dictionary<int, string> { { 2, "b" } }
            }),

            new("DictListArray", new Dictionary<int, List<string[]>>
            {
                { 1, new List<string[]> { new[] { "x" }, new[] { "y", "z" } } }
            }),

            new("ComplexListDict", new List<Dictionary<int, List<int[]>>>
            {
                new Dictionary<int, List<int[]>> { { 1, new List<int[]> { new[] { 1, 2 }, new[] { 3 } } } },
                new Dictionary<int, List<int[]>> { { 2, new List<int[]> { new[] { 4, 5 } } } }
            }),

            new("ComplexDict", new Dictionary<string, List<Dictionary<int, int[]>>>
            {
                { "alpha", new List<Dictionary<int, int[]>>
                    {
                        new Dictionary<int, int[]> { { 1, new[] { 1 } }, { 2, new[] { 2, 3 } } }
                    }
                },
                { "beta", new List<Dictionary<int, int[]>>
                    {
                        new Dictionary<int, int[]> { { 3, new[] { 4 } } }
                    }
                }
            })
        };
    }

    private static IReadOnlyList<PropertyCase> BuildEmptyCollectionCases()
    {
        return new List<PropertyCase>
        {
            new("IntArray", Array.Empty<int>()),
            new("Names", new List<string>()),
            new("IndexedValues", new Dictionary<int, int[]>()),
            new("Nested", new List<Dictionary<int, List<int[]>>>()),
        };
    }

    internal sealed class RoundTripCase(string source, string generatedFile, string packetTypeName, IReadOnlyList<PropertyCase> properties)
    {
        public string Source { get; } = source;
        public string GeneratedFile { get; } = generatedFile;
        public string PacketTypeName { get; } = packetTypeName;
        public IReadOnlyList<PropertyCase> Properties { get; } = properties;
    }
}
