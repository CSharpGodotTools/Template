namespace Template.OptionsGen;

/// <summary>
/// Shared constant values used across options generator parsing and emission stages.
/// </summary>
internal static class OptionsGenConstants
{
    /// <summary>
    /// Diagnostic category used by options generator diagnostics.
    /// </summary>
    public const string GeneratorCategory = "Template.OptionsGen";

    /// <summary>
    /// Expected simple name of the options factory type.
    /// </summary>
    public const string OptionDefinitionsTypeName = "OptionDefinitions";

    /// <summary>
    /// Qualified type-name suffix for options factory resolution.
    /// </summary>
    public const string OptionDefinitionsQualifiedSuffix = ".Ui.OptionDefinitions";

    /// <summary>
    /// Parameter name used for save-key extraction.
    /// </summary>
    public const string SaveKeyParameterName = "saveKey";

    /// <summary>
    /// Parameter name used for default-value extraction.
    /// </summary>
    public const string DefaultValueParameterName = "defaultValue";

    /// <summary>
    /// Target settings type name augmented by generated properties.
    /// </summary>
    public const string OptionsSettingsTypeName = "OptionsSettings";

    /// <summary>
    /// Hint name used when adding generated source to compilation.
    /// </summary>
    public const string GeneratedSourceHintName = "OptionsSettings.Properties.g.cs";
}
