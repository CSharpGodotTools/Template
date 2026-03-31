namespace Template.OptionsGen;

internal sealed class OptionFactoryMetadata(OptionValueKind valueKind, int saveKeyIndex, int defaultValueIndex)
{
    private readonly OptionValueKind _valueKind = valueKind;
    private readonly int _saveKeyIndex = saveKeyIndex;
    private readonly int _defaultValueIndex = defaultValueIndex;

    public OptionValueKind ValueKind => _valueKind;
    public int SaveKeyIndex => _saveKeyIndex;
    public int DefaultValueIndex => _defaultValueIndex;
}
