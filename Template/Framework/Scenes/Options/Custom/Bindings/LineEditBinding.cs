using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Disposable helper that owns a line‑edit row and its event subscription.
/// </summary>
/// <param name="row">Row container that owns the line-edit control.</param>
/// <param name="lineEdit">Line-edit control bound to option state.</param>
/// <param name="onTextChanged">Signal handler used to persist text changes.</param>
internal sealed class LineEditBinding(HBoxContainer row, LineEdit lineEdit, LineEdit.TextChangedEventHandler onTextChanged) : IDisposable
{
    private const float ControlMinWidth = 250f;

    private readonly HBoxContainer _row = row;
    private readonly LineEdit _lineEdit = lineEdit;
    private readonly LineEdit.TextChangedEventHandler _onTextChanged = onTextChanged;

    /// <summary>
    /// Builds the line‑edit control, sets initial text, and wires events.
    /// </summary>
    /// <param name="tabContainer">Tab container that will own the row.</param>
    /// <param name="navButton">Navigation button used for focus wiring.</param>
    /// <param name="lineEditOption">Registered line-edit option metadata.</param>
    /// <returns>Disposable binding that owns row and signal subscription.</returns>
    internal static LineEditBinding Create(
        VBoxContainer tabContainer, Button navButton,
        RegisteredLineEditOption lineEditOption)
    {
        LineEditOptionDefinition definition = lineEditOption.Definition;

        LineEdit lineEdit = new()
        {
            CustomMinimumSize = new Vector2(ControlMinWidth, 0),
            PlaceholderText = definition.Placeholder
        };

        string label = string.IsNullOrWhiteSpace(definition.Label)
            ? $"LINE_EDIT_{lineEditOption.Id}"
            : definition.Label;

        HBoxContainer row = OptionRowFactory.Create(
            tabContainer, navButton, $"CustomLineEdit_{lineEditOption.Id}", label, lineEdit);

        // Push the persisted text into both the definition and control
        string value = lineEditOption.GetValue() ?? string.Empty;
        lineEditOption.SetValue(value);
        lineEdit.Text = value;

        void onTextChanged(string text) => lineEditOption.SetValue(text ?? string.Empty);
        lineEdit.TextChanged += onTextChanged;

        return new LineEditBinding(row, lineEdit, onTextChanged);
    }

    /// <summary>
    /// Unsubscribes events and frees the generated row.
    /// </summary>
    public void Dispose()
    {
        // Unsubscribe only while the line-edit instance is still valid.
        if (GodotObject.IsInstanceValid(_lineEdit))
            _lineEdit.TextChanged -= _onTextChanged;

        // Free row only while the row instance is still valid.
        if (GodotObject.IsInstanceValid(_row))
            _row.QueueFree();
    }
}
