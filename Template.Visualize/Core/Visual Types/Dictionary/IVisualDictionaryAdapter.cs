#if DEBUG
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Abstraction over dictionary-like storage for visualization controls.
/// </summary>
internal interface IVisualDictionaryAdapter
{
    /// <summary>
    /// Gets current dictionary entries.
    /// </summary>
    IEnumerable<(object Key, object Value)> Entries { get; }

    /// <summary>
    /// Checks whether a key exists in the dictionary.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns><see langword="true"/> when the key exists.</returns>
    bool ContainsKey(object key);

    /// <summary>
    /// Gets the value for the provided key.
    /// </summary>
    /// <param name="key">Key to read.</param>
    /// <returns>Value for the key, or null when missing.</returns>
    object? Get(object key);

    /// <summary>
    /// Sets the value for the provided key.
    /// </summary>
    /// <param name="key">Key to update.</param>
    /// <param name="value">Value to set.</param>
    void Set(object key, object? value);

    /// <summary>
    /// Removes the entry for the provided key.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    void Remove(object key);
}
#endif
