#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Handles the VisualizeAttribute
/// </summary>
internal static class VisualizeAttributeHandler
{
    private static readonly BindingFlags _flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static VisualData? RetrieveData(object visualizedObject, Node anchorNode)
    {
        Type type = visualizedObject.GetType();

        List<PropertyInfo> properties = GetVisualMembers<PropertyInfo>(type.GetProperties);
        List<FieldInfo> fields = GetVisualMembers<FieldInfo>(type.GetFields);
        List<MethodInfo> methods = GetVisualMembers<MethodInfo>(type.GetMethods);

        // There is something to visualize for this node
        if (properties.Count != 0 || fields.Count != 0 || methods.Count != 0)
        {
            return new VisualData(anchorNode, visualizedObject, properties, fields, methods);
        }

        // Nothing to visualize for this node
        return null;
    }

    private static List<T> GetVisualMembers<T>(Func<BindingFlags, T[]> getMembers) where T : MemberInfo
    {
        return [.. getMembers(_flags).Where(member => member.GetCustomAttributes(typeof(VisualizeAttribute), false).Length != 0)];
    }
}
#endif
