#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Converts dictionary values to compatible runtime types.
/// </summary>
internal sealed class VisualDictionaryValueConverter : IVisualDictionaryValueConverter
{
    /// <summary>
    /// Converts a value to the expected runtime type when needed.
    /// </summary>
    /// <param name="value">Incoming value.</param>
    /// <param name="expectedType">Expected runtime type.</param>
    /// <returns>Converted value.</returns>
    public object? ConvertToExpectedType(object? value, Type expectedType)
    {
        // Convert to Variant when the destination type requires it.
        if (expectedType == typeof(Variant))
        {
            return ConvertVariant(value);
        }

        // Preserve nulls and already compatible values.
        if (value == null || expectedType.IsInstanceOfType(value))
        {
            return value;
        }

        return value;
    }

    /// <summary>
    /// Converts common CLR and Godot values into a <see cref="Variant"/> payload.
    /// </summary>
    /// <param name="value">Source value to convert.</param>
    /// <returns>Equivalent variant value.</returns>
    private static Variant ConvertVariant(object? value)
    {
        // Map known CLR/Godot types into Variant payloads.
        return value switch
        {
            null => Variant.From((string?)null),
            Variant variant => variant,
            bool v => Variant.From(v),
            int v => Variant.From(v),
            long v => Variant.From(v),
            float v => Variant.From(v),
            double v => Variant.From(v),
            string v => Variant.From(v),
            Vector2 v => Variant.From(v),
            Vector2I v => Variant.From(v),
            Vector3 v => Variant.From(v),
            Vector3I v => Variant.From(v),
            Vector4 v => Variant.From(v),
            Vector4I v => Variant.From(v),
            Quaternion v => Variant.From(v),
            Color v => Variant.From(v),
            NodePath v => Variant.From(v),
            StringName v => Variant.From(v),
            GodotObject v => Variant.From(v),
            _ => Variant.From(value.ToString() ?? string.Empty)
        };
    }
}
#endif
