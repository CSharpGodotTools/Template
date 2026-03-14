using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Runtime wrapper created after a <see cref="ToggleOptionDefinition"/>
/// is registered. Holds a stable ID and resolved getter/setter delegates.
/// </summary>
internal sealed class RegisteredToggleOption(
    int id,
    ToggleOptionDefinition definition,
    Func<bool> getValue,
    Action<bool> setValue)
{
    public int Id { get; } = id;
    public ToggleOptionDefinition Definition { get; } = definition;
    public Func<bool> GetValue { get; } = getValue;
    public Action<bool> SetValue { get; } = setValue;
}
