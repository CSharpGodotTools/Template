namespace Template.OptionsGen;

internal static class OptionValueKindNaming
{
    public static string GetTypeKeyword(OptionValueKind kind)
    {
        switch (kind)
        {
            case OptionValueKind.Int:
                return "int";
            case OptionValueKind.Float:
                return "float";
            case OptionValueKind.String:
                return "string";
            case OptionValueKind.Bool:
                return "bool";
            default:
                return "object";
        }
    }

    public static string GetGetterName(OptionValueKind kind)
    {
        switch (kind)
        {
            case OptionValueKind.Int:
                return "GetInt";
            case OptionValueKind.Float:
                return "GetFloat";
            case OptionValueKind.String:
                return "GetString";
            case OptionValueKind.Bool:
                return "GetBool";
            default:
                return "GetString";
        }
    }

    public static string GetSetterName(OptionValueKind kind)
    {
        switch (kind)
        {
            case OptionValueKind.Int:
                return "SetInt";
            case OptionValueKind.Float:
                return "SetFloat";
            case OptionValueKind.String:
                return "SetString";
            case OptionValueKind.Bool:
                return "SetBool";
            default:
                return "SetString";
        }
    }
}
