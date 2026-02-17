using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace PacketGen.Tests;

[TestFixture]
internal sealed class GeneratorEqualityTests
{
    private static readonly int[] _initialInts = [1, 2, 3];
    private static readonly string[] _initialStrings = ["alpha", "beta"];
    private static readonly byte[] _initialBytesOneTwo = [1, 2];
    private static readonly byte[] _initialBytesThree = [3];
    private static readonly int[] _changedInts = [100, 200, 300];
    private static readonly int[] _changedNestedSingle = [7];
    private static readonly int[] _changedNestedPair = [8, 9];
    private static readonly int[] _nestedOneTwo = [1, 2];
    private static readonly int[] _nestedThree = [3];
    private static readonly int[] _nestedOne = [1];
    private static readonly int[] _nestedTwoThree = [2, 3];

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
        object sameAsLeft = left;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(left, Is.EqualTo(sameAsLeft), "Packet equality should be reflexive.");
            Assert.That(left, Is.EqualTo(right), "Packets with matching structural values should be equal.");
            Assert.That(right, Is.EqualTo(left), "Packet equality should be symmetric.");
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()), "Equal packets must produce equal hash codes.");
            Assert.That(left, Is.Not.Null, "Packet should not be equal to null.");
        }

        PacketReflectionHelper.SetProperty(right, "Ints", (int[])_changedInts.Clone());
        Assert.That(left, Is.Not.EqualTo(right), "Changing nested collection content should break equality.");
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
            Assert.That(left, Is.EqualTo(right), "Nested structural collections should compare equal when values match.");
            Assert.That(leftHashBefore, Is.EqualTo(rightHashBefore), "Equal packets should share the same hash code.");
        }

        PacketReflectionHelper.SetProperty(right, "DictListArray", new Dictionary<string, List<int[]>>
        {
            { "a", new List<int[]> { (int[])_changedNestedSingle.Clone(), (int[])_changedNestedPair.Clone() } }
        });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(left, Is.Not.EqualTo(right), "Changing deep dictionary/list values should break equality.");
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
        PacketReflectionHelper.SetProperty(packet, "Ints", (int[])_initialInts.Clone());
        PacketReflectionHelper.SetProperty(packet, "Strings", (string[])_initialStrings.Clone());
        PacketReflectionHelper.SetProperty(packet, "BytesJagged", new byte[][]
        {
            (byte[])_initialBytesOneTwo.Clone(),
            (byte[])_initialBytesThree.Clone()
        });
        PacketReflectionHelper.SetProperty(packet, "IntList", new List<int> { 1, 2, 3 });
        PacketReflectionHelper.SetProperty(packet, "StringList", new List<string> { "one", "two" });
        PacketReflectionHelper.SetProperty(packet, "StringIntDict", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } });
        PacketReflectionHelper.SetProperty(packet, "IntStringDict", new Dictionary<int, string> { { 1, "x" }, { 2, "y" } });
    }

    private static void AssignNestedValues(object packet)
    {
        PacketReflectionHelper.SetProperty(packet, "IntListOfArrays", new List<int[]>
        {
            (int[])_nestedOneTwo.Clone(),
            (int[])_nestedThree.Clone()
        });
        PacketReflectionHelper.SetProperty(packet, "IntListOfLists", new List<List<int>>
        {
            new() { 1, 2 },
            new() { 3 }
        });
        PacketReflectionHelper.SetProperty(packet, "IntListOfListOfArrays", new List<List<int[]>>
        {
            new() {
                (int[])_nestedOne.Clone(),
                (int[])_nestedTwoThree.Clone()
            }
        });
        PacketReflectionHelper.SetProperty(packet, "ArrayOfListOfArrays", new List<int[]>[]
        {
            [
                (int[])_nestedOne.Clone(),
                (int[])_nestedTwoThree.Clone()
            ]
        });
        PacketReflectionHelper.SetProperty(packet, "IntJagged3", new int[][][]
        {
            [
                (int[])_nestedOneTwo.Clone(),
                (int[])_nestedThree.Clone()
            ]
        });
        PacketReflectionHelper.SetProperty(packet, "DictListArray", new Dictionary<string, List<int[]>>
        {
            {
                "a",
                new List<int[]>
                {
                    (int[])_nestedOne.Clone(),
                    (int[])_nestedTwoThree.Clone()
                }
            }
        });
    }
}
