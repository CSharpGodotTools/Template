#if DEBUG
using Godot;
using System;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// A set of utility methods for handling the VisualizeAttribute
/// </summary>
internal static class VisualHandler
{
    public static void SetMemberValue(MemberInfo member, object target, object value)
    {
        try
        {
            if (member is PropertyInfo property)
            {
                SetPropertyValue(property, target, value);
            }
            else if (member is FieldInfo field)
            {
                SetFieldValue(field, target, value);
            }
        }
        catch (Exception ex)
        {
            GD.Print($"[Visualize] Failed to set value for {member.Name}: {ex.Message}");
        }
    }

    private static void SetPropertyValue(PropertyInfo property, object target, object value)
    {
        if (property.CanWrite)
        {
            object convertedValue = ConvertValue(value, property.PropertyType);

            MethodInfo setter = property.SetMethod;
            if (setter != null && setter.IsStatic)
            {
                property.SetValue(null, convertedValue);
            }
            else
            {
                property.SetValue(target, convertedValue);
            }
        }
        else
        {
            GD.Print($"[Visualize] Property {property.Name} is read-only.");
        }
    }

    private static void SetFieldValue(FieldInfo field, object target, object value)
    {
        object convertedValue = ConvertValue(value, field.FieldType);

        if (field.IsStatic)
        {
            field.SetValue(null, convertedValue);
        }
        else
        {
            field.SetValue(target, convertedValue);
        }
    }

    public static T GetMemberValue<T>(MemberInfo member, object node)
    {
        if (member == null)
        {
            return default;
        }

        object value = GetMemberValue(member, node);

        if (value == null)
        {
            return default;
        }

        if (value is float floatValue && typeof(T) == typeof(double))
        {
            return (T)(object)Convert.ToDouble(floatValue);
        }

        return (T)value;
    }

    public static object GetMemberValue(MemberInfo member, object obj)
    {
        return member switch
        {
            FieldInfo fieldInfo when fieldInfo.IsStatic => fieldInfo.GetValue(null),
            FieldInfo fieldInfo => fieldInfo.GetValue(obj),

            PropertyInfo propertyInfo when propertyInfo.GetGetMethod(true)?.IsStatic == true => propertyInfo.GetValue(null),
            PropertyInfo propertyInfo => propertyInfo.GetValue(obj),

            _ => throw new ArgumentException("[Visualize] Member must be a field or property.")
        };
    }

    private static object ConvertValue(object value, Type targetType)
    {
        if (value == null)
        {
            return null;
        }

        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        return Convert.ChangeType(value, targetType);
    }

    public static Type GetMemberType(MemberInfo member)
    {
        return member switch
        {
            FieldInfo fieldInfo => fieldInfo.FieldType,
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            _ => throw new ArgumentException("[Visualize] Member must be a field or property.")
        };
    }
}
#endif
