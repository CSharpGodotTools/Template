using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Runtime wrapper created after a <see cref="SliderOptionDefinition"/>
/// is registered. Holds a stable ID and resolved getter/setter delegates.
/// </summary>
internal sealed class RegisteredSliderOption(
    int id,
    SliderOptionDefinition definition,
    Func<float> getValue,
    Action<float> setValue)
{
    public int Id { get; } = id;
    public SliderOptionDefinition Definition { get; } = definition;
    public Func<float> GetValue { get; } = getValue;
    public Action<float> SetValue { get; } = setValue;
}
