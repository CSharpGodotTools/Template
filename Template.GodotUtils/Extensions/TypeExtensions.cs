using System.Collections.Generic;
using System;

namespace GodotUtils;

/// <summary>
/// Extension helpers for <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Returns true when the type is numeric.
    /// </summary>
    public static bool IsNumericType(this Type @type)
    {
        return _numericTypes.Contains(@type);
    }

    /// <summary>
    /// Returns true if the <paramref name="type"/> is a whole number.
    /// </summary>
    public static bool IsWholeNumber(this Type type)
    {
        return
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(short) ||
            type == typeof(byte) ||
            type == typeof(sbyte) ||
            type == typeof(uint) ||
            type == typeof(ulong) ||
            type == typeof(ushort);
    }

    private static readonly HashSet<Type> _numericTypes =
    [
        typeof(int),
        typeof(float),
        typeof(double),
        typeof(long),
        typeof(short),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(decimal),
        typeof(byte),
        typeof(sbyte)
    ];
}
