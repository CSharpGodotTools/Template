using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;

namespace GodotUtils;

/// <summary>
/// Extension helpers for collections.
/// </summary>
public static class ExtensionsCollection
{
    /// <summary>
    /// Iterates over the sequence and invokes <paramref name="action"/> for each element.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> value, Action<T> action)
    {
        foreach (T element in value)
        {
            action(element);
        }
    }
}
