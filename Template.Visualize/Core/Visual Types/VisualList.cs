#if DEBUG
using System;
using System.Collections;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// List/array-like visual-control builders for generic list types.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a control for generic list-like collections.
    /// </summary>
    /// <param name="type">Concrete list type.</param>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created list-control info.</returns>
    private static VisualControlInfo VisualList(Type type, VisualControlContext context)
    {
        Type elementType = type.GetGenericArguments()[0];
        object listObject = context.InitialValue ?? Activator.CreateInstance(type)!;

        // Fast path for CLR collections that expose IList.
        if (listObject is IList list)
        {
            return CreateIndexedCollectionControl(
                elementType,
                () => list.Count,
                index => list[index],
                (index, value) => list[index] = value,
                value => list.Add(value),
                index => list.RemoveAt(index),
                () => list,
                value =>
                {
                    // Replace backing list reference when the collection instance changes.
                    if (value is IList nextList)
                        list = nextList;
                },
                context);
        }

        PropertyInfo? countProperty = type.GetProperty("Count");
        PropertyInfo? indexerProperty = type.GetProperty("Item");
        MethodInfo? addMethod = type.GetMethod("Add", [elementType]);
        MethodInfo? removeAtMethod = type.GetMethod("RemoveAt", [typeof(int)]);

        // Fallback path for Godot.Collections.Array<T> and other list-like types without IList.
        if (countProperty == null || indexerProperty == null || addMethod == null || removeAtMethod == null)
            return new VisualControlInfo(null);

        return CreateIndexedCollectionControl(
            elementType,
            () => (int)countProperty.GetValue(listObject)!,
            index => indexerProperty.GetValue(listObject, [index]),
            (index, value) => indexerProperty.SetValue(listObject, value, [index]),
            value => addMethod.Invoke(listObject, [value]),
            index => removeAtMethod.Invoke(listObject, [index]),
            () => listObject,
            value =>
            {
                // Replace backing object when a non-null list-like value is provided.
                if (value != null)
                    listObject = value;
            },
            context);
    }
}
#endif
