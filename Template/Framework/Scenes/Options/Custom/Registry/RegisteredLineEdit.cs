using System;

namespace Framework.Ui;

/// <summary>
/// Runtime wrapper created after a <see cref="LineEditOptionDefinition"/>
/// is registered. Holds a stable ID and resolved getter/setter delegates.
/// </summary>
internal sealed class RegisteredLineEditOption(
    int id,
    LineEditOptionDefinition definition,
    Func<string> getValue,
    Action<string> setValue)
{
    public int Id { get; } = id;
    public LineEditOptionDefinition Definition { get; } = definition;
    public Func<string> GetValue { get; } = getValue;
    public Action<string> SetValue { get; } = setValue;
}
