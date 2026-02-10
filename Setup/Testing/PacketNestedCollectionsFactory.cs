using System.Collections.Generic;

namespace Template.Setup.Testing;

public static class PacketNestedCollectionsFactory
{
    public static CPacketNestedCollections CreateSample()
    {
        CPacketNestedCollections packet = new()
        {
            BoolValues = new[] { true, false, true },
            ByteValues = new byte[] { 1, 2, 3 },
            SByteValues = new sbyte[] { -1, 0, 1 },
            ShortValues = new short[] { -10, 0, 10 },
            UShortValues = new ushort[] { 10, 20, 30 },
            IntValues = new[] { -100, 0, 100 },
            UIntValues = new uint[] { 100u, 200u, 300u },
            LongValues = new long[] { -1000L, 0L, 1000L },
            ULongValues = new ulong[] { 1000UL, 2000UL, 3000UL },
            FloatValues = new[] { 1.25f, 2.5f, 3.75f },
            DoubleValues = new[] { 1.5, 2.5, 3.5 },
            DecimalValues = new[] { 1.1m, 2.2m, 3.3m },
            CharValues = new[] { 'A', 'B', 'C' },
            StringValues = new[] { "Alpha", "Beta", "Gamma" },
            IntListValues = new List<int> { 1, 2, 3 },
            StringListValues = new List<string> { "One", "Two", "Three" },
            IntListOfArrays = new List<int[]> { new[] { 1, 2 }, new[] { 3, 4, 5 } },
            StringListOfArrays = new List<string[]> { new[] { "Red", "Blue" }, new[] { "Green" } },
            IntListOfLists = new List<List<int>> { new() { 10, 20 }, new() { 30, 40, 50 } },
            StringListOfLists = new List<List<string>> { new() { "North", "South" }, new() { "East", "West" } },
            IntJagged3 = new[]
            {
                new[] { new[] { 1, 2 }, new[] { 3 } },
                new[] { new[] { 4, 5, 6 } }
            },
            IntListOfJagged = new List<int[][]>
            {
                new[] { new[] { 7, 8 }, new[] { 9 } },
                new[] { new[] { 10 } }
            },
            IntListOfListOfArrays = new List<List<int[]>>
            {
                new() { new[] { 11, 12 }, new[] { 13 } },
                new() { new[] { 14, 15, 16 } }
            },
            ArrayOfListOfArrays = new[]
            {
                new List<int[]> { new[] { 17, 18 }, new[] { 19 } },
                new List<int[]> { new[] { 20 } }
            },
            ArrayOfListOfListOfArrays = new[]
            {
                new List<List<int[]>> { new() { new[] { 21 }, new[] { 22, 23 } } },
                new List<List<int[]>> { new() { new[] { 24, 25 } } }
            },
            FloatListOfListOfLists = new List<List<List<float>>>
            {
                new() { new() { 1.1f, 2.2f }, new() { 3.3f } },
                new() { new() { 4.4f } }
            },
            DecimalListOfListOfArrays = new List<List<decimal[]>>
            {
                new() { new[] { 1.01m, 2.02m }, new[] { 3.03m } },
                new() { new[] { 4.04m } }
            }
        };

        return packet;
    }

    public static CPacketNestedCollections CreateDeepSample()
    {
        CPacketNestedCollections packet = new()
        {
            BoolValues = new[] { false, true },
            ByteValues = new byte[] { 10, 20 },
            SByteValues = new sbyte[] { -5, 5 },
            ShortValues = new short[] { -200, 200 },
            UShortValues = new ushort[] { 200, 400 },
            IntValues = new[] { -500, 500 },
            UIntValues = new uint[] { 500u, 1000u },
            LongValues = new long[] { -5000L, 5000L },
            ULongValues = new ulong[] { 5000UL, 10000UL },
            FloatValues = new[] { 9.5f, 10.5f },
            DoubleValues = new[] { 9.9, 10.1 },
            DecimalValues = new[] { 9.9m, 10.1m },
            CharValues = new[] { 'X', 'Y' },
            StringValues = new[] { "Delta", "Epsilon" },
            IntListValues = new List<int> { 100, 200 },
            StringListValues = new List<string> { "Four", "Five" },
            IntListOfArrays = new List<int[]> { new[] { 100, 200, 300 }, new[] { 400 } },
            StringListOfArrays = new List<string[]> { new[] { "Cyan" }, new[] { "Magenta", "Yellow" } },
            IntListOfLists = new List<List<int>> { new() { 1000, 2000, 3000 } },
            StringListOfLists = new List<List<string>> { new() { "Up", "Down", "Left", "Right" } },
            IntJagged3 = new[]
            {
                new[] { new[] { 31, 32 }, new[] { 33, 34 } }
            },
            IntListOfJagged = new List<int[][]>
            {
                new[] { new[] { 35 }, new[] { 36, 37 } }
            },
            IntListOfListOfArrays = new List<List<int[]>>
            {
                new() { new[] { 38 }, new[] { 39, 40 } },
                new() { new[] { 41, 42 } }
            },
            ArrayOfListOfArrays = new[]
            {
                new List<int[]> { new[] { 43, 44 } }
            },
            ArrayOfListOfListOfArrays = new[]
            {
                new List<List<int[]>>
                {
                    new() { new[] { 45, 46, 47 } },
                    new() { new[] { 48 } }
                }
            },
            FloatListOfListOfLists = new List<List<List<float>>>
            {
                new() { new() { 6.6f, 7.7f }, new() { 8.8f, 9.9f } }
            },
            DecimalListOfListOfArrays = new List<List<decimal[]>>
            {
                new() { new[] { 5.05m, 6.06m }, new[] { 7.07m, 8.08m } }
            }
        };

        return packet;
    }
}
