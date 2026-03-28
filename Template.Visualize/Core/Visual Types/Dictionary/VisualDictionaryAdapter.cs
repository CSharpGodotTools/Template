#if DEBUG
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class VisualDictionaryAdapter : IVisualDictionaryAdapter
{
    private readonly Func<IEnumerable<(object Key, object Value)>> _entriesFactory;
    private readonly Func<object, bool> _containsKey;
    private readonly Func<object, object?> _get;
    private readonly Action<object, object?> _set;
    private readonly Action<object> _remove;

    public VisualDictionaryAdapter(
        Func<IEnumerable<(object Key, object Value)>> entriesFactory,
        Func<object, bool> containsKey,
        Func<object, object?> get,
        Action<object, object?> set,
        Action<object> remove)
    {
        _entriesFactory = entriesFactory;
        _containsKey = containsKey;
        _get = get;
        _set = set;
        _remove = remove;
    }

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
