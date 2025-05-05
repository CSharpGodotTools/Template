using Godot;
using System;
using System.Reflection;

namespace Template;

/// <summary>
/// A set of utility methods for handling the VisualizeAttribute
/// </summary>
public static class VisualHandler
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
            object convertedValue = Convert.ChangeType(value, property.PropertyType);

            if (property.GetMethod.IsStatic)
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
        object convertedValue = Convert.ChangeType(value, field.FieldType);

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

        object value = member switch
        {
            FieldInfo fieldInfo when fieldInfo.IsStatic => fieldInfo.GetValue(null),
            FieldInfo fieldInfo => fieldInfo.GetValue(node),

            PropertyInfo propertyInfo when propertyInfo.GetMethod.IsStatic => propertyInfo.GetValue(null),
            PropertyInfo propertyInfo => propertyInfo.GetValue(node),

            _ => throw new ArgumentException("[Visualize] Member is not a FieldInfo or PropertyInfo")
        };

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

            PropertyInfo propertyInfo when propertyInfo.GetMethod.IsStatic => propertyInfo.GetValue(null),
            PropertyInfo propertyInfo => propertyInfo.GetValue(obj),

            _ => throw new ArgumentException("[Visualize] Member must be a field or property.")
        };
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
