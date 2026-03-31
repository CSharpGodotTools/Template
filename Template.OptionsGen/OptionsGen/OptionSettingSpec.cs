namespace Template.OptionsGen;

internal enum OptionValueKind
{
    Int,
    Float,
    String,
    Bool,
}

internal sealed class OptionSettingSpec(string saveKey, OptionValueKind valueKind, string defaultLiteral)
{
    public string SaveKey { get; } = saveKey;
    public OptionValueKind ValueKind { get; } = valueKind;
    public string DefaultLiteral { get; } = defaultLiteral;
}
