using System.Globalization;
using System.Linq;
using System;
using GodotUtils.RegEx;

namespace GodotUtils;

/// <summary>
/// Extension helpers for strings.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Returns true if the string looks like an IP address or "localhost".
    /// </summary>
    public static bool IsAddress(this string v)
    {
        return v != null && (RegexUtils.IpAddress().IsMatch(v) || v.Contains("localhost"));
    }

    /// <summary>
    /// Converts "helloWorld" into "hello World".
    /// </summary>
    public static string AddSpaceBeforeEachCapital(this string v)
    {
        return string.Concat(v.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
    }

    /// <summary>
    /// Returns true when the string contains only letters or digits.
    /// </summary>
    public static bool IsAlphaNumeric(this string v)
    {
        return v.All(char.IsLetterOrDigit);
    }

    /// <summary>
    /// Returns true when the string contains only letters.
    /// </summary>
    public static bool IsAlphaOnly(this string v)
    {
        return v.All(char.IsLetter);
    }

    /// <summary>
    /// Returns true when the string contains only digits.
    /// </summary>
    public static bool IsNumericOnly(this string v)
    {
        return v.All(char.IsDigit);
    }

    /// <summary>
    /// Converts "hello world" into "Hello World".
    /// </summary>
    public static string ToTitleCase(this string v)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(v.ToLower());
    }

    /// <summary>
    /// Uppercases words with length less than or equal to <paramref name="maxLength"/>.
    /// </summary>
    public static string SmallWordsToUpper(this string v, int maxLength = 2, Func<string, bool> filter = null)
    {
        string[] words = v.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length <= maxLength && (filter == null || filter(words[i])))
            {
                words[i] = words[i].ToUpper();
            }
        }

        return string.Join(" ", words);
    }

    /// <summary>
    /// Uppercases the first character in the string.
    /// </summary>
    public static string FirstCharToUpper(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input is "")
            return input;

        return string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));
    }
}
