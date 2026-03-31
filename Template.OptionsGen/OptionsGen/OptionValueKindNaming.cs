namespace Template.OptionsGen;

internal static class OptionValueKindNaming
{
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
