#if DEBUG
using Godot;
using System;
using System.Collections;
using System.Reflection;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualGodotArray(VisualControlContext context)
    {
        object arrayObject = context.InitialValue ?? new Godot.Collections.Array();

        if (arrayObject is IList list)
        {
            return CreateIndexedCollectionControl(
                typeof(object),
                () => list.Count,
                index => list[index],
                (index, value) => list[index] = value,
                value => list.Add(value),
                index => list.RemoveAt(index),
                () => list,
                value =>
                {
                    if (value is IList nextList)
                    {
                        list = nextList;
                    }
                },
                context);
        }

        Type type = arrayObject.GetType();
        PropertyInfo? countProperty = type.GetProperty("Count");
        PropertyInfo? indexerProperty = type.GetProperty("Item");
        MethodInfo? addMethod = FindSingleParameterMethod(type, "Add");
        MethodInfo? removeAtMethod = type.GetMethod("RemoveAt", [typeof(int)]);
        Type indexerValueType = indexerProperty?.PropertyType ?? typeof(object);
        Type addValueType = addMethod?.GetParameters().Length == 1 ? addMethod.GetParameters()[0].ParameterType : typeof(object);

        if (countProperty == null || indexerProperty == null || addMethod == null || removeAtMethod == null)
        {
            return new VisualControlInfo(null);
        }

        return CreateIndexedCollectionControl(
            typeof(object),
            () => (int)countProperty.GetValue(arrayObject)!,
            index => indexerProperty.GetValue(arrayObject, [index]),
            (index, value) => indexerProperty.SetValue(arrayObject, ConvertArrayValueToExpectedType(value, indexerValueType), [index]),
            value => addMethod.Invoke(arrayObject, [ConvertArrayValueToExpectedType(value, addValueType)]),
            index => removeAtMethod.Invoke(arrayObject, [index]),
            () => arrayObject,
            value =>
            {
                if (value != null)
                {
                    arrayObject = value;
                }
            },
            context);
    }

    private static VisualControlInfo VisualArray(Type type, VisualControlContext context)
    {
        Type? elementType = type.GetElementType();
        Array array = context.InitialValue as Array ?? Array.CreateInstance(elementType!, 0);
        return CreateIndexedCollectionControl(
            elementType!,
            () => array.Length,
            index => array.GetValue(index)!,
            (index, value) => array.SetValue(value, index),
            value => array = Append(array, value),
            index => array = array.RemoveAt(index),
            () => array,
            value =>
            {
                if (value is Array nextArray)
                {
                    array = nextArray;
                }
            },
            context);
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

    private static object? ConvertArrayValueToExpectedType(object? value, Type expectedType)
    {
        if (expectedType == typeof(Variant))
        {
            return ConvertToVariantValue(value);
        }

        if (value == null || expectedType.IsInstanceOfType(value))
        {
            return value;
        }

        return value;
    }

    private static Variant ConvertToVariantValue(object? value)
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
}
#endif
