using Godot;
using System.Collections.Generic;
using System;

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
    public static string Filter(this TextEdit textEdit, Func<string, bool> filter)
    {
        string text = textEdit.Text;
        ulong id = textEdit.GetInstanceId();

        if (string.IsNullOrWhiteSpace(text))
            return _prevTexts.TryGetValue(id, out string value) ? value : null;

        if (!filter(text))
        {
            if (!_prevTexts.TryGetValue(id, out string value))
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

    private static void ChangeTextEditText(this TextEdit textEdit, string text)
    {
        textEdit.Text = text;
        //textEdit.CaretColumn = text.Length;
    }
}
