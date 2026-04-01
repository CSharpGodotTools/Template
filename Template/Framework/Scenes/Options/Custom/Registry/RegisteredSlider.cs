using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Runtime wrapper created after a <see cref="SliderOptionDefinition"/>
/// is registered. Holds a stable ID and resolved getter/setter delegates.
/// </summary>
/// <param name="id">Stable option registration id.</param>
/// <param name="definition">Source slider option definition.</param>
/// <param name="getValue">Delegate that reads current slider value.</param>
/// <param name="setValue">Delegate that persists slider values.</param>
internal sealed class RegisteredSliderOption(
    int id,
    SliderOptionDefinition definition,
    Func<float> getValue,
    Action<float> setValue)
{
    /// <summary>
    /// Gets stable registration id.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    /// Gets source slider definition.
    /// </summary>
    public SliderOptionDefinition Definition { get; } = definition;

    /// <summary>
    /// Gets delegate that reads current slider value.
    /// </summary>
    public Func<float> GetValue { get; } = getValue;

    /// <summary>
    /// Gets delegate that persists slider values.
    /// </summary>
    public Action<float> SetValue { get; } = setValue;
}
