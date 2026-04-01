using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Extension helpers for <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Returns true when the type is numeric.
    /// </summary>
    /// <param name="type">Type to test.</param>
    /// <returns><see langword="true"/> when type is in known numeric set.</returns>
    public static bool IsNumericType(this Type @type)
    {
        return _numericTypes.Contains(@type);
    }

    /// <summary>
    /// Returns true if the <paramref name="type"/> is a whole number.
    /// </summary>
    /// <param name="type">Type to test.</param>
    /// <returns><see langword="true"/> for integral numeric types.</returns>
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

    /// <summary>
    /// Lookup set used by <see cref="IsNumericType(Type)"/>.
    /// </summary>
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
