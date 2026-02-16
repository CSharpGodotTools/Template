#if DEBUG
using System;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualArray(Type type, VisualControlContext context)
    {
        Type elementType = type.GetElementType();
        Array array = context.InitialValue as Array ?? Array.CreateInstance(elementType, 0);
        return CreateIndexedCollectionControl(
            elementType,
            () => array.Length,
            index => array.GetValue(index),
            (index, value) => array.SetValue(value, index),
            value => array = Append(array, value),
            index => array = array.RemoveAt(index),
            () => array,
            context);
    }
}
#endif
