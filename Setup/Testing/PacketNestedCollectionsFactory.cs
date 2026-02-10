using System.Collections.Generic;

namespace Template.Setup.Testing;

public static class PacketNestedCollectionsFactory
{
    public static CPacketNestedCollections CreateSample()
    {
        CPacketNestedCollections packet = new()
        {
            BoolValues = [true, false, true],
            ByteValues = [1, 2, 3],
            SByteValues = [-1, 0, 1],
            ShortValues = [-10, 0, 10],
            UShortValues = [10, 20, 30],
            IntValues = [-100, 0, 100],
            UIntValues = [100u, 200u, 300u],
            LongValues = [-1000L, 0L, 1000L],
            ULongValues = [1000UL, 2000UL, 3000UL],
            FloatValues = [1.25f, 2.5f, 3.75f],
            DoubleValues = [1.5, 2.5, 3.5],
            DecimalValues = [1.1m, 2.2m, 3.3m],
            CharValues = ['A', 'B', 'C'],
            StringValues = ["Alpha", "Beta", "Gamma"],
            IntListValues = [1, 2, 3],
            StringListValues = ["One", "Two", "Three"],
            IntListOfArrays = [[1, 2], [3, 4, 5]],
            StringListOfArrays = [["Red", "Blue"], ["Green"]],
            IntListOfLists = [[10, 20], [30, 40, 50]],
            StringListOfLists = [["North", "South"], ["East", "West"]],
            IntJagged3 =
            [
                [[1, 2], [3]],
                [[4, 5, 6]]
            ],
            IntListOfJagged =
            [
                [[7, 8], [9]],
                [[10]]
            ],
            IntListOfListOfArrays =
            [
                [[11, 12], [13]],
                [[14, 15, 16]]
            ],
            ArrayOfListOfArrays =
            [
                [[17, 18], [19]],
                [[20]]
            ],
            ArrayOfListOfListOfArrays =
            [
                [[[21], [22, 23]]],
                [[[24, 25]]]
            ],
            FloatListOfListOfLists =
            [
                [[1.1f, 2.2f], [3.3f]],
                [[4.4f]]
            ],
            DecimalListOfListOfArrays =
            [
                [[1.01m, 2.02m], [3.03m]],
                [[4.04m]]
            ]
        };

        return packet;
    }

    public static CPacketNestedCollections CreateDeepSample()
    {
        CPacketNestedCollections packet = new()
        {
            BoolValues = [false, true],
            ByteValues = [10, 20],
            SByteValues = [-5, 5],
            ShortValues = [-200, 200],
            UShortValues = [200, 400],
            IntValues = [-500, 500],
            UIntValues = [500u, 1000u],
            LongValues = [-5000L, 5000L],
            ULongValues = [5000UL, 10000UL],
            FloatValues = [9.5f, 10.5f],
            DoubleValues = [9.9, 10.1],
            DecimalValues = [9.9m, 10.1m],
            CharValues = ['X', 'Y'],
            StringValues = ["Delta", "Epsilon"],
            IntListValues = [100, 200],
            StringListValues = ["Four", "Five"],
            IntListOfArrays = [[100, 200, 300], [400]],
            StringListOfArrays = [["Cyan"], ["Magenta", "Yellow"]],
            IntListOfLists = [[1000, 2000, 3000]],
            StringListOfLists = [["Up", "Down", "Left", "Right"]],
            IntJagged3 =
            [
                [[31, 32], [33, 34]]
            ],
            IntListOfJagged =
            [
                [[35], [36, 37]]
            ],
            IntListOfListOfArrays =
            [
                [[38], [39, 40]],
                [[41, 42]]
            ],
            ArrayOfListOfArrays =
            [
                [[43, 44]]
            ],
            ArrayOfListOfListOfArrays =
            [
                [
                    [[45, 46, 47]],
                    [[48]]
                ]
            ],
            FloatListOfListOfLists =
            [
                [[6.6f, 7.7f], [8.8f, 9.9f]]
            ],
            DecimalListOfListOfArrays =
            [
                [[5.05m, 6.06m], [7.07m, 8.08m]]
            ]
        };

        return packet;
    }
}
