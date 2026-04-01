using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Runtime wrapper created after a <see cref="LineEditOptionDefinition"/>
/// is registered. Holds a stable ID and resolved getter/setter delegates.
/// </summary>
/// <param name="id">Stable option registration id.</param>
/// <param name="definition">Source line-edit option definition.</param>
/// <param name="getValue">Delegate that reads current text.</param>
/// <param name="setValue">Delegate that persists text values.</param>
internal sealed class RegisteredLineEditOption(
    int id,
    LineEditOptionDefinition definition,
    Func<string> getValue,
    Action<string> setValue)
{
    /// <summary>
    /// Gets stable registration id.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    /// Gets source line-edit definition.
    /// </summary>
    public LineEditOptionDefinition Definition { get; } = definition;

    /// <summary>
    /// Gets delegate that reads current text.
    /// </summary>
    public Func<string> GetValue { get; } = getValue;

    /// <summary>
    /// Gets delegate that persists text values.
    /// </summary>
    public Action<string> SetValue { get; } = setValue;
}
