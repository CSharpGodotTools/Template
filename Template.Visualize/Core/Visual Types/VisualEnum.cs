#if DEBUG
using Godot;
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Enum visual-control builders.
/// </summary>
internal static partial class VisualControlTypes
{
    /// <summary>
    /// Creates an option-button control for enum values.
    /// </summary>
    /// <param name="type">Enum type to visualize.</param>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created enum-control info.</returns>
    private static VisualControlInfo VisualEnum(Type type, VisualControlContext context)
    {
        // Guard against non-enum types.
        if (!type.IsEnum)
            throw new ArgumentException("type must be an enum");

        OptionButton optionButton = new()
        {
            Alignment = HorizontalAlignment.Center
        };

        // Populate the option list from enum values.
        foreach (object item in Enum.GetValues(type))
        {
            optionButton.AddItem(VisualText.ToSpacedName(item.ToString()!));
        }

        void OnItemSelected(long index)
        {
            object? selectedValue = Enum.GetValues(type).GetValue(index);
            context.ValueChanged(selectedValue!);
            optionButton.ReleaseFocus();
        }

        optionButton.ItemSelected += OnItemSelected;
        CleanupOnTreeExited(optionButton, () => optionButton.ItemSelected -= OnItemSelected);

        void Select(object value)
        {
            int selectedIndex = Array.IndexOf(Enum.GetValues(type), value);
            optionButton.Select(selectedIndex);
        }

        Select(context.InitialValue!);

        return new VisualControlInfo(new OptionButtonEnumControl(optionButton, Select));
    }
}

/// <summary>
/// Visual-control wrapper around an <see cref="OptionButton"/> for enums.
/// </summary>
/// <param name="optionButton">Option button to manage.</param>
/// <param name="select">Selection callback.</param>
internal sealed class OptionButtonEnumControl(OptionButton optionButton, Action<object> select) : IVisualControl
{
    /// <summary>
    /// Gets the underlying option button control.
    /// </summary>
    public Control Control => optionButton;

    /// <summary>
    /// Updates the selection from the provided value.
    /// </summary>
    /// <param name="value">Incoming value.</param>
    public void SetValue(object value)
    {
        select(value);
    }

    /// <summary>
    /// Toggles editability for the option button.
    /// </summary>
    /// <param name="editable">Whether the control is editable.</param>
    public void SetEditable(bool editable)
    {
        optionButton.Disabled = !editable;
    }
}
#endif
