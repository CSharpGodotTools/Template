using System.Collections.Generic;
using System.Reflection;

namespace Template.Setup.Testing;

public static class PacketNestedCollectionsFactory
{
    public static CPacketNestedCollections CreateSample()
    {
        CPacketNestedCollections packet = new();
        SetIfExists(packet, "BoolValues", new[] { true, false, true });
        SetIfExists(packet, "ByteValues", new byte[] { 1, 2, 3 });
        SetIfExists(packet, "SByteValues", new sbyte[] { -1, 0, 1 });
        SetIfExists(packet, "ShortValues", new short[] { -10, 0, 10 });
        SetIfExists(packet, "UShortValues", new ushort[] { 10, 20, 30 });
        SetIfExists(packet, "IntValues", new[] { -100, 0, 100 });
        SetIfExists(packet, "UIntValues", new uint[] { 100u, 200u, 300u });
        SetIfExists(packet, "LongValues", new long[] { -1000L, 0L, 1000L });
        SetIfExists(packet, "ULongValues", new ulong[] { 1000UL, 2000UL, 3000UL });
        SetIfExists(packet, "FloatValues", new[] { 1.25f, 2.5f, 3.75f });
        SetIfExists(packet, "DoubleValues", new[] { 1.5, 2.5, 3.5 });
        SetIfExists(packet, "DecimalValues", new[] { 1.1m, 2.2m, 3.3m });
        SetIfExists(packet, "CharValues", new[] { 'A', 'B', 'C' });
        SetIfExists(packet, "StringValues", new[] { "Alpha", "Beta", "Gamma" });
        SetIfExists(packet, "IntListValues", new List<int> { 1, 2, 3 });
        SetIfExists(packet, "StringListValues", new List<string> { "One", "Two", "Three" });
        SetIfExists(packet, "IntListOfArrays", new List<int[]> { new[] { 1, 2 }, new[] { 3, 4, 5 } });
        SetIfExists(packet, "StringListOfArrays", new List<string[]> { new[] { "Red", "Blue" }, new[] { "Green" } });
        SetIfExists(packet, "IntListOfLists", new List<List<int>> { new() { 10, 20 }, new() { 30, 40, 50 } });
        SetIfExists(packet, "StringListOfLists", new List<List<string>> { new() { "North", "South" }, new() { "East", "West" } });
        SetIfExists(packet, "IntJagged3", new[]
        {
            new[] { new[] { 1, 2 }, new[] { 3 } },
            new[] { new[] { 4, 5, 6 } }
        });
        SetIfExists(packet, "IntListOfJagged", new List<int[][]>
        {
            new[] { new[] { 7, 8 }, new[] { 9 } },
            new[] { new[] { 10 } }
        });
        SetIfExists(packet, "IntListOfListOfArrays", new List<List<int[]>>
        {
            new() { new[] { 11, 12 }, new[] { 13 } },
            new() { new[] { 14, 15, 16 } }
        });
        SetIfExists(packet, "ArrayOfListOfArrays", new[]
        {
            new List<int[]> { new[] { 17, 18 }, new[] { 19 } },
            new List<int[]> { new[] { 20 } }
        });
        SetIfExists(packet, "ArrayOfListOfListOfArrays", new[]
        {
            new List<List<int[]>> { new() { new[] { 21 }, new[] { 22, 23 } } },
            new List<List<int[]>> { new() { new[] { 24, 25 } } }
        });
        SetIfExists(packet, "FloatListOfListOfLists", new List<List<List<float>>>
        {
            new() { new() { 1.1f, 2.2f }, new() { 3.3f } },
            new() { new() { 4.4f } }
        });
        SetIfExists(packet, "DecimalListOfListOfArrays", new List<List<decimal[]>>
        {
            new() { new[] { 1.01m, 2.02m }, new[] { 3.03m } },
            new() { new[] { 4.04m } }
        });

        return packet;
    }

    public static CPacketNestedCollections CreateDeepSample()
    {
        CPacketNestedCollections packet = new();
        SetIfExists(packet, "BoolValues", new[] { false, true });
        SetIfExists(packet, "ByteValues", new byte[] { 10, 20 });
        SetIfExists(packet, "SByteValues", new sbyte[] { -5, 5 });
        SetIfExists(packet, "ShortValues", new short[] { -200, 200 });
        SetIfExists(packet, "UShortValues", new ushort[] { 200, 400 });
        SetIfExists(packet, "IntValues", new[] { -500, 500 });
        SetIfExists(packet, "UIntValues", new uint[] { 500u, 1000u });
        SetIfExists(packet, "LongValues", new long[] { -5000L, 5000L });
        SetIfExists(packet, "ULongValues", new ulong[] { 5000UL, 10000UL });
        SetIfExists(packet, "FloatValues", new[] { 9.5f, 10.5f });
        SetIfExists(packet, "DoubleValues", new[] { 9.9, 10.1 });
        SetIfExists(packet, "DecimalValues", new[] { 9.9m, 10.1m });
        SetIfExists(packet, "CharValues", new[] { 'X', 'Y' });
        SetIfExists(packet, "StringValues", new[] { "Delta", "Epsilon" });
        SetIfExists(packet, "IntListValues", new List<int> { 100, 200 });
        SetIfExists(packet, "StringListValues", new List<string> { "Four", "Five" });
        SetIfExists(packet, "IntListOfArrays", new List<int[]> { new[] { 100, 200, 300 }, new[] { 400 } });
        SetIfExists(packet, "StringListOfArrays", new List<string[]> { new[] { "Cyan" }, new[] { "Magenta", "Yellow" } });
        SetIfExists(packet, "IntListOfLists", new List<List<int>> { new() { 1000, 2000, 3000 } });
        SetIfExists(packet, "StringListOfLists", new List<List<string>> { new() { "Up", "Down", "Left", "Right" } });
        SetIfExists(packet, "IntJagged3", new[]
        {
            new[] { new[] { 31, 32 }, new[] { 33, 34 } }
        });
        SetIfExists(packet, "IntListOfJagged", new List<int[][]>
        {
            new[] { new[] { 35 }, new[] { 36, 37 } }
        });
        SetIfExists(packet, "IntListOfListOfArrays", new List<List<int[]>>
        {
            new() { new[] { 38 }, new[] { 39, 40 } },
            new() { new[] { 41, 42 } }
        });
        SetIfExists(packet, "ArrayOfListOfArrays", new[]
        {
            new List<int[]> { new[] { 43, 44 } }
        });
        SetIfExists(packet, "ArrayOfListOfListOfArrays", new[]
        {
            new List<List<int[]>>
            {
                new() { new[] { 45, 46, 47 } },
                new() { new[] { 48 } }
            }
        });
        SetIfExists(packet, "FloatListOfListOfLists", new List<List<List<float>>>
        {
            new() { new() { 6.6f, 7.7f }, new() { 8.8f, 9.9f } }
        });
        SetIfExists(packet, "DecimalListOfListOfArrays", new List<List<decimal[]>>
        {
            new() { new[] { 5.05m, 6.06m }, new[] { 7.07m, 8.08m } }
        });

        return packet;
    }

    private static void SetIfExists<T>(CPacketNestedCollections packet, string name, T value)
    {
        PropertyInfo property = typeof(CPacketNestedCollections).GetProperty(name);
        if (property == null || !property.CanWrite)
        {
            return;
        }

        property.SetValue(packet, value);
    }
}
