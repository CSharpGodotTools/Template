using System;
using System.Collections.Generic;

namespace Template;

/// <summary>
/// Represents the context for a visual control
/// </summary>
public class VisualControlContext(List<VisualSpinBox> spinBoxes, object initialValue, Action<object> valueChanged)
{
    public List<VisualSpinBox> SpinBoxes { get; set; } = spinBoxes;
    public object InitialValue { get; set; } = initialValue;
    public Action<object> ValueChanged { get; set; } = valueChanged;
}
