using Godot;
using System;

namespace __TEMPLATE__;

/// <summary>
/// More utility methods
/// </summary>
public static partial class VisualControlTypes
{
    private static void SetControlValue(Control control, object value)
    {
        switch (control)
        {
            case ColorPickerButton colorPickerButton:
                colorPickerButton.Color = (Color)value;
                break;
            case LineEdit lineEdit:
                lineEdit.Text = (string)value;
                break;
            case SpinBox spinBox:
                spinBox.Value = Convert.ToDouble(value);
                break;
            case CheckBox checkBox:
                checkBox.ButtonPressed = (bool)value;
                break;
            case OptionButton optionButton:
                optionButton.Select((int)value);
                break;
            // Add more control types here as needed
        }
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

/*
    private static object ConvertNumericValue(SpinBox spinBox, double value, Type paramType)
    {
        object convertedValue;

        try
        {
            convertedValue = Convert.ChangeType(value, paramType);
        }
        catch
        {
            (object min, object max) = TypeRangeConstraints.GetRange(paramType);

            if (Convert.ToDouble(value) < Convert.ToDouble(min))
            {
                spinBox.Value = Convert.ToDouble(min);
                convertedValue = min;
            }
            else if (Convert.ToDouble(value) > Convert.ToDouble(max))
            {
                spinBox.Value = Convert.ToDouble(max);
                convertedValue = max;
            }
            else
            {
                string errorMessage = $"[Visualize] The provided value '{value}' is not assignable to the parameter type '{paramType}'.";
                throw new InvalidOperationException(errorMessage);
            }
        }

        return convertedValue;
    }
*/

    private static SpinBox CreateSpinBox(Type type)
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
