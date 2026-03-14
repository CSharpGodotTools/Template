#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

internal sealed class MultiSpinBoxControl<T>(Control container, SpinBox[] spinBoxes, Func<T, double[]> getComponents) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is not T typedValue)
            return;

        double[] components = getComponents(typedValue);

        for (int i = 0; i < spinBoxes.Length && i < components.Length; i++)
        {
            spinBoxes[i].Value = components[i];
        }
    }

    public Control Control => container;

    public void SetEditable(bool editable)
    {
        foreach (SpinBox spinBox in spinBoxes)
        {
            spinBox.Editable = editable;
        }
    }
}
#endif
