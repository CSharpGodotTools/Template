using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Disposable helper that owns a toggle/checkbox row and its event subscription.
/// </summary>
internal sealed class ToggleBinding(HBoxContainer row, CheckBox checkbox, CheckBox.ToggledEventHandler onToggled) : IDisposable
{
    private const float ControlMinWidth = 250f;

    private readonly HBoxContainer _row = row;
    private readonly CheckBox _checkbox = checkbox;
    private readonly CheckBox.ToggledEventHandler _onToggled = onToggled;

    /// <summary>
    /// Builds the checkbox control, syncs its initial state, and wires events.
    /// </summary>
    internal static ToggleBinding Create(
        VBoxContainer tabContainer, Button navButton,
        RegisteredToggleOption toggleOption)
    {
        ToggleOptionDefinition definition = toggleOption.Definition;

        CheckBox checkbox = new()
        {
            CustomMinimumSize = new Vector2(ControlMinWidth, 0)
        };

        string label = string.IsNullOrWhiteSpace(definition.Label)
            ? $"TOGGLE_{toggleOption.Id}"
            : definition.Label;

        HBoxContainer row = OptionRowFactory.Create(
            tabContainer, navButton, $"CustomToggle_{toggleOption.Id}", label, checkbox);

        // Push persisted state into both the definition and control
        bool value = toggleOption.GetValue();
        toggleOption.SetValue(value);
        checkbox.ButtonPressed = value;

        void onToggled(bool pressed) => toggleOption.SetValue(pressed);
        checkbox.Toggled += onToggled;

        return new ToggleBinding(row, checkbox, onToggled);
    }

    public void Dispose()
    {
        if (GodotObject.IsInstanceValid(_checkbox))
            _checkbox.Toggled -= _onToggled;

        if (GodotObject.IsInstanceValid(_row))
            _row.QueueFree();
    }
}
