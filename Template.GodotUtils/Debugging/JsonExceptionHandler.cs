using Godot;
using System.IO;
using System.Text;
using System;
using System.Text.Json;

namespace GodotUtils.Debugging;

/// <summary>
/// Formats JSON parsing errors with nearby context lines.
/// </summary>
public class JsonExceptionHandler
{
    /// <summary>
    /// Prints a formatted JSON parsing error with context.
    /// </summary>
    public static void Handle(JsonException ex, string jsonText, string path)
    {
        // Extract relevant information from the exception
        long? lineNumber = ex.LineNumber;

        if (lineNumber.HasValue)
        {
            // Split the JSON into lines
            string[] lines = jsonText.Split('\n');

            // Get the problematic line
            int lineIndex = (int)lineNumber.Value;
            lineIndex = Math.Clamp(lineIndex, 0, Math.Max(0, lines.Length - 1));
            string problematicLine = lines[lineIndex];

            // Determine the range of lines to display
            int startLine = Math.Max(0, lineIndex - 7);
            int endLine = Math.Min(lines.Length, lineIndex + 8);

            // Create the error message
            StringBuilder errorMessage = new();

            errorMessage.AppendLine($"ERROR: Failed to parse {Path.GetFileName(path)}");
            errorMessage.AppendLine();
            errorMessage.AppendLine($"{ex.Message}");
            errorMessage.AppendLine();

            // Add the lines before the problematic line
            for (int i = startLine; i < lineIndex; i++)
            {
                errorMessage.AppendLine(lines[i]);
            }

            // Add the problematic line with the caret indicating the error position
            errorMessage.AppendLine($"{problematicLine} <--- Syntax error could be on this line or the next line");

            // Add the lines after the problematic line
            for (int i = lineIndex + 1; i < endLine; i++)
            {
                errorMessage.AppendLine(lines[i]);
            }

            GD.Print(errorMessage);
        }
        else
        {
            GD.Print($"ERROR: Failed to parse {Path.GetFileName(path)}");
            GD.Print(ex.Message);
        }
    }
}
