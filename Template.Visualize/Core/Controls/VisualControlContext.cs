#if DEBUG
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Carries initial value and change callback for visual control creation.
/// </summary>
/// <param name="initialValue">Initial value shown by the control.</param>
/// <param name="valueChanged">Callback invoked when the control value changes.</param>
internal sealed class VisualControlContext(object? initialValue, Action<object> valueChanged)
{
    /// <summary>
    /// Initial value shown by the control.
    /// </summary>
    public object? InitialValue { get; } = initialValue;

    /// <summary>
    /// Callback invoked when the control emits a new value.
    /// </summary>
    public Action<object> ValueChanged { get; } = valueChanged ?? throw new ArgumentNullException(nameof(valueChanged));
}
#endif
