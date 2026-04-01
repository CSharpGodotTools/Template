namespace Template.OptionsGen;

/// <summary>
/// Supported option value kinds emitted by the options source generator.
/// </summary>
internal enum OptionValueKind
{
    Int,
    Float,
    String,
    Bool,
}

/// <summary>
/// Parsed option metadata used by generation and emission stages.
/// </summary>
/// <param name="saveKey">Persistent save key used at runtime.</param>
/// <param name="valueKind">Kind of option value.</param>
/// <param name="defaultLiteral">C# literal text for default value emission.</param>
internal sealed class OptionSettingSpec(string saveKey, OptionValueKind valueKind, string defaultLiteral)
{
    /// <summary>
    /// Persistent save key used by generated accessors.
    /// </summary>
    public string SaveKey { get; } = saveKey;

    /// <summary>
    /// Value kind of this option.
    /// </summary>
    public OptionValueKind ValueKind { get; } = valueKind;

    /// <summary>
    /// Default value literal emitted into generated code.
    /// </summary>
    public string DefaultLiteral { get; } = defaultLiteral;
}
