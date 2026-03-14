using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Runtime wrapper created after a <see cref="DropdownOptionDefinition"/>
/// is registered. Holds a stable ID and resolved getter/setter delegates.
/// </summary>
internal sealed class RegisteredDropdownOption(
    int id,
    DropdownOptionDefinition definition,
    Func<int> getValue,
    Action<int> setValue)
{
    public int Id { get; } = id;
    public DropdownOptionDefinition Definition { get; } = definition;
    public Func<int> GetValue { get; } = getValue;
    public Action<int> SetValue { get; } = setValue;
}
