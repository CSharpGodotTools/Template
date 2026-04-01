using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Runtime wrapper created after a <see cref="ToggleOptionDefinition"/>
/// is registered. Holds a stable ID and resolved getter/setter delegates.
/// </summary>
/// <param name="id">Stable option registration id.</param>
/// <param name="definition">Source toggle option definition.</param>
/// <param name="getValue">Delegate that reads current toggle state.</param>
/// <param name="setValue">Delegate that persists toggle state.</param>
internal sealed class RegisteredToggleOption(
    int id,
    ToggleOptionDefinition definition,
    Func<bool> getValue,
    Action<bool> setValue)
{
    /// <summary>
    /// Gets stable registration id.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    /// Gets source toggle definition.
    /// </summary>
    public ToggleOptionDefinition Definition { get; } = definition;

    /// <summary>
    /// Gets delegate that reads current toggle state.
    /// </summary>
    public Func<bool> GetValue { get; } = getValue;

    /// <summary>
    /// Gets delegate that persists toggle state.
    /// </summary>
    public Action<bool> SetValue { get; } = setValue;
}
