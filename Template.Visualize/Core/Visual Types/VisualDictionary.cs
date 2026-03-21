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
        bool useStableDisplayOrder = genericArguments.Length == 2;
        Type keyType = genericArguments.Length == 2 ? genericArguments[0] : typeof(object);
        Type valueType = genericArguments.Length == 2 ? genericArguments[1] : typeof(object);

        object defaultKey = keyType == typeof(object) ? "Key" : VisualMethods.CreateDefaultValue(keyType);
        object defaultValue = valueType == typeof(object) ? string.Empty : VisualMethods.CreateDefaultValue(valueType);

        // Keep the runtime dictionary instance and an adapter so CLR and Godot dictionaries share one UI path.
        object dictionaryObject = context.InitialValue ?? Activator.CreateInstance(type)!;
        DictionaryAdapter adapter = CreateDictionaryAdapter(dictionaryObject, type, keyType);
        // Tracks readonly row placement so key renames can keep their original position.
        List<object> displayOrder = [];

        foreach ((object key, object value) in adapter.Entries)
        {
            if (useStableDisplayOrder)
            {
                displayOrder.Add(key);
            }

            AddEntry(key, value);
        }

        void OnAddPressed()
        {
            if (!isEditable)
            {
                return;
            }

            object keyToAdd = defaultKey;

            if (adapter.ContainsKey(keyToAdd) && !TryGetNextAvailableAddKey(keyToAdd, keyType, adapter.ContainsKey, out keyToAdd))
            {
                return;
            }

            adapter.Set(keyToAdd, defaultValue);
            context.ValueChanged(dictionaryObject);
            AddEntry(keyToAdd, defaultValue);
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
                if (useStableDisplayOrder)
                {
                    ReconcileDisplayOrder();
                }

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
                    return;
                }

                object resolvedKey = v;
                bool keyWasAutoAdjusted = false;

                // If a key already exists, resolve to the nearest free numeric/enum key.
                if (adapter.ContainsKey(resolvedKey) && !TryGetNextAvailableKey(currentKey, resolvedKey, keyType, adapter.ContainsKey, out resolvedKey))
                {
                    keyControlInfo?.VisualControl?.SetValue(currentKey);
                    return;
                }

                keyWasAutoAdjusted = !Equals(resolvedKey, v);

                if (!keyType.IsAssignableFrom(resolvedKey.GetType()))
                {
                    keyControlInfo?.VisualControl?.SetValue(currentKey);
                    throw new ArgumentException($"[Visualize] Type mismatch: Expected {keyType}, got {resolvedKey.GetType()}");
                }

                object? currentValue = adapter.Get(currentKey);
                adapter.Remove(currentKey);
                adapter.Set(resolvedKey, currentValue);
                currentKey = resolvedKey;
                context.ValueChanged(dictionaryObject);

                if (keyWasAutoAdjusted)
                {
                    keyControlInfo?.VisualControl?.SetValue(currentKey);
                }

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
            if (useStableDisplayOrder)
            {
                ReconcileDisplayOrder();
            }

            foreach (Node child in dictionaryVBox.GetChildren())
            {
                if (child == addButton)
                {
                    continue;
                }

                dictionaryVBox.RemoveChild(child);
                child.QueueFree();
            }

            if (!useStableDisplayOrder)
            {
                foreach ((object key, object value) in adapter.Entries)
                {
                    AddEntry(key, value);
                }
            }
            else
            {
                foreach (object key in displayOrder)
                {
                    object? value = adapter.Get(key);

                    if (value == null)
                    {
                        continue;
                    }

                    AddEntry(key, value);
                }
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
        MethodInfo? containsKeyMethod = FindDictionarySingleParameterMethod(dictionaryType, "ContainsKey");
        MethodInfo? removeMethod = FindDictionarySingleParameterMethod(dictionaryType, "Remove");
        Type indexerKeyType = indexerProperty?.GetIndexParameters().Length == 1
            ? indexerProperty.GetIndexParameters()[0].ParameterType
            : typeof(object);
        Type indexerValueType = indexerProperty?.PropertyType ?? typeof(object);
        Type containsKeyParamType = containsKeyMethod?.GetParameters().Length == 1
            ? containsKeyMethod.GetParameters()[0].ParameterType
            : typeof(object);
        Type removeKeyParamType = removeMethod?.GetParameters().Length == 1
            ? removeMethod.GetParameters()[0].ParameterType
            : typeof(object);

        // Godot generic dictionaries are adapted through reflected members.
        if (indexerProperty == null || containsKeyMethod == null || removeMethod == null)
        {
            return new DictionaryAdapter(() => [], _ => false, _ => null, (_, _) => { }, _ => { });
        }

        return new DictionaryAdapter(
            () => EnumerateObjectDictionaryEntries(dictionaryObject),
            key => (bool)containsKeyMethod.Invoke(dictionaryObject, [ConvertDictionaryValueToExpectedType(key, containsKeyParamType)])!,
            key => indexerProperty.GetValue(dictionaryObject, [ConvertDictionaryValueToExpectedType(key, indexerKeyType)]),
            (key, value) => indexerProperty.SetValue(dictionaryObject, ConvertDictionaryValueToExpectedType(value, indexerValueType), [ConvertDictionaryValueToExpectedType(key, indexerKeyType)]),
            key => removeMethod.Invoke(dictionaryObject, [ConvertDictionaryValueToExpectedType(key, removeKeyParamType)]));
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

    private static bool TryGetNextAvailableAddKey(object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey)
    {
        resolvedKey = duplicateKey;

        if (keyType.IsEnum)
        {
            long candidate;

            try
            {
                candidate = Convert.ToInt64(duplicateKey);
            }
            catch (Exception)
            {
                return false;
            }

            while (true)
            {
                candidate += 1;
                object enumKey = Enum.ToObject(keyType, candidate);

                if (containsKey(enumKey))
                {
                    continue;
                }

                resolvedKey = enumKey;
                return true;
            }
        }

        if (keyType.IsNumericType())
        {
            long candidate;

            try
            {
                candidate = Convert.ToInt64(duplicateKey);
            }
            catch (Exception)
            {
                return false;
            }

            while (true)
            {
                candidate += 1;

                try
                {
                    object nextKey = Convert.ChangeType(candidate, keyType)!;

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

        if (keyType == typeof(string) || keyType == typeof(object))
        {
            string baseKey = duplicateKey?.ToString() ?? "Key";
            int suffix = 1;

            while (true)
            {
                object nextKey = $"{baseKey} {suffix}";

                if (containsKey(nextKey))
                {
                    suffix++;
                    continue;
                }

                resolvedKey = nextKey;
                return true;
            }
        }

        return false;
    }

    private static object? ConvertDictionaryValueToExpectedType(object? value, Type expectedType)
    {
        if (expectedType == typeof(Variant))
        {
            return ConvertDictionaryVariantValue(value);
        }

        if (value == null || expectedType.IsInstanceOfType(value))
        {
            return value;
        }

        return value;
    }

    private static MethodInfo? FindDictionarySingleParameterMethod(Type type, string methodName)
    {
        foreach (MethodInfo method in type.GetMethods())
        {
            if (method.Name == methodName && method.GetParameters().Length == 1)
            {
                return method;
            }
        }

        return null;
    }

    private static Variant ConvertDictionaryVariantValue(object? value)
    {
        return value switch
        {
            null => Variant.From((string?)null),
            Variant variant => variant,
            bool v => Variant.From(v),
            int v => Variant.From(v),
            long v => Variant.From(v),
            float v => Variant.From(v),
            double v => Variant.From(v),
            string v => Variant.From(v),
            Vector2 v => Variant.From(v),
            Vector2I v => Variant.From(v),
            Vector3 v => Variant.From(v),
            Vector3I v => Variant.From(v),
            Vector4 v => Variant.From(v),
            Vector4I v => Variant.From(v),
            Quaternion v => Variant.From(v),
            Color v => Variant.From(v),
            NodePath v => Variant.From(v),
            StringName v => Variant.From(v),
            GodotObject v => Variant.From(v),
            _ => Variant.From(value.ToString() ?? string.Empty)
        };
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
