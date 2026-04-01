using System;

namespace Template.Setup.Testing;

/// <summary>
/// Provides standardized console output helpers for integration tests.
/// </summary>
public static class TestOutput
{
    private const int MinHeaderWidth = 40;
    private const int HeaderSideDashes = 10;

    /// <summary>
    /// Writes a framed test header line for improved log readability.
    /// </summary>
    /// <param name="testName">Test name displayed in the header.</param>
    public static void Header(string testName)
    {
        int minWidthForName = testName.Length + 2 + (HeaderSideDashes * 2);
        int width = Math.Max(MinHeaderWidth, minWidthForName);
        int extra = width - minWidthForName;

        // Split extra dashes so header text stays centered.
        int leftDashes = HeaderSideDashes + (extra / 2);
        int rightDashes = HeaderSideDashes + (extra - (extra / 2));

        string bar = new('-', width);
        string middle = $"{new string('-', leftDashes)} {testName} {new string('-', rightDashes)}";

        Console.WriteLine(bar);
        Console.WriteLine(middle);
        Console.WriteLine(bar);
    }

    /// <summary>
    /// Writes a formatted step line within a test output block.
    /// </summary>
    /// <param name="message">Step text to display.</param>
    public static void Step(string message)
    {
        Console.WriteLine($"  - {message}");
    }

    /// <summary>
    /// Writes a labeled elapsed-time value in milliseconds.
    /// </summary>
    /// <param name="label">Label displayed before the timing value.</param>
    /// <param name="ms">Elapsed milliseconds.</param>
    public static void Timing(string label, long ms)
    {
        Console.WriteLine($"{label}: {ms} ms");
    }

    /// <summary>
    /// Writes milliseconds in parenthesized form without a trailing newline.
    /// </summary>
    /// <param name="ms">Elapsed milliseconds.</param>
    public static void WriteMsInParens(long ms)
    {
        Console.Write($" ({ms} ms)");
    }
}
