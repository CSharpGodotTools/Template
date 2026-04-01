#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Builds adapters for dictionary-like runtime types.
/// </summary>
/// <param name="valueConverter">Converter for adapting values to expected types.</param>
internal sealed class VisualDictionaryAdapterFactory(IVisualDictionaryValueConverter valueConverter) : IVisualDictionaryAdapterFactory
{
    private readonly IVisualDictionaryValueConverter _valueConverter = valueConverter;

    /// <summary>
    /// Creates an adapter for the provided dictionary instance and type.
    /// </summary>
    /// <param name="dictionaryObject">Dictionary instance.</param>
    /// <param name="dictionaryType">Dictionary runtime type.</param>
    /// <returns>Adapter for the dictionary.</returns>
    public IVisualDictionaryAdapter Create(object dictionaryObject, Type dictionaryType)
    {
        // Fast path for IDictionary implementations.
        if (dictionaryObject is IDictionary dictionary)
        {
            return new VisualDictionaryAdapter(
                () => EnumerateDictionaryEntries(dictionary),
                key => dictionary.Contains(key),
                key => dictionary[key],
                (key, value) => dictionary[key] = value,
                key => dictionary.Remove(key));
        }

        PropertyInfo? indexerProperty = dictionaryType.GetProperty("Item");
        MethodInfo? containsKeyMethod = FindSingleParameterMethod(dictionaryType, "ContainsKey");
        MethodInfo? removeMethod = FindSingleParameterMethod(dictionaryType, "Remove");

        // Fallback to an empty adapter when required members are missing.
        if (indexerProperty == null || containsKeyMethod == null || removeMethod == null)
        {
            return CreateEmptyAdapter();
        }

        Type indexerKeyType = indexerProperty.GetIndexParameters().Length == 1
            ? indexerProperty.GetIndexParameters()[0].ParameterType
            : typeof(object);
        Type indexerValueType = indexerProperty.PropertyType;
        Type containsKeyParamType = containsKeyMethod.GetParameters().Length == 1
            ? containsKeyMethod.GetParameters()[0].ParameterType
            : typeof(object);
        Type removeKeyParamType = removeMethod.GetParameters().Length == 1
            ? removeMethod.GetParameters()[0].ParameterType
            : typeof(object);

        // Build an adapter around reflected dictionary APIs.
        return new VisualDictionaryAdapter(
            () => EnumerateObjectDictionaryEntries(dictionaryObject),
            key => (bool)containsKeyMethod.Invoke(dictionaryObject, [_valueConverter.ConvertToExpectedType(key, containsKeyParamType)])!,
            key => indexerProperty.GetValue(dictionaryObject, [_valueConverter.ConvertToExpectedType(key, indexerKeyType)]),
            (key, value) => indexerProperty.SetValue(
                dictionaryObject,
                _valueConverter.ConvertToExpectedType(value, indexerValueType),
                [_valueConverter.ConvertToExpectedType(key, indexerKeyType)]),
            key => removeMethod.Invoke(dictionaryObject, [_valueConverter.ConvertToExpectedType(key, removeKeyParamType)]));
    }

    /// <summary>
    /// Creates a no-op adapter for unsupported dictionary shapes.
    /// </summary>
    /// <returns>Empty dictionary adapter.</returns>
    private static VisualDictionaryAdapter CreateEmptyAdapter()
    {
        return new VisualDictionaryAdapter(() => [], _ => false, _ => null, (_, _) => { }, _ => { });
    }

    /// <summary>
    /// Finds a method with the given name and a single parameter.
    /// </summary>
    /// <param name="type">Type to inspect.</param>
    /// <param name="methodName">Method name to match.</param>
    /// <returns>Matching method info, or null when not found.</returns>
    private static MethodInfo? FindSingleParameterMethod(Type type, string methodName)
    {
        foreach (MethodInfo method in type.GetMethods())
        {
            // Match the name and expected parameter count.
            if (method.Name == methodName && method.GetParameters().Length == 1)
            {
                return method;
            }
        }

        return null;
    }

    /// <summary>
    /// Enumerates entries from an <see cref="IDictionary"/> instance.
    /// </summary>
    /// <param name="dictionary">Dictionary to enumerate.</param>
    /// <returns>Enumerable of key/value entries.</returns>
    private static IEnumerable<(object Key, object Value)> EnumerateDictionaryEntries(IDictionary dictionary)
    {
        foreach (DictionaryEntry entry in dictionary)
        {
            yield return (entry.Key, entry.Value!);
        }
    }

    /// <summary>
    /// Enumerates entries from a dictionary-like object using reflection.
    /// </summary>
    /// <param name="dictionaryObject">Dictionary-like object.</param>
    /// <returns>Enumerable of key/value entries.</returns>
    private static IEnumerable<(object Key, object Value)> EnumerateObjectDictionaryEntries(object dictionaryObject)
    {
        // Stop when the object is not enumerable.
        if (dictionaryObject is not IEnumerable enumerable)
        {
            yield break;
        }

        foreach (object? entry in enumerable)
        {
            // Skip null entries in the enumeration.
            if (entry == null)
            {
                continue;
            }

            // Handle non-generic DictionaryEntry values.
            if (entry is DictionaryEntry dictionaryEntry)
            {
                yield return (dictionaryEntry.Key, dictionaryEntry.Value!);
                continue;
            }

            Type entryType = entry.GetType();
            PropertyInfo? keyProperty = entryType.GetProperty("Key");
            PropertyInfo? valueProperty = entryType.GetProperty("Value");

            // Skip entries without key/value properties.
            if (keyProperty == null || valueProperty == null)
            {
                continue;
            }

            object? key = keyProperty.GetValue(entry);
            object? value = valueProperty.GetValue(entry);

            // Skip entries with null key/value values.
            if (key == null || value == null)
            {
                continue;
            }

            yield return (key, value);
        }
    }
}
#endif
