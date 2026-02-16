#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualList(Type type, VisualControlContext context)
    {
        Type elementType = type.GetGenericArguments()[0];
        IList list = context.InitialValue as IList ?? (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
        return CreateIndexedCollectionControl(
            elementType,
            () => list.Count,
            index => list[index],
            (index, value) => list[index] = value,
            value => list.Add(value),
            index => list.RemoveAt(index),
            () => list,
            context);
    }
}
#endif
