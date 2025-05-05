using Godot;
using GodotUtils;
using System;
using System.Reflection;

namespace Template;

/// <summary>
/// More utility methods
/// </summary>
public static partial class VisualControlTypes
{
    private static void SetControlValue(Control control, object value)
    {
        if (control is ColorPickerButton colorPickerButton)
        {
            colorPickerButton.Color = (Color)value;
        }
        else if (control is LineEdit lineEdit)
        {
            lineEdit.Text = (string)value;
        }
        else if (control is SpinBox spinBox)
        {
            spinBox.Value = Convert.ToDouble(value);
        }
        else if (control is CheckBox checkBox)
        {
            checkBox.ButtonPressed = (bool)value;
        }
        else if (control is OptionButton optionButton)
        {
            optionButton.Select((int)value);
        }
        else if (control is ColorPickerButton colorPicker)
        {
            colorPicker.Color = (Color)value;
        }
        // Add more control types here as needed
    }

    // Helper method to remove an element from an array
    private static Array RemoveAt(this Array source, int index)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (index < 0 || index >= source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"[Visualize] Index was out of range");
        }

        Array dest = Array.CreateInstance(source.GetType().GetElementType(), source.Length - 1);
        Array.Copy(source, 0, dest, 0, index);
        Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    private static object ConvertNumericValue(SpinBox spinBox, double value, Type paramType)
    {
        object convertedValue = value;

        try
        {
            convertedValue = Convert.ChangeType(value, paramType);
        }
        catch
        {
            (object Min, object Max) = TypeRangeConstraints.GetRange(paramType);

            if (Convert.ToDouble(value) < Convert.ToDouble(Min))
            {
                spinBox.Value = Convert.ToDouble(Min);
                convertedValue = Min;
            }
            else if (Convert.ToDouble(value) > Convert.ToDouble(Max))
            {
                spinBox.Value = Convert.ToDouble(Max);
                convertedValue = Max;
            }
            else
            {
                string errorMessage = $"[Visualize] The provided value '{value}' is not assignable to the parameter type '{paramType}'.";
                throw new InvalidOperationException(errorMessage);
            }
        }

        return convertedValue;
    }

    private static SpinBox CreateSpinBox(Type type, MemberInfo memberInfo)
    {
        SpinBox spinBox = new()
        {
            UpdateOnTextChanged = true,
            AllowLesser = false,
            AllowGreater = false,
            MinValue = int.MinValue,
            MaxValue = int.MaxValue,
            Alignment = HorizontalAlignment.Center
        };

        spinBox.Step = type switch
        {
            _ when type == typeof(float) => 0.1,
            _ when type == typeof(double) => 0.1,
            _ when type == typeof(decimal) => 0.01,
            _ when type == typeof(int) => 1,
            _ => 1
        };

        return spinBox;
    }
}
