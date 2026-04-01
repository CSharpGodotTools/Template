using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Runtime wrapper created after a <see cref="DropdownOptionDefinition"/>
/// is registered. Holds a stable ID and resolved getter/setter delegates.
/// </summary>
/// <param name="id">Stable option registration id.</param>
/// <param name="definition">Source dropdown option definition.</param>
/// <param name="getValue">Delegate that reads current selected index.</param>
/// <param name="setValue">Delegate that persists selected index.</param>
internal sealed class RegisteredDropdownOption(
    int id,
    DropdownOptionDefinition definition,
    Func<int> getValue,
    Action<int> setValue)
{
    /// <summary>
    /// Gets stable registration id.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    /// Gets source dropdown definition.
    /// </summary>
    public DropdownOptionDefinition Definition { get; } = definition;

    /// <summary>
    /// Gets delegate that reads current selected index.
    /// </summary>
    public Func<int> GetValue { get; } = getValue;

    /// <summary>
    /// Gets delegate that persists selected index.
    /// </summary>
    public Action<int> SetValue { get; } = setValue;
}
