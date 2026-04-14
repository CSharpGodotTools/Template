using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Extension helpers for dictionaries.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Merges all entries from <paramref name="merge"/> into <paramref name="me"/>, overwriting keys that exist.
    /// </summary>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Dictionary value type.</typeparam>
    /// <param name="me">Destination dictionary.</param>
    /// <param name="merge">Source dictionary to merge from.</param>
    public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> me, Dictionary<TKey, TValue> merge) where TKey : notnull
    {
        foreach (KeyValuePair<TKey, TValue> item in merge)
            me[item.Key] = item.Value;
    }
}
