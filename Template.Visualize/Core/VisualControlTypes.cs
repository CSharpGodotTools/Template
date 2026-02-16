#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates a control for the given member type and context
    /// </summary>
    public static VisualControlInfo CreateControlForType(Type type, MemberInfo memberInfo, VisualControlContext context)
    {
        VisualControlInfo info = type switch
        {
            _ when type == typeof(bool)       => VisualBool(context),
            _ when type == typeof(string)     => VisualString(context),
            _ when type == typeof(object)     => VisualObject(context),
            _ when type == typeof(Color)      => VisualColor(context),
            _ when type == typeof(Vector2)    => VisualVector2(context),
            _ when type == typeof(Vector2I)   => VisualVector2I(context),
            _ when type == typeof(Vector3)    => VisualVector3(context),
            _ when type == typeof(Vector3I)   => VisualVector3I(context),
            _ when type == typeof(Vector4)    => VisualVector4(context),
            _ when type == typeof(Vector4I)   => VisualVector4I(context),
            _ when type == typeof(Quaternion) => VisualQuaternion(context),
            _ when type == typeof(NodePath)   => VisualNodePath(context),
            _ when type == typeof(StringName) => VisualStringName(context),
            _ when type.IsNumericType()       => VisualNumeric(type, memberInfo, context),
            _ when type.IsEnum                => VisualEnum(type, context),
            
            // Arrays
            _ when type.IsArray => VisualArray(type, context),
            _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) => VisualList(type, context),
            _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>) => VisualDictionary(type, context),
            
            // Godot Resource
            _ when type.IsClass && type.IsSubclassOf(typeof(Resource)) => VisualClass(type, context),
            
            // Class
            _ when type.IsClass && !type.IsSubclassOf(typeof(GodotObject)) => VisualClass(type, context),
            
            // Struct
            _ when type.IsValueType && !type.IsClass && !type.IsSubclassOf(typeof(GodotObject)) => VisualClass(type, context),
            
            // Not defined
            _ => new VisualControlInfo(null)
        };

        if (info.VisualControl == null)
        {
            PrintUtils.Warning($"[Visualize] The type '{type.Namespace}.{type.Name}' is not supported for the {nameof(VisualizeAttribute)}");
        }

        return info;
    }
}

internal interface IVisualControl
{
    void SetValue(object value);
    Control Control { get; }
    void SetEditable(bool editable);
}

internal sealed class VisualControlInfo(IVisualControl visualControl)
{
    public IVisualControl VisualControl { get; } = visualControl;
}
#endif
