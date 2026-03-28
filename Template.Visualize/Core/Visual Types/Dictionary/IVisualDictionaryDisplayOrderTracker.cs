#if DEBUG
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal interface IVisualDictionaryDisplayOrderTracker
{
    bool UseStableOrder { get; }

    IReadOnlyList<object> Keys { get; }

    void Seed(IEnumerable<(object Key, object Value)> entries);

    void TrackAddedKey(object key);

    void TrackRemovedKey(object key);

    void TrackRenamedKey(object previousKey, object nextKey);

    void Reconcile(IEnumerable<(object Key, object Value)> entries);
}
#endif
