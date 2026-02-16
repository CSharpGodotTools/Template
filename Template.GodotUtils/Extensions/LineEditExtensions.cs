using Godot;
using System.Collections.Generic;
using System;

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
    public static string Filter(this LineEdit lineEdit, Func<string, bool> filter)
    {
        ulong id = lineEdit.GetInstanceId();

        if (!filter(lineEdit.Text))
        {
            string previousText = _prevTexts.TryGetValue(id, out string value) ? value : "";
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
    public static void ValidateNumber(this string value, LineEdit input, int min, int max, ref int prevNum)
    {
        // do NOT use text.Clear() as it will trigger _on_NumAttempts_text_changed and cause infinite loop -> stack overflow
        if (string.IsNullOrEmpty(value))
        {
            prevNum = 0;
            EditInputText(input, "");
            return;
        }

        if (!int.TryParse(value.Trim(), out int num))
        {
            EditInputText(input, $"{prevNum}");
            return;
        }

        if (value.Length > max.ToString().Length && num <= max)
        {
            string spliced = value[..^1];
            prevNum = int.Parse(spliced);
            EditInputText(input, spliced);
            return;
        }

        if (num < min)
        {
            num = min;
            EditInputText(input, $"{min}");
        }

        if (num > max)
        {
            num = max;
            EditInputText(input, $"{max}");
        }

        prevNum = num;
    }

    private static void EditInputText(LineEdit input, string text)
    {
        input.Text = text;
        input.CaretColumn = input.Text.Length;
    }
}
