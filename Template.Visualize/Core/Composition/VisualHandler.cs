#if DEBUG
using Godot;
using System;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Reflection helpers for reading and writing members exposed through <see cref="VisualizeAttribute"/>.
/// </summary>
internal static class VisualHandler
{
    private const string MemberTypeErrorMessage = "[Visualize] Member must be a field or property.";

    /// <summary>
    /// Writes a value to a field or property member, including conversion to the member type.
    /// </summary>
    /// <param name="member">Target field or property metadata.</param>
    /// <param name="target">Instance that owns non-static members.</param>
    /// <param name="value">Value to assign.</param>
    public static void SetMemberValue(MemberInfo member, object target, object value)
    {
        try
        {
            // Route writes through property handling when member metadata is a property.
            if (member is PropertyInfo property)
            {
                SetPropertyValue(property, target, value);
            }
            // Route writes through field handling when member metadata is a field.
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

    /// <summary>
    /// Writes a value to a property when writable.
    /// </summary>
    /// <param name="property">Property metadata.</param>
    /// <param name="target">Instance that owns non-static properties.</param>
    /// <param name="value">Value to assign.</param>
    private static void SetPropertyValue(PropertyInfo property, object target, object value)
    {
        // Ignore non-writable properties and log a diagnostic message.
        if (property.CanWrite)
        {
            object? convertedValue = ConvertValue(value, property.PropertyType);

            MethodInfo? setter = property.SetMethod;
            // Static setters do not require an instance target.
            if (setter != null && setter.IsStatic)
            {
                property.SetValue(null, convertedValue);
            }
            // Instance setters are applied to the provided target.
            else
            {
                property.SetValue(target, convertedValue);
            }
        }
        // Read-only properties are intentionally skipped.
        else
        {
            GD.Print($"[Visualize] Property {property.Name} is read-only.");
        }
    }

    /// <summary>
    /// Writes a value to a field, handling static and instance targets.
    /// </summary>
    /// <param name="field">Field metadata.</param>
    /// <param name="target">Instance that owns non-static fields.</param>
    /// <param name="value">Value to assign.</param>
    private static void SetFieldValue(FieldInfo field, object target, object value)
    {
        object? convertedValue = ConvertValue(value, field.FieldType);

        // Static fields are set without an instance target.
        if (field.IsStatic)
        {
            field.SetValue(null, convertedValue);
        }
        // Instance fields are set on the supplied target object.
        else
        {
            field.SetValue(target, convertedValue);
        }
    }

    /// <summary>
    /// Reads a member value and attempts to cast or adapt it to the requested generic type.
    /// </summary>
    /// <typeparam name="T">Requested return type.</typeparam>
    /// <param name="member">Field or property metadata.</param>
    /// <param name="node">Instance used for non-static member access.</param>
    /// <returns>Converted value when available; otherwise default value for <typeparamref name="T"/>.</returns>
    public static T? GetMemberValue<T>(MemberInfo member, object node)
    {
        // Missing member metadata yields default immediately.
        if (member == null)
        {
            return default;
        }

        object? value = GetMemberValue(member, node);

        // Null values map to default generic result.
        if (value == null)
        {
            return default;
        }

        // Support float-to-double adaptation for numeric UI controls.
        if (value is float floatValue && typeof(T) == typeof(double))
        {
            return (T)(object)Convert.ToDouble(floatValue);
        }

        return (T)value;
    }

    /// <summary>
    /// Reads a value from a field or property, including support for static members.
    /// </summary>
    /// <param name="member">Field or property metadata.</param>
    /// <param name="obj">Instance used for non-static members.</param>
    /// <returns>Member value.</returns>
    /// <exception cref="ArgumentException">Thrown when member is neither field nor property.</exception>
    public static object? GetMemberValue(MemberInfo member, object? obj)
    {
        return (member switch
        {
            FieldInfo fieldInfo when fieldInfo.IsStatic => fieldInfo.GetValue(null),
            FieldInfo fieldInfo => fieldInfo.GetValue(obj),

            PropertyInfo propertyInfo when propertyInfo.GetGetMethod(true)?.IsStatic == true => propertyInfo.GetValue(null),
            PropertyInfo propertyInfo => propertyInfo.GetValue(obj),

            _ => throw new ArgumentException(MemberTypeErrorMessage)
        });
    }

    /// <summary>
    /// Converts a value to a target type when needed for member assignment.
    /// </summary>
    /// <param name="value">Source value.</param>
    /// <param name="targetType">Type expected by target member.</param>
    /// <returns>Converted value or original value when already compatible.</returns>
    private static object? ConvertValue(object value, Type targetType)
    {
        // Preserve null assignments for nullable/reference targets.
        if (value == null)
        {
            return null;
        }

        // Skip conversion when value already matches target type.
        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        return Convert.ChangeType(value, targetType);
    }

    /// <summary>
    /// Returns the declared type of a field or property member.
    /// </summary>
    /// <param name="member">Field or property metadata.</param>
    /// <returns>Declared member type.</returns>
    /// <exception cref="ArgumentException">Thrown when member is neither field nor property.</exception>
    public static Type GetMemberType(MemberInfo member)
    {
        return member switch
        {
            FieldInfo fieldInfo => fieldInfo.FieldType,
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            _ => throw new ArgumentException(MemberTypeErrorMessage)
        };
    }
}
#endif
