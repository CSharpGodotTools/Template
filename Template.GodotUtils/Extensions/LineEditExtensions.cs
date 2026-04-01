using Godot;
using System;
using System.Collections.Generic;

namespace GodotUtils;

/// <summary>
/// Extension helpers for LineEdit input.
/// </summary>
public static class LineEditExtensions
{
    private static readonly Dictionary<ulong, string> _prevTexts = [];

    /// <summary>
    /// Filters text input by reverting to the last valid value.
    /// </summary>
    /// <param name="lineEdit">Input control whose text should be validated.</param>
    /// <param name="filter">Predicate that returns whether a candidate text value is valid.</param>
    /// <returns>The accepted text value after filtering.</returns>
    public static string Filter(this LineEdit lineEdit, Func<string, bool> filter)
    {
        ulong id = lineEdit.GetInstanceId();

        // Revert invalid input to last valid cached text.
        if (!filter(lineEdit.Text))
        {
            string? previousText = _prevTexts.TryGetValue(id, out string? value) ? value : "";
            lineEdit.Text = previousText;
            lineEdit.CaretColumn = previousText.Length;
            return previousText;
        }

        _prevTexts[id] = lineEdit.Text;

        return lineEdit.Text;
    }

    /// <summary>
    /// Validates numeric input and clamps it between <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="value">Raw text value to parse.</param>
    /// <param name="input">Line edit control whose text will be normalized.</param>
    /// <param name="min">Minimum allowed numeric value.</param>
    /// <param name="max">Maximum allowed numeric value.</param>
    /// <param name="prevNum">Previously accepted value used when parsing fails.</param>
    public static void ValidateNumber(this string value, LineEdit input, int min, int max, ref int prevNum)
    {
        // do NOT use text.Clear() as it will trigger _on_NumAttempts_text_changed and cause infinite loop -> stack overflow
        // Empty input resets the tracked value.
        if (string.IsNullOrEmpty(value))
        {
            prevNum = 0;
            EditInputText(input, "");
            return;
        }

        // Reject non-integer input and restore previous value.
        if (!int.TryParse(value.Trim(), out int num))
        {
            EditInputText(input, $"{prevNum}");
            return;
        }

        // Trim extra characters when value length exceeds max-length shape.
        if (value.Length > max.ToString().Length && num <= max)
        {
            string spliced = value[..^1];
            prevNum = int.Parse(spliced);
            EditInputText(input, spliced);
            return;
        }

        // Clamp lower bound.
        if (num < min)
        {
            num = min;
            EditInputText(input, $"{min}");
        }

        // Clamp upper bound.
        if (num > max)
        {
            num = max;
            EditInputText(input, $"{max}");
        }

        prevNum = num;
    }

    /// <summary>
    /// Updates input text and keeps the caret anchored at the end.
    /// </summary>
    /// <param name="input">Target line edit control.</param>
    /// <param name="text">Text to assign to the control.</param>
    private static void EditInputText(LineEdit input, string text)
    {
        input.Text = text;
        input.CaretColumn = input.Text.Length;
    }
}
