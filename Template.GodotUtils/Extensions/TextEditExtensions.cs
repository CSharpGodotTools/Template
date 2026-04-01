using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Extension helpers for TextEdit input.
/// </summary>
public static class TextEditExtensions
{
    private static readonly Dictionary<ulong, string> _prevTexts = [];

    /// <summary>
    /// Filters text input by reverting to the last valid value.
    /// </summary>
    /// <param name="textEdit">Text editor whose input should be validated.</param>
    /// <param name="filter">Predicate used to validate candidate text.</param>
    /// <returns>Accepted text value, or <see langword="null"/> when no valid text exists.</returns>
    public static string? Filter(this TextEdit textEdit, Func<string, bool> filter)
    {
        string text = textEdit.Text;
        ulong id = textEdit.GetInstanceId();

        // Empty text reuses last valid value when available.
        if (string.IsNullOrWhiteSpace(text))
            return _prevTexts.TryGetValue(id, out string? value) ? value : null;

        // Revert invalid text to previous valid content.
        if (!filter(text))
        {
            // Restore previous text fallback when no cached value exists.
            if (!_prevTexts.TryGetValue(id, out string? value))
            {
                textEdit.ChangeTextEditText("");
                return null;
            }

            textEdit.ChangeTextEditText(value);
            return value;
        }

        _prevTexts[id] = text;
        return text;
    }

    /// <summary>
    /// Assigns text directly to the editor as part of the filter rollback flow.
    /// </summary>
    /// <param name="textEdit">Target text editor instance.</param>
    /// <param name="text">Text value to apply.</param>
    private static void ChangeTextEditText(this TextEdit textEdit, string text)
    {
        textEdit.Text = text;
        //textEdit.CaretColumn = text.Length;
    }
}
