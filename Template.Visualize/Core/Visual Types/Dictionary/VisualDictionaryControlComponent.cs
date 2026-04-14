#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using static Godot.Control;

namespace GodotUtils.Debugging;

/// <summary>
/// Builds and manages dictionary controls for the visualize UI.
/// </summary>
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

    /// <summary>
    /// Initializes a dictionary control component.
    /// </summary>
    /// <param name="typeInfo">Dictionary type metadata.</param>
    /// <param name="dictionaryObject">Dictionary instance.</param>
    /// <param name="adapter">Adapter for dictionary access.</param>
    /// <param name="context">Context for value change notifications.</param>
    /// <param name="adapterFactory">Adapter factory for refreshes.</param>
    /// <param name="keyResolver">Resolver for duplicate keys.</param>
    /// <param name="displayOrderTracker">Display order tracker.</param>
    /// <param name="createControlForType">Factory for child controls.</param>
    /// <param name="cleanupOnTreeExited">Cleanup hook for control teardown.</param>
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

    /// <summary>
    /// Builds the dictionary control and wires events.
    /// </summary>
    /// <returns>Control info for the dictionary UI.</returns>
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

    /// <summary>
    /// Creates initial rows from the current dictionary entries.
    /// </summary>
    private void InitializeRows()
    {
        List<(object Key, object Value)> entries = [.. _adapter.Entries];
        _displayOrderTracker.Seed(entries);

        foreach ((object key, object value) in entries)
            _rowBuilder.AddEntry(_dictionaryVBox, key, value);
    }

    /// <summary>
    /// Handles the add button press to insert a new entry.
    /// </summary>
    private void OnAddPressed()
    {
        // Ignore add requests when editing is disabled.
        if (!_isEditable)
            return;

        object keyToAdd = _typeInfo.DefaultKey;

        // Resolve duplicates or bail when no suitable key exists.
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

    /// <summary>
    /// Handles external value changes for the dictionary.
    /// </summary>
    /// <param name="value">New dictionary value.</param>
    private void OnExternalValueChanged(object value)
    {
        // Ensure a dictionary instance exists for the new value.
        _dictionaryObject = value ?? Activator.CreateInstance(_typeInfo.DictionaryType)!;
        _adapter = _adapterFactory.Create(_dictionaryObject, _typeInfo.DictionaryType);
        _displayOrderTracker.Reconcile(_adapter.Entries);
        RefreshEntries();
    }

    /// <summary>
    /// Updates editability and refreshes rows.
    /// </summary>
    /// <param name="editable">Whether the control is editable.</param>
    private void SetEditable(bool editable)
    {
        _isEditable = editable;
        _addButton.Disabled = !editable;
        RefreshEntries();
    }

    /// <summary>
    /// Rebuilds the entry rows to match the current dictionary state.
    /// </summary>
    private void RefreshEntries()
    {
        // Reconcile stable ordering before rebuilding the UI.
        if (_displayOrderTracker.UseStableOrder)
            _displayOrderTracker.Reconcile(_adapter.Entries);

        foreach (Node child in _dictionaryVBox.GetChildren())
        {
            // Preserve the add button row.
            if (child == _addButton)
                continue;

            _dictionaryVBox.RemoveChild(child);
            child.QueueFree();
        }

        // Rebuild rows either from adapter order or tracked key order.
        if (!_displayOrderTracker.UseStableOrder)
        {
            foreach ((object key, object value) in _adapter.Entries)
                _rowBuilder.AddEntry(_dictionaryVBox, key, value);
        }
        else
        {
            foreach (object key in _displayOrderTracker.Keys)
            {
                object? value = _adapter.Get(key);

                // Skip keys that no longer exist in the dictionary.
                if (value == null)
                    continue;

                _rowBuilder.AddEntry(_dictionaryVBox, key, value);
            }
        }

        _dictionaryVBox.MoveChild(_addButton, _dictionaryVBox.GetChildCount() - 1);
    }
}
#endif
