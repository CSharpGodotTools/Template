namespace Template.OptionsGen;

/// <summary>
/// Converts <see cref="OptionValueKind"/> values into C# type and method identifiers used by generated option code.
/// </summary>
internal static class OptionValueKindNaming
{
    /// <summary>
    /// Maps an option value kind to the C# keyword emitted for generated settings properties.
    /// Unknown kinds fall back to <c>object</c> so generation remains compilable.
    /// </summary>
    /// <param name="kind">Option value kind from the parsed option specification.</param>
    /// <returns>C# type keyword used in emitted source.</returns>
    public static string GetTypeKeyword(OptionValueKind kind)
    {
        return kind switch
        {
            OptionValueKind.Int => "int",
            OptionValueKind.Float => "float",
            OptionValueKind.String => "string",
            OptionValueKind.Bool => "bool",
            _ => "object",
        };
    }

    /// <summary>
    /// Maps an option value kind to the getter method name expected on the runtime settings store.
    /// Unknown kinds default to <c>GetString</c> as a conservative fallback.
    /// </summary>
    /// <param name="kind">Option value kind from the parsed option specification.</param>
    /// <returns>Getter name emitted into generated source.</returns>
    public static string GetGetterName(OptionValueKind kind)
    {
        return kind switch
        {
            OptionValueKind.Int => "GetInt",
            OptionValueKind.Float => "GetFloat",
            OptionValueKind.String => "GetString",
            OptionValueKind.Bool => "GetBool",
            _ => "GetString",
        };
    }

    /// <summary>
    /// Maps an option value kind to the setter method name expected on the runtime settings store.
    /// Unknown kinds default to <c>SetString</c> to align with the getter fallback behavior.
    /// </summary>
    /// <param name="kind">Option value kind from the parsed option specification.</param>
    /// <returns>Setter name emitted into generated source.</returns>
    public static string GetSetterName(OptionValueKind kind)
    {
        return kind switch
        {
            OptionValueKind.Int => "SetInt",
            OptionValueKind.Float => "SetFloat",
            OptionValueKind.String => "SetString",
            OptionValueKind.Bool => "SetBool",
            _ => "SetString",
        };
    }
}
