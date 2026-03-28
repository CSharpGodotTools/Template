using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Disposable helper that owns a dropdown row and its event subscription.
/// </summary>
internal sealed class DropdownBinding(HBoxContainer row, OptionButton dropdown, OptionButton.ItemSelectedEventHandler onItemSelected) : IDisposable
{
    private readonly HBoxContainer _row = row;
    private readonly OptionButton _dropdown = dropdown;
    private readonly OptionButton.ItemSelectedEventHandler _onItemSelected = onItemSelected;

    /// <summary>
    /// Builds the dropdown control, populates items, syncs selection, and wires events.
    /// </summary>
    internal static DropdownBinding Create(
        VBoxContainer tabContainer, Button navButton,
        RegisteredDropdownOption dropdownOption)
    {
        DropdownOptionDefinition definition = dropdownOption.Definition;

        float controlMinWidth = Mathf.Max(1f, definition.ControlMinWidth);
        OptionButton dropdown = new() { CustomMinimumSize = new Vector2(controlMinWidth, 0) };

        for (int index = 0; index < definition.Items.Count; index++)
            dropdown.AddItem(definition.Items[index], index);

        string label = string.IsNullOrWhiteSpace(definition.Label)
            ? $"DROPDOWN_{dropdownOption.Id}"
            : definition.Label;

        HBoxContainer row = OptionRowFactory.Create(
            tabContainer, navButton, $"CustomDropdown_{dropdownOption.Id}", label, dropdown);

        // Clamp the persisted index to the valid item range
        int maxIndex = definition.Items.Count - 1;
        int clamped = Mathf.Clamp(dropdownOption.GetValue(), 0, maxIndex);
        dropdownOption.SetValue(clamped);
        dropdown.Select(clamped);

        OptionButton.ItemSelectedEventHandler onItemSelected = i => dropdownOption.SetValue((int)i);
        dropdown.ItemSelected += onItemSelected;

        return new DropdownBinding(row, dropdown, onItemSelected);
    }

    public void Dispose()
    {
        if (GodotObject.IsInstanceValid(_dropdown))
            _dropdown.ItemSelected -= _onItemSelected;

        if (GodotObject.IsInstanceValid(_row))
            _row.QueueFree();
    }
}
