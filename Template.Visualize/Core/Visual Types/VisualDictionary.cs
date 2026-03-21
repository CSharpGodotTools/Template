#if DEBUG
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static Godot.Control;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualDictionary(Type type, VisualControlContext context)
    {
        VBoxContainer dictionaryVBox = new() { SizeFlagsHorizontal = SizeFlags.ShrinkEnd };
        Button addButton = new() { Text = "+" };
        bool isEditable = true;

        Type[] genericArguments = type.GetGenericArguments();
        Type keyType = genericArguments[0];
        Type valueType = genericArguments[1];

        object defaultKey = VisualMethods.CreateDefaultValue(keyType);
        object defaultValue = VisualMethods.CreateDefaultValue(valueType);

        // Keep the runtime dictionary instance and an adapter so CLR and Godot dictionaries share one UI path.
        object dictionaryObject = context.InitialValue ?? Activator.CreateInstance(type)!;
        DictionaryAdapter adapter = CreateDictionaryAdapter(dictionaryObject, type, keyType);
        // Tracks readonly row placement so key renames can keep their original position.
        List<object> displayOrder = [];

        foreach ((object key, object value) in adapter.Entries)
        {
            displayOrder.Add(key);
            AddEntry(key, value);
        }

        void OnAddPressed()
        {
            if (!isEditable)
            {
                return;
            }

            if (adapter.ContainsKey(defaultKey))
            {
                return;
            }

            adapter.Set(defaultKey, defaultValue);
            context.ValueChanged(dictionaryObject);
            AddEntry(defaultKey, defaultValue);
            dictionaryVBox.MoveChild(addButton, dictionaryVBox.GetChildCount() - 1);
        }

        addButton.Pressed += OnAddPressed;
        CleanupOnTreeExited(addButton, () => addButton.Pressed -= OnAddPressed);
        dictionaryVBox.AddChild(addButton);

        return new VisualControlInfo(new VBoxContainerControl(
            dictionaryVBox,
            value =>
            {
                // Readonly polling can replace the bound instance; rebuild adapters from the latest object.
                dictionaryObject = value;
                adapter = CreateDictionaryAdapter(dictionaryObject, type, keyType);
                ReconcileDisplayOrder();
                RefreshEntries();
            },
            editable =>
            {
                isEditable = editable;
                addButton.Disabled = !editable;
                RefreshEntries();
            }));

        void AddEntry(object key, object value)
        {
            object currentKey = key;
            VisualControlInfo? keyControlInfo = null;

            VisualControlInfo valueControl = CreateControlForType(valueType, null, new VisualControlContext(value, v =>
            {
                adapter.Set(currentKey, v);
                context.ValueChanged(dictionaryObject);
            }));

            VisualControlInfo keyControl = CreateControlForType(keyType, null, new VisualControlContext(currentKey, v =>
            {
                if (v == null)
                {
                    keyControlInfo?.VisualControl?.SetValue(currentKey);
                    return;
                }

                if (Equals(v, currentKey))
                {
                    keyControlInfo?.VisualControl?.SetValue(currentKey);
                    return;
                }

                object resolvedKey = v;

                // If a key already exists, resolve to the nearest free numeric/enum key.
                if (adapter.ContainsKey(resolvedKey) && !TryGetNextAvailableKey(currentKey, resolvedKey, keyType, adapter.ContainsKey, out resolvedKey))
                {
                    keyControlInfo?.VisualControl?.SetValue(currentKey);
                    return;
                }

                if (resolvedKey.GetType() != keyType)
                {
                    keyControlInfo?.VisualControl?.SetValue(currentKey);
                    throw new ArgumentException($"[Visualize] Type mismatch: Expected {keyType}, got {resolvedKey.GetType()}");
                }

                object? currentValue = adapter.Get(currentKey);
                adapter.Remove(currentKey);
                adapter.Set(resolvedKey, currentValue);
                currentKey = resolvedKey;
                context.ValueChanged(dictionaryObject);
                keyControlInfo?.VisualControl?.SetValue(currentKey);
                valueControl.VisualControl?.SetValue(defaultValue);
            }));

            keyControlInfo = keyControl;

            if (keyControl.VisualControl == null || valueControl.VisualControl == null)
            {
                return;
            }

            keyControl.VisualControl.SetValue(currentKey);
            keyControl.VisualControl.SetEditable(isEditable);
            valueControl.VisualControl.SetValue(value);
            valueControl.VisualControl.SetEditable(isEditable);

            Button removeKeyEntryButton = new() { Text = "-", Disabled = !isEditable };
            HBoxContainer hbox = new();

            void OnRemovePressed()
            {
                if (!isEditable)
                {
                    return;
                }

                dictionaryVBox.RemoveChild(hbox);
                hbox.QueueFree();
                adapter.Remove(currentKey);
                context.ValueChanged(dictionaryObject);
            }

            removeKeyEntryButton.Pressed += OnRemovePressed;
            CleanupOnTreeExited(removeKeyEntryButton, () => removeKeyEntryButton.Pressed -= OnRemovePressed);

            hbox.AddChild(keyControl.VisualControl.Control);
            hbox.AddChild(valueControl.VisualControl.Control);
            hbox.AddChild(removeKeyEntryButton);
            dictionaryVBox.AddChild(hbox);
        }

        void RefreshEntries()
        {
            ReconcileDisplayOrder();

            foreach (Node child in dictionaryVBox.GetChildren())
            {
                if (child == addButton)
                {
                    continue;
                }

                dictionaryVBox.RemoveChild(child);
                child.QueueFree();
            }

            foreach (object key in displayOrder)
            {
                object? value = adapter.Get(key);

                if (value == null)
                {
                    continue;
                }

                AddEntry(key, value);
            }

            dictionaryVBox.MoveChild(addButton, dictionaryVBox.GetChildCount() - 1);
        }

        void ReconcileDisplayOrder()
        {
            List<object> currentKeys = [];

            foreach ((object key, _) in adapter.Entries)
            {
                currentKeys.Add(key);
            }

            List<object> removedKeys = [];
            foreach (object key in displayOrder)
            {
                if (!currentKeys.Contains(key))
                {
                    removedKeys.Add(key);
                }
            }

            List<object> addedKeys = [];
            foreach (object key in currentKeys)
            {
                if (!displayOrder.Contains(key))
                {
                    addedKeys.Add(key);
                }
            }

            int replacementCount = Math.Min(removedKeys.Count, addedKeys.Count);
            // Replace removed keys in-place first so key renames preserve visible order.
            for (int i = 0; i < replacementCount; i++)
            {
                object removedKey = removedKeys[i];
                object addedKey = addedKeys[i];
                int removedIndex = displayOrder.IndexOf(removedKey);

                if (removedIndex >= 0)
                {
                    displayOrder[removedIndex] = addedKey;
                }
            }

            for (int i = replacementCount; i < removedKeys.Count; i++)
            {
                displayOrder.Remove(removedKeys[i]);
            }

            for (int i = replacementCount; i < addedKeys.Count; i++)
            {
                displayOrder.Add(addedKeys[i]);
            }
        }
    }

    /// <summary>
    /// Builds a common dictionary adapter for both CLR and Godot generic dictionary implementations.
    /// </summary>
    private static DictionaryAdapter CreateDictionaryAdapter(object dictionaryObject, Type dictionaryType, Type keyType)
    {
        // CLR dictionaries implement IDictionary directly.
        if (dictionaryObject is IDictionary dictionary)
        {
            return new DictionaryAdapter(
                () => EnumerateDictionaryEntries(dictionary),
                key => dictionary.Contains(key),
                key => dictionary[key],
                (key, value) => dictionary[key] = value,
                key => dictionary.Remove(key));
        }

        PropertyInfo? indexerProperty = dictionaryType.GetProperty("Item");
        MethodInfo? containsKeyMethod = dictionaryType.GetMethod("ContainsKey", [keyType]);
        MethodInfo? removeMethod = dictionaryType.GetMethod("Remove", [keyType]);

        // Godot generic dictionaries are adapted through reflected members.
        if (indexerProperty == null || containsKeyMethod == null || removeMethod == null)
        {
            return new DictionaryAdapter(() => [], _ => false, _ => null, (_, _) => { }, _ => { });
        }

        return new DictionaryAdapter(
            () => EnumerateObjectDictionaryEntries(dictionaryObject),
            key => (bool)containsKeyMethod.Invoke(dictionaryObject, [key])!,
            key => indexerProperty.GetValue(dictionaryObject, [key]),
            (key, value) => indexerProperty.SetValue(dictionaryObject, value, [key]),
            key => removeMethod.Invoke(dictionaryObject, [key]));
    }

    /// <summary>
    /// Enumerates key/value pairs from an <see cref="IDictionary"/>.
    /// </summary>
    private static IEnumerable<(object Key, object Value)> EnumerateDictionaryEntries(IDictionary dictionary)
    {
        foreach (DictionaryEntry entry in dictionary)
        {
            yield return (entry.Key, entry.Value!);
        }
    }

    /// <summary>
    /// Enumerates key/value pairs from dictionary-like objects that expose an enumerable of entries.
    /// </summary>
    private static IEnumerable<(object Key, object Value)> EnumerateObjectDictionaryEntries(object dictionaryObject)
    {
        if (dictionaryObject is not IEnumerable enumerable)
        {
            yield break;
        }

        foreach (object? entry in enumerable)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry is DictionaryEntry dictionaryEntry)
            {
                yield return (dictionaryEntry.Key, dictionaryEntry.Value!);
                continue;
            }

            Type entryType = entry.GetType();
            PropertyInfo? keyProperty = entryType.GetProperty("Key");
            PropertyInfo? valueProperty = entryType.GetProperty("Value");

            if (keyProperty == null || valueProperty == null)
            {
                continue;
            }

            object? key = keyProperty.GetValue(entry);
            object? value = valueProperty.GetValue(entry);

            if (key == null || value == null)
            {
                continue;
            }

            yield return (key, value);
        }
    }

    /// <summary>
    /// Resolves duplicate numeric or enum keys by walking in the user's edit direction until a free key is found.
    /// </summary>
    private static bool TryGetNextAvailableKey(object currentKey, object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey)
    {
        resolvedKey = duplicateKey;

        long currentNumeric;
        long duplicateNumeric;

        try
        {
            currentNumeric = Convert.ToInt64(currentKey);
            duplicateNumeric = Convert.ToInt64(duplicateKey);
        }
        catch (Exception)
        {
            return false;
        }

        long step = duplicateNumeric > currentNumeric ? 1 : -1;

        // Enum keys are treated as their underlying numeric values.
        if (keyType.IsEnum)
        {
            long candidate = duplicateNumeric;

            do
            {
                candidate += step;
                resolvedKey = Enum.ToObject(keyType, candidate);
            }
            while (containsKey(resolvedKey));

            return true;
        }

        if (!keyType.IsNumericType())
        {
            return false;
        }

        // Numeric keys walk in the user's edit direction until a free key is found.
        long numericCandidate = duplicateNumeric;

        while (true)
        {
            numericCandidate += step;

            try
            {
                object nextKey = Convert.ChangeType(numericCandidate, keyType)!;

                if (containsKey(nextKey))
                {
                    continue;
                }

                resolvedKey = nextKey;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    private readonly record struct DictionaryAdapter(
        Func<IEnumerable<(object Key, object Value)>> EntriesFactory,
        Func<object, bool> ContainsKey,
        Func<object, object?> Get,
        Action<object, object?> Set,
        Action<object> Remove)
    {
        public IEnumerable<(object Key, object Value)> Entries => EntriesFactory();
    }
}
#endif
