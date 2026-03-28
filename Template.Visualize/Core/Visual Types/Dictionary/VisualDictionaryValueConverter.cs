#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal sealed class VisualDictionaryValueConverter : IVisualDictionaryValueConverter
{
    public object? ConvertToExpectedType(object? value, Type expectedType)
    {
        if (expectedType == typeof(Variant))
        {
            return ConvertVariant(value);
        }

        if (value == null || expectedType.IsInstanceOfType(value))
        {
            return value;
        }

        return value;
    }

    private static Variant ConvertVariant(object? value)
    {
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
