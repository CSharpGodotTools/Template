#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using static Godot.Control;

namespace GodotUtils.Debugging;

internal sealed class VisualDictionaryControlComponent
{
    private readonly VisualDictionaryTypeInfo _typeInfo;
    private readonly VisualControlContext _context;
    private readonly IVisualDictionaryAdapterFactory _adapterFactory;
    private readonly IVisualDictionaryKeyResolver _keyResolver;
    private readonly IVisualDictionaryDisplayOrderTracker _displayOrderTracker;
    private readonly Action<Node, Action> _cleanupOnTreeExited;
    private readonly VisualDictionaryRowBuilder _rowBuilder;
    private readonly VBoxContainer _dictionaryVBox = new() { SizeFlagsHorizontal = SizeFlags.ShrinkEnd };
    private readonly Button _addButton = new() { Text = "+" };

    private object _dictionaryObject;
    private IVisualDictionaryAdapter _adapter;
    private bool _isEditable = true;

    public VisualDictionaryControlComponent(
        VisualDictionaryTypeInfo typeInfo,
        object dictionaryObject,
        IVisualDictionaryAdapter adapter,
        VisualControlContext context,
        IVisualDictionaryAdapterFactory adapterFactory,
        IVisualDictionaryKeyResolver keyResolver,
        IVisualDictionaryDisplayOrderTracker displayOrderTracker,
        Func<Type, VisualControlContext, VisualControlInfo> createControlForType,
        Action<Node, Action> cleanupOnTreeExited)
    {
        _typeInfo = typeInfo;
        _dictionaryObject = dictionaryObject;
        _adapter = adapter;
        _context = context;
        _adapterFactory = adapterFactory;
        _keyResolver = keyResolver;
        _displayOrderTracker = displayOrderTracker;
        _cleanupOnTreeExited = cleanupOnTreeExited;
        _rowBuilder = new VisualDictionaryRowBuilder(
            _typeInfo,
            _context,
            keyResolver,
            displayOrderTracker,
            createControlForType,
            cleanupOnTreeExited,
            () => _adapter,
            () => _dictionaryObject,
            () => _isEditable);
    }

    public VisualControlInfo Build()
    {
        InitializeRows();

        _addButton.Pressed += OnAddPressed;
        _cleanupOnTreeExited(_addButton, () => _addButton.Pressed -= OnAddPressed);
        _dictionaryVBox.AddChild(_addButton);

        return new VisualControlInfo(new VBoxContainerControl(
            _dictionaryVBox,
            OnExternalValueChanged,
            SetEditable));
    }

    private void InitializeRows()
    {
        List<(object Key, object Value)> entries = new(_adapter.Entries);
        _displayOrderTracker.Seed(entries);

        foreach ((object key, object value) in entries)
        {
            _rowBuilder.AddEntry(_dictionaryVBox, key, value);
        }
    }

    private void OnAddPressed()
    {
        if (!_isEditable)
        {
            return;
        }

        object keyToAdd = _typeInfo.DefaultKey;

        if (_adapter.ContainsKey(keyToAdd)
            && !_keyResolver.TryResolveAddedKey(keyToAdd, _typeInfo.KeyType, _adapter.ContainsKey, out keyToAdd))
        {
            return;
        }

        _adapter.Set(keyToAdd, _typeInfo.DefaultValue);
        _displayOrderTracker.TrackAddedKey(keyToAdd);
        _context.ValueChanged(_dictionaryObject);
        _rowBuilder.AddEntry(_dictionaryVBox, keyToAdd, _typeInfo.DefaultValue);
        _dictionaryVBox.MoveChild(_addButton, _dictionaryVBox.GetChildCount() - 1);
    }

    private void OnExternalValueChanged(object value)
    {
        _dictionaryObject = value ?? Activator.CreateInstance(_typeInfo.DictionaryType)!;
        _adapter = _adapterFactory.Create(_dictionaryObject, _typeInfo.DictionaryType);
        _displayOrderTracker.Reconcile(_adapter.Entries);
        RefreshEntries();
    }

    private void SetEditable(bool editable)
    {
        _isEditable = editable;
        _addButton.Disabled = !editable;
        RefreshEntries();
    }

    private void RefreshEntries()
    {
        if (_displayOrderTracker.UseStableOrder)
        {
            _displayOrderTracker.Reconcile(_adapter.Entries);
        }

        foreach (Node child in _dictionaryVBox.GetChildren())
        {
            if (child == _addButton)
            {
                continue;
            }

            _dictionaryVBox.RemoveChild(child);
            child.QueueFree();
        }

        if (!_displayOrderTracker.UseStableOrder)
        {
            foreach ((object key, object value) in _adapter.Entries)
            {
                _rowBuilder.AddEntry(_dictionaryVBox, key, value);
            }
        }
        else
        {
            foreach (object key in _displayOrderTracker.Keys)
            {
                object? value = _adapter.Get(key);

                if (value == null)
                {
                    continue;
                }

                _rowBuilder.AddEntry(_dictionaryVBox, key, value);
            }
        }

        _dictionaryVBox.MoveChild(_addButton, _dictionaryVBox.GetChildCount() - 1);
    }
}
#endif
