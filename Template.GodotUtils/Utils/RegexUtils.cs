using System.Text.RegularExpressions;

namespace GodotUtils.RegEx;

public static partial class RegexUtils
{
    /// <summary>
    /// Matches a script path attribute within a Godot scene file.
    /// </summary>
    [GeneratedRegex(@"(?<=type=""Script""[^\n]*path="")[^""]*(?="")", RegexOptions.Multiline)]
    public static partial Regex ScriptPath();

    /// <summary>
    /// Splits a command line into parameters, honoring quotes.
    /// </summary>
    [GeneratedRegex(@"[^\s""']+|""([^""]*)""|'([^']*)'")]
    public static partial Regex CommandParams();

    /// <summary>
    /// Matches an IPv4 address.
    /// </summary>
    [GeneratedRegex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")]
    public static partial Regex IpAddress();

    /// <summary>
    /// Matches alphanumeric text with spaces and commas.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z0-9\s,]*$")]
    public static partial Regex AlphaNumericAndSpaces();
}
