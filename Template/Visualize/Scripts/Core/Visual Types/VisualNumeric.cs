using Godot;
using GodotUtils;
using System;
using System.Linq;
using System.Reflection;

namespace Template;

public static partial class VisualControlTypes
{
    private const double DEFAULT_STEP_VALUE = 0.005;
    private const float MIN_SLIDER_WIDTH = 200;

    private static VisualControlInfo VisualNumeric(Type type, MemberInfo memberInfo, VisualControlContext context)
    {
        Godot.Range control;

        if (memberInfo != null && TryGetExportAttributeRange(memberInfo, out ExportAttributeRange range))
        {
            control = new HSlider()
            {
                CustomMinimumSize = new Vector2(MIN_SLIDER_WIDTH, 0),
                MinValue = range.MinValue,
                MaxValue = range.MaxValue,
                Step = range.StepValue,
            };
        }
        else
        {
            control = CreateSpinBox(type, memberInfo);
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

        string[] args = exportAttribute.HintString.Split(',').Select(x => x.Trim()).ToArray();

        if (args.Length < 2)
            return false;

        if (!double.TryParse(args[0], out double minValue))
            return false;

        if (!double.TryParse(args[1], out double maxValue))
            return false;

        double stepValue = DEFAULT_STEP_VALUE;

        if (args.Length >= 3 && double.TryParse(args[2].Trim(), out double parsedStepValue))
        {
            stepValue = parsedStepValue;
        }

        exportAttributeRange = new ExportAttributeRange(minValue, maxValue, stepValue);

        return true;
    }

    private class ExportAttributeRange(double minValue, double maxValue, double stepValue)
    {
        public double MinValue { get; set; } = minValue;
        public double MaxValue { get; set; } = maxValue;
        public double StepValue { get; set; } = stepValue;
    }
}

public class NumericControl(Godot.Range spinBox) : IVisualControl
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
        // There is no Editable property for Range so we instead use SetAllowGreater and SetAllowLesser
        spinBox.SetAllowGreater(editable);
        spinBox.SetAllowLesser(editable);
    }
}
