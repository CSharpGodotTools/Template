#if DEBUG
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal interface IVisualDictionaryAdapter
{
    IEnumerable<(object Key, object Value)> Entries { get; }

    bool ContainsKey(object key);

    object? Get(object key);

    void Set(object key, object? value);

    void Remove(object key);
}
#endif
