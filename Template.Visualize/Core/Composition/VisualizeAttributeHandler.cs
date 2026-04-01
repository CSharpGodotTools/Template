#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Discovers fields, properties, and methods annotated with <see cref="VisualizeAttribute"/>.
/// </summary>
internal static class VisualizeAttributeHandler
{
    private static readonly BindingFlags _flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    /// <summary>
    /// Retrieves visualization metadata for an object when at least one annotated member exists.
    /// </summary>
    /// <param name="visualizedObject">Object inspected for visualization attributes.</param>
    /// <param name="anchorNode">Anchor node used for positioning visual UI.</param>
    /// <returns>Visual metadata when annotated members exist; otherwise <see langword="null"/>.</returns>
    public static VisualData? RetrieveData(object visualizedObject, Node anchorNode)
    {
        Type type = visualizedObject.GetType();

        List<PropertyInfo> properties = GetVisualMembers<PropertyInfo>(type.GetProperties);
        List<FieldInfo> fields = GetVisualMembers<FieldInfo>(type.GetFields);
        List<MethodInfo> methods = GetVisualMembers<MethodInfo>(type.GetMethods);

        // Return a visual data payload only when at least one annotated member was found.
        if (properties.Count != 0 || fields.Count != 0 || methods.Count != 0)
        {
            return new VisualData(anchorNode, visualizedObject, properties, fields, methods);
        }

        // No annotated members means this object should not be tracked by visualize runtime.
        return null;
    }

    /// <summary>
    /// Collects members of a given kind that are annotated with <see cref="VisualizeAttribute"/>.
    /// </summary>
    /// <typeparam name="T">Member type to collect.</typeparam>
    /// <param name="getMembers">Delegate that retrieves members for supplied binding flags.</param>
    /// <returns>Annotated member list.</returns>
    private static List<T> GetVisualMembers<T>(Func<BindingFlags, T[]> getMembers) where T : MemberInfo
    {
        return [.. getMembers(_flags).Where(member => member.GetCustomAttributes(typeof(VisualizeAttribute), false).Length != 0)];
    }
}
#endif
