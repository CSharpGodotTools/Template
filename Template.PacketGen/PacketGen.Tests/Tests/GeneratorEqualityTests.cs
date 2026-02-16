using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace PacketGen.Tests;

[TestFixture]
internal sealed class GeneratorEqualityTests
{
    [Test]
    public void EqualsAndHashCode_StructuralCollectionsAndArrays()
    {
        string testCode = BuildCollectionSource();

        GeneratedAssemblyHarness harness = GeneratedAssemblyHarness.Build<PacketGenerator>(testCode, "CPacketEquality.g.cs");
        Type packetType = harness.GetTypeOrFail("TestPackets.CPacketEquality");

        object left = PacketReflectionHelper.CreatePacketInstance(packetType);
        object right = PacketReflectionHelper.CreatePacketInstance(packetType);

        AssignEqualityValues(left);
        AssignEqualityValues(right);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(object.Equals(left, left), Is.True, "Packet equality should be reflexive.");
            Assert.That(object.Equals(left, right), Is.True, "Packets with matching structural values should be equal.");
            Assert.That(object.Equals(right, left), Is.True, "Packet equality should be symmetric.");
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()), "Equal packets must produce equal hash codes.");
            Assert.That(object.Equals(left, null), Is.False, "Packet should not be equal to null.");
        }

        PacketReflectionHelper.SetProperty(right, "Ints", new[] { 100, 200, 300 });
        Assert.That(object.Equals(left, right), Is.False, "Changing nested collection content should break equality.");
    }

    [Test]
    public void EqualsAndHashCode_NestedCollections()
    {
        string testCode = BuildNestedCollectionSource();

        GeneratedAssemblyHarness harness = GeneratedAssemblyHarness.Build<PacketGenerator>(testCode, "CPacketNestedCollections.g.cs");
        Type packetType = harness.GetTypeOrFail("TestPackets.CPacketNestedCollections");

        object left = PacketReflectionHelper.CreatePacketInstance(packetType);
        object right = PacketReflectionHelper.CreatePacketInstance(packetType);

        AssignNestedValues(left);
        AssignNestedValues(right);

        int leftHashBefore = left.GetHashCode();
        int rightHashBefore = right.GetHashCode();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(object.Equals(left, right), Is.True, "Nested structural collections should compare equal when values match.");
            Assert.That(leftHashBefore, Is.EqualTo(rightHashBefore), "Equal packets should share the same hash code.");
        }

        PacketReflectionHelper.SetProperty(right, "DictListArray", new Dictionary<string, List<int[]>>
        {
            { "a", new List<int[]> { new[] { 7 }, new[] { 8, 9 } } }
        });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(object.Equals(left, right), Is.False, "Changing deep dictionary/list values should break equality.");
            Assert.That(right.GetHashCode(), Is.Not.EqualTo(leftHashBefore), "Hash code should reflect deep value changes.");
        }
    }

    private static string BuildCollectionSource()
    {
        return $$"""
        using System.Collections.Generic;
        using Framework.Netcode;

        namespace TestPackets;

        public partial class CPacketEquality : ClientPacket
        {
            public int Id { get; set; }
            public int[] Ints { get; set; }
            public string[] Strings { get; set; }
            public byte[][] BytesJagged { get; set; }
            public List<int> IntList { get; set; }
            public List<string> StringList { get; set; }
            public Dictionary<string, int> StringIntDict { get; set; }
            public Dictionary<int, string> IntStringDict { get; set; }
        }
        """;
    }

    private static string BuildNestedCollectionSource()
    {
        return $$"""
        using System.Collections.Generic;
        using Framework.Netcode;

        namespace TestPackets;

        public partial class CPacketNestedCollections : ClientPacket
        {
            public List<int[]> IntListOfArrays { get; set; }
            public List<List<int>> IntListOfLists { get; set; }
            public List<List<int[]>> IntListOfListOfArrays { get; set; }
            public List<int[]>[] ArrayOfListOfArrays { get; set; }
            public int[][][] IntJagged3 { get; set; }
            public Dictionary<string, List<int[]>> DictListArray { get; set; }
        }
        """;
    }

    private static void AssignEqualityValues(object packet)
    {
        PacketReflectionHelper.SetProperty(packet, "Id", 42);
        PacketReflectionHelper.SetProperty(packet, "Ints", new[] { 1, 2, 3 });
        PacketReflectionHelper.SetProperty(packet, "Strings", new[] { "alpha", "beta" });
        PacketReflectionHelper.SetProperty(packet, "BytesJagged", new[] { new byte[] { 1, 2 }, new byte[] { 3 } });
        PacketReflectionHelper.SetProperty(packet, "IntList", new List<int> { 1, 2, 3 });
        PacketReflectionHelper.SetProperty(packet, "StringList", new List<string> { "one", "two" });
        PacketReflectionHelper.SetProperty(packet, "StringIntDict", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } });
        PacketReflectionHelper.SetProperty(packet, "IntStringDict", new Dictionary<int, string> { { 1, "x" }, { 2, "y" } });
    }

    private static void AssignNestedValues(object packet)
    {
        PacketReflectionHelper.SetProperty(packet, "IntListOfArrays", new List<int[]>
        {
            new[] { 1, 2 },
            new[] { 3 }
        });
        PacketReflectionHelper.SetProperty(packet, "IntListOfLists", new List<List<int>>
        {
            new List<int> { 1, 2 },
            new List<int> { 3 }
        });
        PacketReflectionHelper.SetProperty(packet, "IntListOfListOfArrays", new List<List<int[]>>
        {
            new List<int[]>
            {
                new[] { 1 },
                new[] { 2, 3 }
            }
        });
        PacketReflectionHelper.SetProperty(packet, "ArrayOfListOfArrays", new List<int[]>[]
        {
            new List<int[]>
            {
                new[] { 1 },
                new[] { 2, 3 }
            }
        });
        PacketReflectionHelper.SetProperty(packet, "IntJagged3", new int[][][]
        {
            new[]
            {
                new[] { 1, 2 },
                new[] { 3 }
            }
        });
        PacketReflectionHelper.SetProperty(packet, "DictListArray", new Dictionary<string, List<int[]>>
        {
            {
                "a",
                new List<int[]>
                {
                    new[] { 1 },
                    new[] { 2, 3 }
                }
            }
        });
    }
}
