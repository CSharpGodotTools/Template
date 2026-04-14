#if DEBUG
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Tracks stable ordering for dictionary entries in visualization.
/// </summary>
/// <param name="useStableOrder">Whether to keep a stable entry order.</param>
internal sealed class VisualDictionaryDisplayOrderTracker(bool useStableOrder) : IVisualDictionaryDisplayOrderTracker
{
    private readonly List<object> _keys = [];

    /// <summary>
    /// Gets whether stable ordering is enabled.
    /// </summary>
    public bool UseStableOrder { get; } = useStableOrder;

    /// <summary>
    /// Gets the current ordered list of keys.
    /// </summary>
    public IReadOnlyList<object> Keys => _keys;

    /// <summary>
    /// Seeds the tracker with initial entries.
    /// </summary>
    /// <param name="entries">Entries to seed.</param>
    public void Seed(IEnumerable<(object Key, object Value)> entries)
    {
        // Skip seeding when stable ordering is disabled.
        if (!UseStableOrder)
            return;

        _keys.Clear();

        foreach ((object key, _) in entries)
            _keys.Add(key);
    }

    /// <summary>
    /// Registers a newly added key.
    /// </summary>
    /// <param name="key">Key to track.</param>
    public void TrackAddedKey(object key)
    {
        // Only track new keys when stable order is enabled.
        if (!UseStableOrder || _keys.Contains(key))
            return;

        _keys.Add(key);
    }

    /// <summary>
    /// Registers a removed key.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    public void TrackRemovedKey(object key)
    {
        // Skip when stable ordering is disabled.
        if (!UseStableOrder)
            return;

        _keys.Remove(key);
    }

    /// <summary>
    /// Updates tracking when a key is renamed.
    /// </summary>
    /// <param name="previousKey">Original key.</param>
    /// <param name="nextKey">New key.</param>
    public void TrackRenamedKey(object previousKey, object nextKey)
    {
        // Skip when stable ordering is disabled.
        if (!UseStableOrder)
            return;

        int previousIndex = _keys.IndexOf(previousKey);

        // Replace in-place when the old key is tracked.
        if (previousIndex >= 0)
        {
            _keys[previousIndex] = nextKey;
            return;
        }

        TrackAddedKey(nextKey);
    }

    /// <summary>
    /// Reconciles tracked ordering with the latest entries.
    /// </summary>
    /// <param name="entries">Current dictionary entries.</param>
    public void Reconcile(IEnumerable<(object Key, object Value)> entries)
    {
        // Skip when stable ordering is disabled.
        if (!UseStableOrder)
            return;

        List<object> currentKeys = [];

        // Snapshot keys from the current entries.
        foreach ((object key, _) in entries)
            currentKeys.Add(key);

        List<object> removedKeys = [];
        // Identify keys that no longer exist.
        foreach (object key in _keys)
        {
            // Capture keys that were removed from the current dictionary snapshot.
            if (!currentKeys.Contains(key))
                removedKeys.Add(key);
        }

        List<object> addedKeys = [];
        // Identify keys that are newly added.
        foreach (object key in currentKeys)
        {
            // Capture keys that are new compared with tracked ordering state.
            if (!_keys.Contains(key))
                addedKeys.Add(key);
        }

        int replacementCount = Math.Min(removedKeys.Count, addedKeys.Count);

        for (int i = 0; i < replacementCount; i++)
        {
            object removedKey = removedKeys[i];
            object addedKey = addedKeys[i];
            int removedIndex = _keys.IndexOf(removedKey);

            // Replace removed slots with new keys when possible.
            if (removedIndex >= 0)
                _keys[removedIndex] = addedKey;
        }

        for (int i = replacementCount; i < removedKeys.Count; i++)
            _keys.Remove(removedKeys[i]);

        for (int i = replacementCount; i < addedKeys.Count; i++)
            _keys.Add(addedKeys[i]);
    }
}
#endif
