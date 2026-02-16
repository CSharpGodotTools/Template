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
    public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> me, Dictionary<TKey, TValue> merge)
    {
        foreach (KeyValuePair<TKey, TValue> item in merge)
        {
            me[item.Key] = item.Value;
        }
    }
}
