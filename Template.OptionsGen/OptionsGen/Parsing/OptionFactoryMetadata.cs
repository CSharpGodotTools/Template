namespace Template.OptionsGen;

/// <summary>
/// Describes how to extract option specification data from a specific factory method shape.
/// </summary>
/// <param name="valueKind">Option value kind produced by the factory method.</param>
/// <param name="saveKeyIndex">Positional index of the save-key argument.</param>
/// <param name="defaultValueIndex">Positional index of the default-value argument.</param>
internal sealed class OptionFactoryMetadata(OptionValueKind valueKind, int saveKeyIndex, int defaultValueIndex)
{
    private readonly OptionValueKind _valueKind = valueKind;
    private readonly int _saveKeyIndex = saveKeyIndex;
    private readonly int _defaultValueIndex = defaultValueIndex;

    /// <summary>
    /// Option value kind produced by the factory method.
    /// </summary>
    public OptionValueKind ValueKind => _valueKind;

    /// <summary>
    /// Zero-based index of the save-key argument in positional factory calls.
    /// </summary>
    public int SaveKeyIndex => _saveKeyIndex;

    /// <summary>
    /// Zero-based index of the default-value argument in positional factory calls.
    /// </summary>
    public int DefaultValueIndex => _defaultValueIndex;
}
