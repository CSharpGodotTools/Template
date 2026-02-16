#if DEBUG
using Godot;
using System;
using System.Linq;
using System.Reflection;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private const double DefaultStepValue = 0.005;

    private static VisualControlInfo VisualNumeric(Type type, MemberInfo memberInfo, VisualControlContext context)
    {
        SpinBox control;

        if (memberInfo != null && TryGetExportAttributeRange(memberInfo, out ExportAttributeRange range))
        {
            control = new SpinBox()
            {
                Alignment = HorizontalAlignment.Center,
                MinValue = range.MinValue,
                MaxValue = range.MaxValue,
                Step = range.StepValue,
            };
        }
        else
        {
            control = CreateSpinBox(type);
        }

        control.Value = Convert.ToDouble(context.InitialValue);

        control.ValueChanged += value =>
        {
            object convertedValue = Convert.ChangeType(value, type);
            context.ValueChanged(convertedValue);
        };

        return new VisualControlInfo(new NumericControl(control));
    }

    /// <summary>
    /// Checks if the member has [Export(PropertyHint.Range, "0, 1, 0.1")] and gets the min, max, and step values.
    /// </summary>
    private static bool TryGetExportAttributeRange(MemberInfo memberInfo, out ExportAttributeRange exportAttributeRange)
    {
        ExportAttribute exportAttribute = memberInfo.GetCustomAttribute<ExportAttribute>();
        exportAttributeRange = null;

        if (exportAttribute == null || exportAttribute.Hint != PropertyHint.Range)
            return false;

        string[] args = [.. exportAttribute.HintString.Split(',').Select(x => x.Trim())];

        if (args.Length < 2)
            return false;

        if (!double.TryParse(args[0], out double minValue))
            return false;

        if (!double.TryParse(args[1], out double maxValue))
            return false;

        double stepValue = DefaultStepValue;

        if (args.Length >= 3 && double.TryParse(args[2].Trim(), out double parsedStepValue))
        {
            stepValue = parsedStepValue;
        }

        exportAttributeRange = new ExportAttributeRange(minValue, maxValue, stepValue);

        return true;
    }

    private class ExportAttributeRange(double minValue, double maxValue, double stepValue)
    {
        public double MinValue { get; } = minValue;
        public double MaxValue { get; } = maxValue;
        public double StepValue { get; } = stepValue;
    }
}

internal sealed class NumericControl(SpinBox spinBox) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value != null)
        {
            try
            {
                spinBox.Value = Convert.ToDouble(value);
            }
            catch (InvalidCastException)
            {
                // Handle the case where the value cannot be converted to double
                PrintUtils.Warning($"Cannot convert value of type {value.GetType()} to double.");
            }
        }
    }

    public Control Control => spinBox;

    public void SetEditable(bool editable)
    {
        spinBox.Editable = editable;
    }
}
#endif
