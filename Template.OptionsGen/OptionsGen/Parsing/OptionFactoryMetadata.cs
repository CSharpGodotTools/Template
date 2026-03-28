namespace Template.OptionsGen;

internal sealed class OptionFactoryMetadata
{
    private readonly OptionValueKind _valueKind;
    private readonly int _saveKeyIndex;
    private readonly int _defaultValueIndex;

    public OptionFactoryMetadata(OptionValueKind valueKind, int saveKeyIndex, int defaultValueIndex)
    {
        _valueKind = valueKind;
        _saveKeyIndex = saveKeyIndex;
        _defaultValueIndex = defaultValueIndex;
    }

    public OptionValueKind ValueKind => _valueKind;
    public int SaveKeyIndex => _saveKeyIndex;
    public int DefaultValueIndex => _defaultValueIndex;
}
