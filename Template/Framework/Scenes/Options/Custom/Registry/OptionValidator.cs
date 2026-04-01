using System;
using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Validates custom option definitions before they are registered.
/// </summary>
internal static class OptionValidator
{
    /// <summary>
    /// Ensures the option tab name is not blank.
    /// </summary>
    /// <param name="tab">Tab name to validate.</param>
    public static void ValidateTab(string tab)
    {
        // Option tabs must provide a non-empty display name.
        if (string.IsNullOrWhiteSpace(tab))
            throw new ArgumentException("Option tab cannot be empty.");
    }

    /// <summary>
    /// Ensures the option label is not blank.
    /// </summary>
    /// <param name="label">Option label to validate.</param>
    /// <param name="optionType">Option type name used in error messages.</param>
    public static void ValidateLabel(string label, string optionType)
    {
        // Option labels must contain visible text for UI rendering.
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException($"{optionType} label cannot be empty.");
    }

    /// <summary>
    /// Ensures the dropdown has at least one non‑blank item.
    /// </summary>
    /// <param name="items">Dropdown item list to validate.</param>
    public static void ValidateDropdownItems(IReadOnlyList<string> items)
    {
        // Dropdowns must provide at least one selectable item.
        if (items == null || items.Count == 0)
            throw new ArgumentException("Dropdown must define at least one item.");

        foreach (string item in items)
        {
            // Reject empty entries so every dropdown row is user-visible.
            if (string.IsNullOrWhiteSpace(item))
                throw new ArgumentException("Dropdown items cannot be empty.");
        }
    }

    /// <summary>
    /// Ensures slider min &lt; max and step &gt; 0.
    /// </summary>
    /// <param name="minValue">Minimum slider value.</param>
    /// <param name="maxValue">Maximum slider value.</param>
    /// <param name="step">Slider step size.</param>
    public static void ValidateSliderRange(double minValue, double maxValue, double step)
    {
        // Slider upper bound must be strictly greater than lower bound.
        if (maxValue <= minValue)
            throw new ArgumentException("Slider max value must be greater than min value.");

        // Slider step must advance the value by a positive increment.
        if (step <= 0)
            throw new ArgumentException("Slider step must be greater than 0.");
    }
}
