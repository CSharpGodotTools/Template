using GodotUtils.RegEx;
using System;
using System.Globalization;
using System.Linq;

namespace GodotUtils;

/// <summary>
/// Extension helpers for strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns true if the string looks like an IP address or "localhost".
    /// </summary>
    /// <param name="v">Input string to evaluate.</param>
    /// <returns><see langword="true"/> when the string matches an address pattern.</returns>
    public static bool IsAddress(this string v)
    {
        return v != null && (RegexUtils.IpAddress().IsMatch(v) || v.Contains("localhost"));
    }

    /// <summary>
    /// Converts "helloWorld" into "hello World".
    /// </summary>
    /// <param name="v">Input camel/pascal case string.</param>
    /// <returns>String with spaces inserted before capital letters.</returns>
    public static string AddSpaceBeforeEachCapital(this string v)
    {
        return string.Concat(v.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
    }

    /// <summary>
    /// Returns true when the string contains only letters or digits.
    /// </summary>
    /// <param name="v">Input string to evaluate.</param>
    /// <returns><see langword="true"/> when all characters are alphanumeric.</returns>
    public static bool IsAlphaNumeric(this string v)
    {
        return v.All(char.IsLetterOrDigit);
    }

    /// <summary>
    /// Returns true when the string contains only letters.
    /// </summary>
    /// <param name="v">Input string to evaluate.</param>
    /// <returns><see langword="true"/> when all characters are letters.</returns>
    public static bool IsAlphaOnly(this string v)
    {
        return v.All(char.IsLetter);
    }

    /// <summary>
    /// Returns true when the string contains only digits.
    /// </summary>
    /// <param name="v">Input string to evaluate.</param>
    /// <returns><see langword="true"/> when all characters are numeric.</returns>
    public static bool IsNumericOnly(this string v)
    {
        return v.All(char.IsDigit);
    }

    /// <summary>
    /// Converts "hello world" into "Hello World".
    /// </summary>
    /// <param name="v">Input string to convert.</param>
    /// <returns>Title-cased string using the current culture.</returns>
    public static string ToTitleCase(this string v)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(v.ToLower());
    }

    /// <summary>
    /// Uppercases words with length less than or equal to <paramref name="maxLength"/>.
    /// </summary>
    /// <param name="v">Input string whose words may be transformed.</param>
    /// <param name="maxLength">Maximum word length that will be uppercased.</param>
    /// <param name="filter">Optional predicate for deciding whether a small word should be uppercased.</param>
    /// <returns>Input string with selected short words uppercased.</returns>
    public static string SmallWordsToUpper(this string v, int maxLength = 2, Func<string, bool>? filter = null)
    {
        string[] words = v.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            // Uppercase small words that satisfy optional filter.
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
    /// <param name="input">Input string to transform.</param>
    /// <returns>String with the first character converted to upper case.</returns>
    public static string FirstCharToUpper(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Preserve empty input.
        if (input is "")
            return input;

        return string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));
    }
}
