#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

internal sealed class VisualDictionaryAdapterFactory(IVisualDictionaryValueConverter valueConverter) : IVisualDictionaryAdapterFactory
{
    private readonly IVisualDictionaryValueConverter _valueConverter = valueConverter;

    public IVisualDictionaryAdapter Create(object dictionaryObject, Type dictionaryType)
    {
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

    private static VisualDictionaryAdapter CreateEmptyAdapter()
    {
        return new VisualDictionaryAdapter(() => [], _ => false, _ => null, (_, _) => { }, _ => { });
    }

    private static MethodInfo? FindSingleParameterMethod(Type type, string methodName)
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

    private static IEnumerable<(object Key, object Value)> EnumerateDictionaryEntries(IDictionary dictionary)
    {
        foreach (DictionaryEntry entry in dictionary)
        {
            yield return (entry.Key, entry.Value!);
        }
    }

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
}
#endif
