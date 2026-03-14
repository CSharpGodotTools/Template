using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Validates custom option definitions before they are registered.
/// </summary>
internal static class OptionValidator
{
    /// <summary>
    /// Ensures the option label is not blank.
    /// </summary>
    public static void ValidateLabel(string label, string optionType)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException($"{optionType} label cannot be empty.");
    }

    /// <summary>
    /// Ensures the dropdown has at least one non‑blank item.
    /// </summary>
    public static void ValidateDropdownItems(IReadOnlyList<string> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Dropdown must define at least one item.");

        foreach (string item in items)
        {
            if (string.IsNullOrWhiteSpace(item))
                throw new ArgumentException("Dropdown items cannot be empty.");
        }
    }

    /// <summary>
    /// Ensures slider min &lt; max and step &gt; 0.
    /// </summary>
    public static void ValidateSliderRange(double minValue, double maxValue, double step)
    {
        if (maxValue <= minValue)
            throw new ArgumentException("Slider max value must be greater than min value.");

        if (step <= 0)
            throw new ArgumentException("Slider step must be greater than 0.");
    }
}
