using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace __TEMPLATE__;

/// <summary>
/// Handles the VisualizeAttribute
/// </summary>
public static class VisualizeAttributeHandler
{
    private static readonly BindingFlags _flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static VisualData RetrieveData(Node specificNode)
    {
        Type type = specificNode.GetType();

        VisualizeAttribute attribute = (VisualizeAttribute)type.GetCustomAttribute(typeof(VisualizeAttribute), false);

        Vector2 initialPosition = Vector2.Zero;
        string[] readonlyMembers = null;

        if (attribute != null)
        {
            initialPosition = attribute.InitialPosition;
            readonlyMembers = attribute.VisualizeMembers;
        }

        List<PropertyInfo> properties = GetVisualMembers<PropertyInfo>(type.GetProperties);
        List<FieldInfo> fields = GetVisualMembers<FieldInfo>(type.GetFields);
        List<MethodInfo> methods = GetVisualMembers<MethodInfo>(type.GetMethods);

        if (properties.Count != 0 || fields.Count != 0 || methods.Count != 0 || (attribute != null && attribute.VisualizeMembers != null))
        {
            return new VisualData(specificNode, initialPosition, readonlyMembers, properties, fields, methods);
        }

        // Nothing to visualize for this node
        return null;
    }

    private static List<T> GetVisualMembers<T>(Func<BindingFlags, T[]> getMembers) where T : MemberInfo
    {
        return getMembers(_flags)
            .Where(member => member.GetCustomAttributes(typeof(VisualizeAttribute), false).Length != 0)
            .ToList();
    }
}
