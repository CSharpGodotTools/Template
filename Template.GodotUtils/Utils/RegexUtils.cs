using System.Text.RegularExpressions;

namespace GodotUtils.RegEx;

/// <summary>
/// Centralized generated-regex definitions used across Godot utility helpers.
/// </summary>
public static partial class RegexUtils
{
    /// <summary>
    /// Matches a script path attribute within a Godot scene file.
    /// </summary>
    /// <returns>Compiled regex for locating script-path attributes.</returns>
    [GeneratedRegex(@"(?<=type=""Script""[^\n]*path="")[^""]*(?="")", RegexOptions.Multiline)]
    public static partial Regex ScriptPath();

    /// <summary>
    /// Splits a command line into parameters, honoring quotes.
    /// </summary>
    /// <returns>Compiled regex for tokenizing command arguments.</returns>
    [GeneratedRegex(@"[^\s""']+|""([^""]*)""|'([^']*)'")]
    public static partial Regex CommandParams();

    /// <summary>
    /// Matches an IPv4 address.
    /// </summary>
    /// <returns>Compiled regex for IPv4 address detection.</returns>
    [GeneratedRegex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")]
    public static partial Regex IpAddress();

    /// <summary>
    /// Matches alphanumeric text with spaces and commas.
    /// </summary>
    /// <returns>Compiled regex for alphanumeric text with spaces/commas.</returns>
    [GeneratedRegex(@"^[a-zA-Z0-9\s,]*$")]
    public static partial Regex AlphaNumericAndSpaces();
}
