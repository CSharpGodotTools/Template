using Godot;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Stores objects by their instance ID and returns the cached instance if it already exists.
/// </summary>
/// <typeparam name="TValue">The type of Godot object to cache.</typeparam>
public class GodotObjectCache<TValue> where TValue : GodotObject
{
    private readonly Dictionary<ulong, TValue> _cache = [];

    /// <summary>
    /// Returns the cached object if it exists; otherwise adds the object to the cache and returns it.
    /// </summary>
    public TValue Get(TValue obj)
    {
        ulong key = obj.GetInstanceId();

        if (_cache.TryGetValue(key, out TValue value))
            return value;

        _cache.Add(key, obj);
        return obj;
    }

    /// <summary>
    /// Removes an object from the cache.
    /// </summary>
    public void Remove(TValue obj)
    {
        _cache.Remove(obj.GetInstanceId());
    }

    /// <summary>
    /// Clears all objects from the cache.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
