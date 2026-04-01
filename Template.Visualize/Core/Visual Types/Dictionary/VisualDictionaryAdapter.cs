#if DEBUG
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Adapter around dictionary-like operations for visualization.
/// </summary>
/// <param name="entriesFactory">Factory for enumerating entries.</param>
/// <param name="containsKey">Predicate to test key existence.</param>
/// <param name="get">Accessor for retrieving a value by key.</param>
/// <param name="set">Mutator for setting a value by key.</param>
/// <param name="remove">Remover for deleting a key.</param>
internal sealed class VisualDictionaryAdapter(
    Func<IEnumerable<(object Key, object Value)>> entriesFactory,
    Func<object, bool> containsKey,
    Func<object, object?> get,
    Action<object, object?> set,
    Action<object> remove) : IVisualDictionaryAdapter
{
    private readonly Func<IEnumerable<(object Key, object Value)>> _entriesFactory = entriesFactory;
    private readonly Func<object, bool> _containsKey = containsKey;
    private readonly Func<object, object?> _get = get;
    private readonly Action<object, object?> _set = set;
    private readonly Action<object> _remove = remove;

    /// <summary>
    /// Gets current dictionary entries.
    /// </summary>
    public IEnumerable<(object Key, object Value)> Entries => _entriesFactory();

    /// <summary>
    /// Checks whether a key exists.
    /// </summary>
    /// <param name="key">Key to test.</param>
    /// <returns><see langword="true"/> when the key exists.</returns>
    public bool ContainsKey(object key)
    {
        return _containsKey(key);
    }

    /// <summary>
    /// Gets the value for the provided key.
    /// </summary>
    /// <param name="key">Key to read.</param>
    /// <returns>Value for the key, or null when missing.</returns>
    public object? Get(object key)
    {
        return _get(key);
    }

    /// <summary>
    /// Sets the value for the provided key.
    /// </summary>
    /// <param name="key">Key to update.</param>
    /// <param name="value">Value to set.</param>
    public void Set(object key, object? value)
    {
        _set(key, value);
    }

    /// <summary>
    /// Removes the entry for the provided key.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    public void Remove(object key)
    {
        _remove(key);
    }
}
#endif
