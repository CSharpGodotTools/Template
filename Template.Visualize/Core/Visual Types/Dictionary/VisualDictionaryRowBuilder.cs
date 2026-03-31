#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal sealed class VisualDictionaryRowBuilder(
    VisualDictionaryTypeInfo typeInfo,
    VisualControlContext context,
    IVisualDictionaryKeyResolver keyResolver,
    IVisualDictionaryDisplayOrderTracker displayOrderTracker,
    Func<Type, VisualControlContext, VisualControlInfo> createControlForType,
    Action<Node, Action> cleanupOnTreeExited,
    Func<IVisualDictionaryAdapter> getAdapter,
    Func<object> getDictionaryObject,
    Func<bool> getIsEditable)
{
    private readonly VisualDictionaryTypeInfo _typeInfo = typeInfo;
    private readonly VisualControlContext _context = context;
    private readonly IVisualDictionaryKeyResolver _keyResolver = keyResolver;
    private readonly IVisualDictionaryDisplayOrderTracker _displayOrderTracker = displayOrderTracker;
    private readonly Func<Type, VisualControlContext, VisualControlInfo> _createControlForType = createControlForType;
    private readonly Action<Node, Action> _cleanupOnTreeExited = cleanupOnTreeExited;
    private readonly Func<IVisualDictionaryAdapter> _getAdapter = getAdapter;
    private readonly Func<object> _getDictionaryObject = getDictionaryObject;
    private readonly Func<bool> _getIsEditable = getIsEditable;

    public void AddEntry(VBoxContainer dictionaryVBox, object key, object value)
    {
        object currentKey = key;
        VisualControlInfo? keyControlInfo = null;

        VisualControlInfo valueControl = _createControlForType(_typeInfo.ValueType, new VisualControlContext(value, v =>
        {
            _getAdapter().Set(currentKey, v);
            _context.ValueChanged(_getDictionaryObject());
        }));

        VisualControlInfo keyControl = _createControlForType(_typeInfo.KeyType, new VisualControlContext(currentKey, v =>
        {
            if (v == null)
            {
                keyControlInfo?.VisualControl?.SetValue(currentKey);
                return;
            }

            if (Equals(v, currentKey))
            {
                return;
            }

            IVisualDictionaryAdapter adapter = _getAdapter();
            object resolvedKey = v;

            if (adapter.ContainsKey(resolvedKey)
                && !_keyResolver.TryResolveRenamedKey(currentKey, resolvedKey, _typeInfo.KeyType, adapter.ContainsKey, out resolvedKey))
            {
                keyControlInfo?.VisualControl?.SetValue(currentKey);
                return;
            }

            bool keyWasAutoAdjusted = !Equals(resolvedKey, v);

            if (!_typeInfo.KeyType.IsAssignableFrom(resolvedKey.GetType()))
            {
                keyControlInfo?.VisualControl?.SetValue(currentKey);
                throw new ArgumentException($"[Visualize] Type mismatch: Expected {_typeInfo.KeyType}, got {resolvedKey.GetType()}");
            }

            object previousKey = currentKey;
            object? currentValue = adapter.Get(currentKey);
            adapter.Remove(previousKey);
            adapter.Set(resolvedKey, currentValue);
            _displayOrderTracker.TrackRenamedKey(previousKey, resolvedKey);
            currentKey = resolvedKey;
            _context.ValueChanged(_getDictionaryObject());

            if (keyWasAutoAdjusted)
            {
                keyControlInfo?.VisualControl?.SetValue(currentKey);
            }

            valueControl.VisualControl?.SetValue(_typeInfo.DefaultValue);
        }));

        keyControlInfo = keyControl;

        if (keyControl.VisualControl == null || valueControl.VisualControl == null)
        {
            return;
        }

        bool isEditable = _getIsEditable();
        keyControl.VisualControl.SetValue(currentKey);
        keyControl.VisualControl.SetEditable(isEditable);
        valueControl.VisualControl.SetValue(value);
        valueControl.VisualControl.SetEditable(isEditable);

        Button removeButton = new() { Text = "-", Disabled = !isEditable };
        HBoxContainer row = new();

        void OnRemovePressed()
        {
            if (!_getIsEditable())
            {
                return;
            }

            dictionaryVBox.RemoveChild(row);
            row.QueueFree();
            _getAdapter().Remove(currentKey);
            _displayOrderTracker.TrackRemovedKey(currentKey);
            _context.ValueChanged(_getDictionaryObject());
        }

        removeButton.Pressed += OnRemovePressed;
        _cleanupOnTreeExited(removeButton, () => removeButton.Pressed -= OnRemovePressed);

        row.AddChild(keyControl.VisualControl.Control);
        row.AddChild(valueControl.VisualControl.Control);
        row.AddChild(removeButton);
        dictionaryVBox.AddChild(row);
    }
}
#endif
