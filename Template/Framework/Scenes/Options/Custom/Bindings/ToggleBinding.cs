using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Disposable helper that owns a toggle/checkbox row and its event subscription.
/// </summary>
/// <param name="row">Row container that owns the checkbox control.</param>
/// <param name="checkbox">Checkbox control bound to option state.</param>
/// <param name="onToggled">Signal handler used to persist toggle changes.</param>
internal sealed class ToggleBinding(HBoxContainer row, CheckBox checkbox, BaseButton.ToggledEventHandler onToggled) : IDisposable
{
    private const float ControlMinWidth = 250f;

    private readonly HBoxContainer _row = row;
    private readonly CheckBox _checkbox = checkbox;
    private readonly BaseButton.ToggledEventHandler _onToggled = onToggled;

    /// <summary>
    /// Builds the checkbox control, syncs its initial state, and wires events.
    /// </summary>
    /// <param name="tabContainer">Tab container that will own the row.</param>
    /// <param name="navButton">Navigation button used for focus wiring.</param>
    /// <param name="toggleOption">Registered toggle option metadata.</param>
    /// <returns>Disposable binding that owns row and signal subscription.</returns>
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

    /// <summary>
    /// Unsubscribes events and frees the generated row.
    /// </summary>
    public void Dispose()
    {
        // Unsubscribe only while the checkbox instance is still valid.
        if (GodotObject.IsInstanceValid(_checkbox))
            _checkbox.Toggled -= _onToggled;

        // Free row only while the row instance is still valid.
        if (GodotObject.IsInstanceValid(_row))
            _row.QueueFree();
    }
}
