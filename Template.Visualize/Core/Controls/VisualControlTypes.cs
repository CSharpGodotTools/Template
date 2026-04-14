#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Maps reflected .NET/Godot types to concrete visualize UI control implementations.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a visual control for the provided member type and initial context.
    /// </summary>
    /// <param name="type">Member or parameter type to render.</param>
    /// <param name="memberInfo">Optional member metadata used by some control providers.</param>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Control info containing created control or null when unsupported.</returns>
    public static VisualControlInfo CreateControlForType(Type type, MemberInfo? memberInfo, VisualControlContext context)
    {
        VisualControlInfo info = type switch
        {
            _ when type == typeof(bool) => VisualBool(context),
            _ when type == typeof(string) => VisualString(context),
            _ when type == typeof(object) => VisualObject(context),
            _ when type == typeof(Color) => VisualColor(context),
            _ when type == typeof(Vector2) => VisualVector2(context),
            _ when type == typeof(Vector2I) => VisualVector2I(context),
            _ when type == typeof(Vector3) => VisualVector3(context),
            _ when type == typeof(Vector3I) => VisualVector3I(context),
            _ when type == typeof(Vector4) => VisualVector4(context),
            _ when type == typeof(Vector4I) => VisualVector4I(context),
            _ when type == typeof(Quaternion) => VisualQuaternion(context),
            _ when type == typeof(NodePath) => VisualNodePath(context),
            _ when type == typeof(StringName) => VisualStringName(context),
            _ when type.IsNumericType() => VisualNumeric(type, memberInfo, context),
            _ when type.IsEnum => VisualEnum(type, context),

            // Arrays
            _ when type.IsArray => VisualArray(type, context),
            _ when type == typeof(Godot.Collections.Array) => VisualGodotArray(context),
            _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) => VisualList(type, context),
            // Reuse the list renderer for Godot's generic array API.
            _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Godot.Collections.Array<>) => VisualList(type, context),
            _ when type == typeof(Godot.Collections.Dictionary) => CreateDictionaryControl(type, context),
            _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>) => CreateDictionaryControl(type, context),
            // Reuse the dictionary renderer for Godot's generic dictionary API.
            _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Godot.Collections.Dictionary<,>) => CreateDictionaryControl(type, context),

            // Godot Resource
            _ when type.IsClass && type.IsSubclassOf(typeof(Resource)) => VisualClass(type, context),

            // Class
            _ when type.IsClass && !type.IsSubclassOf(typeof(GodotObject)) => VisualClass(type, context),

            // Struct
            _ when type.IsValueType && !type.IsClass && !type.IsSubclassOf(typeof(GodotObject)) => VisualClass(type, context),

            // Not defined
            _ => new VisualControlInfo(null)
        };

        // Return an unsupported control placeholder when no visual control was created.
        if (info.VisualControl == null)
            PrintUtils.Warning($"[Visualize] The type '{type.Namespace}.{type.Name}' is not supported for the {nameof(VisualizeAttribute)}");

        return info;
    }

    /// <summary>
    /// Creates dictionary visual controls through the dictionary-provider pipeline.
    /// </summary>
    /// <param name="dictionaryType">Dictionary runtime type.</param>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Dictionary visual control info.</returns>
    private static VisualControlInfo CreateDictionaryControl(Type dictionaryType, VisualControlContext context)
    {
        VisualDictionaryControlProvider provider = new(
            new VisualDictionaryAdapterFactory(new VisualDictionaryValueConverter()),
            new VisualDictionaryKeyResolver(),
            new VisualDictionaryDisplayOrderTrackerFactory(),
            (type, visualContext) => CreateControlForType(type, null, visualContext),
            VisualMethods.CreateDefaultValue,
            CleanupOnTreeExited);

        return provider.CreateControl(dictionaryType, context);
    }
}
#endif
