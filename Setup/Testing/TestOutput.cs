using System;

namespace Template.Setup.Testing;

public static class TestOutput
{
    private const string AnsiOrange = "\u001b[38;2;255;165;0m";
    private const string AnsiReset = "\u001b[0m";
    private const int MinHeaderWidth = 40;
    private const int HeaderSideDashes = 10;

    public static void Header(string testName)
    {
        int minWidthForName = testName.Length + 2 + (HeaderSideDashes * 2);
        int width = Math.Max(MinHeaderWidth, minWidthForName);
        int extra = width - minWidthForName;
        int leftDashes = HeaderSideDashes + (extra / 2);
        int rightDashes = HeaderSideDashes + (extra - (extra / 2));

        string bar = new string('-', width);
        string middle = $"{new string('-', leftDashes)} {testName} {new string('-', rightDashes)}";

        Console.WriteLine(bar);
        Console.WriteLine(middle);
        Console.WriteLine(bar);
    }

    public static void Footer()
    {
    }

    public static void Step(string message)
    {
        Console.WriteLine($"  - {message}");
    }

    public static void Timing(string label, long ms)
    {
        Console.WriteLine($"{label}: {AnsiOrange}{ms} ms{AnsiReset}");
    }

    public static void WriteMsInParens(long ms)
    {
        Console.Write($" ({AnsiOrange}{ms} ms{AnsiReset})");
    }
}
