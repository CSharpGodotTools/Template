#if DEBUG
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

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

    public IEnumerable<(object Key, object Value)> Entries => _entriesFactory();

    public bool ContainsKey(object key)
    {
        return _containsKey(key);
    }

    public object? Get(object key)
    {
        return _get(key);
    }

    public void Set(object key, object? value)
    {
        _set(key, value);
    }

    public void Remove(object key)
    {
        _remove(key);
    }
}
#endif
