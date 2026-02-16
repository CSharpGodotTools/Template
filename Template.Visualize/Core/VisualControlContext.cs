#if DEBUG
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Represents the context for a visual control
/// </summary>
internal sealed class VisualControlContext(object initialValue, Action<object> valueChanged)
{
    public object InitialValue { get; } = initialValue;
    public Action<object> ValueChanged { get; } = valueChanged ?? throw new ArgumentNullException(nameof(valueChanged));
}
#endif
