namespace Template.OptionsGen;

internal enum OptionValueKind
{
    Int,
    Float,
    String,
    Bool,
}

internal sealed class OptionSettingSpec
{
    public OptionSettingSpec(string saveKey, OptionValueKind valueKind, string defaultLiteral)
    {
        SaveKey = saveKey;
        ValueKind = valueKind;
        DefaultLiteral = defaultLiteral;
    }

    public string SaveKey { get; }
    public OptionValueKind ValueKind { get; }
    public string DefaultLiteral { get; }
}
