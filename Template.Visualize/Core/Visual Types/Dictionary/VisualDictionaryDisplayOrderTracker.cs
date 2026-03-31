#if DEBUG
using System;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal sealed class VisualDictionaryDisplayOrderTracker(bool useStableOrder) : IVisualDictionaryDisplayOrderTracker
{
    private readonly bool _useStableOrder = useStableOrder;
    private readonly List<object> _keys = [];

    public bool UseStableOrder => _useStableOrder;

    public IReadOnlyList<object> Keys => _keys;

    public void Seed(IEnumerable<(object Key, object Value)> entries)
    {
        if (!_useStableOrder)
        {
            return;
        }

        _keys.Clear();

        foreach ((object key, _) in entries)
        {
            _keys.Add(key);
        }
    }

    public void TrackAddedKey(object key)
    {
        if (!_useStableOrder || _keys.Contains(key))
        {
            return;
        }

        _keys.Add(key);
    }

    public void TrackRemovedKey(object key)
    {
        if (!_useStableOrder)
        {
            return;
        }

        _keys.Remove(key);
    }

    public void TrackRenamedKey(object previousKey, object nextKey)
    {
        if (!_useStableOrder)
        {
            return;
        }

        int previousIndex = _keys.IndexOf(previousKey);

        if (previousIndex >= 0)
        {
            _keys[previousIndex] = nextKey;
            return;
        }

        TrackAddedKey(nextKey);
    }

    public void Reconcile(IEnumerable<(object Key, object Value)> entries)
    {
        if (!_useStableOrder)
        {
            return;
        }

        List<object> currentKeys = [];

        foreach ((object key, _) in entries)
        {
            currentKeys.Add(key);
        }

        List<object> removedKeys = [];
        foreach (object key in _keys)
        {
            if (!currentKeys.Contains(key))
            {
                removedKeys.Add(key);
            }
        }

        List<object> addedKeys = [];
        foreach (object key in currentKeys)
        {
            if (!_keys.Contains(key))
            {
                addedKeys.Add(key);
            }
        }

        int replacementCount = Math.Min(removedKeys.Count, addedKeys.Count);

        for (int i = 0; i < replacementCount; i++)
        {
            object removedKey = removedKeys[i];
            object addedKey = addedKeys[i];
            int removedIndex = _keys.IndexOf(removedKey);

            if (removedIndex >= 0)
            {
                _keys[removedIndex] = addedKey;
            }
        }

        for (int i = replacementCount; i < removedKeys.Count; i++)
        {
            _keys.Remove(removedKeys[i]);
        }

        for (int i = replacementCount; i < addedKeys.Count; i++)
        {
            _keys.Add(addedKeys[i]);
        }
    }
}
#endif
