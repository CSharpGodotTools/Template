using Godot;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GodotUtils.Debugging;

/// <summary>
/// Formats JSON parsing errors with nearby context lines.
/// </summary>
public static class JsonExceptionHandler
{
    private const int ContextLinesBefore = 7;
    private const int ContextLinesAfter = 8;

    /// <summary>
    /// Prints a formatted JSON parsing error with nearby source context when line data is available.
    /// </summary>
    /// <param name="ex">JSON exception describing the parse failure.</param>
    /// <param name="jsonText">Original JSON source text.</param>
    /// <param name="path">Path of the JSON resource being parsed.</param>
    public static void Handle(JsonException ex, string jsonText, string path)
    {
        long? lineNumber = ex.LineNumber;

        // Prefer context-rich output when the parser reports a source line number.
        if (lineNumber.HasValue)
        {
            string[] lines = jsonText.Split('\n');

            int lineIndex = (int)lineNumber.Value;
            lineIndex = Math.Clamp(lineIndex, 0, Math.Max(0, lines.Length - 1));
            string problematicLine = lines[lineIndex];

            int startLine = Math.Max(0, lineIndex - ContextLinesBefore);
            int endLine = Math.Min(lines.Length, lineIndex + ContextLinesAfter);

            StringBuilder errorMessage = new();

            errorMessage.AppendLine($"ERROR: Failed to parse {Path.GetFileName(path)}");
            errorMessage.AppendLine();
            errorMessage.AppendLine($"{ex.Message}");
            errorMessage.AppendLine();

            for (int i = startLine; i < lineIndex; i++)
                errorMessage.AppendLine(lines[i]);

            errorMessage.AppendLine($"{problematicLine} <--- Syntax error could be on this line or the next line");

            for (int i = lineIndex + 1; i < endLine; i++)
                errorMessage.AppendLine(lines[i]);

            GD.Print(errorMessage);
        }
        else
        {
            GD.Print($"ERROR: Failed to parse {Path.GetFileName(path)}");
            GD.Print(ex.Message);
        }
    }
}
