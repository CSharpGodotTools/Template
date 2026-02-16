using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// A generic cache manager that stores and retrieves objects based on a key.
/// If the object associated with the key does not exist, it creates and caches it.
/// </summary>
public class CacheManager<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _cache = [];

    /// <summary>
    /// Retrieves the cached object associated with the specified key, or creates and caches it if not present.
    /// </summary>
    public TValue Get(TKey key, Func<TValue> createFunc)
    {
        if (_cache.TryGetValue(key, out TValue value))
            return value;

        value = createFunc();
        _cache[key] = value;

        return value;
    }
}
