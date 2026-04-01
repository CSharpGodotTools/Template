#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Visual-control adapter for composite numeric types backed by multiple spin boxes.
/// </summary>
/// <typeparam name="T">Composite value type (for example vectors/quaternions).</typeparam>
/// <param name="container">Root container hosting spin boxes.</param>
/// <param name="spinBoxes">Spin boxes that edit individual components.</param>
/// <param name="getComponents">Function that extracts component values from <typeparamref name="T"/>.</param>
internal sealed class MultiSpinBoxControl<T>(Control container, SpinBox[] spinBoxes, Func<T, double[]> getComponents) : IVisualControl
{
    /// <summary>
    /// Updates spin-box values from the supplied composite value.
    /// </summary>
    /// <param name="value">Composite value to display.</param>
    public void SetValue(object value)
    {
        // Ignore updates that are not the expected composite value type.
        if (value is not T typedValue)
            return;

        double[] components = getComponents(typedValue);

        for (int i = 0; i < spinBoxes.Length && i < components.Length; i++)
        {
            spinBoxes[i].Value = components[i];
        }
    }

    /// <summary>
    /// Root container for this composite control.
    /// </summary>
    public Control Control => container;

    /// <summary>
    /// Enables or disables all component spin boxes.
    /// </summary>
    /// <param name="editable">True to allow edits.</param>
    public void SetEditable(bool editable)
    {
        foreach (SpinBox spinBox in spinBoxes)
        {
            spinBox.Editable = editable;
        }
    }
}
#endif
