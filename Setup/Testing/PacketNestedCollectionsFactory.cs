using System.Collections.Generic;

namespace Template.Setup.Testing;

public static class PacketNestedCollectionsFactory
{
    public static CPacketNestedCollections CreateSample()
    {
        return new CPacketNestedCollections
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

            IntListOfArrays =
            [
                new[] { 1, 2 },
                new[] { 3, 4, 5 }
            ],
            StringListOfArrays =
            [
                new[] { "Red", "Blue" },
                new[] { "Green" }
            ],
            IntListOfLists =
            [
                new List<int> { 10, 20 },
                new List<int> { 30, 40, 50 }
            ],
            StringListOfLists =
            [
                new List<string> { "North", "South" },
                new List<string> { "East", "West" }
            ],

            IntJagged3 =
            [
                new[]
                {
                    new[] { 1, 2 },
                    new[] { 3 }
                },
                new[]
                {
                    new[] { 4, 5, 6 }
                }
            ],
            IntListOfJagged =
            [
                new[]
                {
                    new[] { 7, 8 },
                    new[] { 9 }
                },
                new[]
                {
                    new[] { 10 }
                }
            ],
            IntListOfListOfArrays =
            [
                new List<int[]>
                {
                    new[] { 11, 12 },
                    new[] { 13 }
                },
                new List<int[]>
                {
                    new[] { 14, 15, 16 }
                }
            ],
            ArrayOfListOfArrays =
            [
                new List<int[]>
                {
                    new[] { 17, 18 },
                    new[] { 19 }
                },
                new List<int[]>
                {
                    new[] { 20 }
                }
            ],
            ArrayOfListOfListOfArrays =
            [
                new List<List<int[]>>
                {
                    new List<int[]>
                    {
                        new[] { 21 },
                        new[] { 22, 23 }
                    }
                },
                new List<List<int[]>>
                {
                    new List<int[]>
                    {
                        new[] { 24, 25 }
                    }
                }
            ],
            FloatListOfListOfLists =
            [
                new List<List<float>>
                {
                    new List<float> { 1.1f, 2.2f },
                    new List<float> { 3.3f }
                },
                new List<List<float>>
                {
                    new List<float> { 4.4f }
                }
            ],
            DecimalListOfListOfArrays =
            [
                new List<decimal[]>
                {
                    new[] { 1.01m, 2.02m },
                    new[] { 3.03m }
                },
                new List<decimal[]>
                {
                    new[] { 4.04m }
                }
            ]
        };
    }

    public static CPacketNestedCollections CreateDeepSample()
    {
        return new CPacketNestedCollections
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

            IntListOfArrays =
            [
                new[] { 100, 200, 300 },
                new[] { 400 }
            ],
            StringListOfArrays =
            [
                new[] { "Cyan" },
                new[] { "Magenta", "Yellow" }
            ],
            IntListOfLists =
            [
                new List<int> { 1000, 2000, 3000 }
            ],
            StringListOfLists =
            [
                new List<string> { "Up", "Down", "Left", "Right" }
            ],

            IntJagged3 =
            [
                new[]
                {
                    new[] { 31, 32 },
                    new[] { 33, 34 }
                }
            ],
            IntListOfJagged =
            [
                new[]
                {
                    new[] { 35 },
                    new[] { 36, 37 }
                }
            ],
            IntListOfListOfArrays =
            [
                new List<int[]>
                {
                    new[] { 38 },
                    new[] { 39, 40 }
                },
                new List<int[]>
                {
                    new[] { 41, 42 }
                }
            ],
            ArrayOfListOfArrays =
            [
                new List<int[]>
                {
                    new[] { 43, 44 }
                }
            ],
            ArrayOfListOfListOfArrays =
            [
                new List<List<int[]>>
                {
                    new List<int[]>
                    {
                        new[] { 45, 46, 47 }
                    },
                    new List<int[]>
                    {
                        new[] { 48 }
                    }
                }
            ],
            FloatListOfListOfLists =
            [
                new List<List<float>>
                {
                    new List<float> { 6.6f, 7.7f },
                    new List<float> { 8.8f, 9.9f }
                }
            ],
            DecimalListOfListOfArrays =
            [
                new List<decimal[]>
                {
                    new[] { 5.05m, 6.06m },
                    new[] { 7.07m, 8.08m }
                }
            ]
        };
    }
}
