#if DEBUG
using System.Collections.Generic;

namespace GodotUtils.Debugging;

/// <summary>
/// Tracks stable ordering for visualized dictionary entries.
/// </summary>
internal interface IVisualDictionaryDisplayOrderTracker
{
    /// <summary>
    /// Gets whether stable ordering is enabled.
    /// </summary>
    bool UseStableOrder { get; }

    /// <summary>
    /// Gets the current ordered list of keys.
    /// </summary>
    IReadOnlyList<object> Keys { get; }

    /// <summary>
    /// Seeds the tracker with initial entries.
    /// </summary>
    /// <param name="entries">Entries to seed.</param>
    void Seed(IEnumerable<(object Key, object Value)> entries);

    /// <summary>
    /// Registers a newly added key.
    /// </summary>
    /// <param name="key">Key to track.</param>
    void TrackAddedKey(object key);

    /// <summary>
    /// Registers a removed key.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    void TrackRemovedKey(object key);

    /// <summary>
    /// Updates tracking when a key is renamed.
    /// </summary>
    /// <param name="previousKey">Original key.</param>
    /// <param name="nextKey">New key.</param>
    void TrackRenamedKey(object previousKey, object nextKey);

    /// <summary>
    /// Reconciles tracked ordering with the latest entries.
    /// </summary>
    /// <param name="entries">Current dictionary entries.</param>
    void Reconcile(IEnumerable<(object Key, object Value)> entries);
}
#endif
