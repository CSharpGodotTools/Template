using System;

namespace Template.Setup.Testing;

public static class TestOutput
{
    
    private const int MinHeaderWidth = 40;
    private const int HeaderSideDashes = 10;

    public static void Header(string testName)
    {
        int minWidthForName = testName.Length + 2 + (HeaderSideDashes * 2);
        int width = Math.Max(MinHeaderWidth, minWidthForName);
        int extra = width - minWidthForName;
        int leftDashes = HeaderSideDashes + (extra / 2);
        int rightDashes = HeaderSideDashes + (extra - (extra / 2));

        string bar = new('-', width);
        string middle = $"{new string('-', leftDashes)} {testName} {new string('-', rightDashes)}";

        Console.WriteLine(bar);
        Console.WriteLine(middle);
        Console.WriteLine(bar);
    }

    public static void Step(string message)
    {
        Console.WriteLine($"  - {message}");
    }

    public static void Timing(string label, long ms)
    {
        Console.WriteLine($"{label}: {AnsiColors.Orange}{ms} ms{AnsiColors.Reset}");
    }

    public static void WriteMsInParens(long ms)
    {
        Console.Write($" ({AnsiColors.Orange}{ms} ms{AnsiColors.Reset})");
    }
}
