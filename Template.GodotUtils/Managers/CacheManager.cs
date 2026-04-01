using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Generic key-based cache that lazily creates values on first access.
/// </summary>
/// <typeparam name="TKey">Cache key type.</typeparam>
/// <typeparam name="TValue">Cached value type.</typeparam>
public class CacheManager<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _cache = [];

    /// <summary>
    /// Returns an existing cached value or creates, stores, and returns a new value for the key.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="createFunc">Factory used when the key has no cached value.</param>
    /// <returns>Cached or newly created value.</returns>
    public TValue Get(TKey key, Func<TValue> createFunc)
    {
        // Fast path for cache hits avoids invoking the factory.
        if (_cache.TryGetValue(key, out TValue? value))
            return value;

        value = createFunc();
        _cache[key] = value;

        return value;
    }
}
