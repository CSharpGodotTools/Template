using Godot;

namespace GodotUtils;

/// <summary>
/// Creates a configured code highlighter instance.
/// </summary>
public static class CodeHighlighterFactory
{
    private const string _pink = "ffb6ff";
    private const string _lavenderGray = "a8b1d6";
    private const string _lightPurple = "b988ff";
    private const string _periwinkle = "becaf5";
    private const string _darkGray = "434048";
    private const string _lightLilac = "a59fff";

    /// <summary>
    /// Builds a default highlighter with keyword and token colors.
    /// </summary>
    public static CodeHighlighter Create()
    {
        CodeHighlighter editor = new()
        {
            KeywordColors       = [],
            NumberColor         = new Color(_pink),
            SymbolColor         = new Color(_lavenderGray),
            FunctionColor       = new Color(_lightPurple),
            MemberVariableColor = new Color(_periwinkle),
            ColorRegions        = new Godot.Collections.Dictionary
            {
                { "//", new Color(_darkGray) }
            },
        };

        string[] keywords = ["var", "true", "false", "new", "private", "public", "protected", "internal", "void"];

        foreach (string keyword in keywords)
        {
            editor.KeywordColors.Add(keyword, new Color(_lightLilac));
        }

        return editor;
    }
}
