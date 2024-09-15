﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Visualize.Core;

public class VisualControlContext(List<VisualSpinBox> spinBoxes, object initialValue, Action<object> valueChanged)
{
    public List<VisualSpinBox> SpinBoxes { get; set; } = spinBoxes;
    public object InitialValue { get; set; } = initialValue;
    public Action<object> ValueChanged { get; set; } = valueChanged;
}

