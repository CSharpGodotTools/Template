using Godot;

namespace Framework.Ui;

/// <summary>
/// Builds labelled HBox rows used by the custom option bindings.
/// </summary>
internal static class OptionRowFactory
{
    private const float LabelMinWidth = 200f;

    /// <summary>
    /// Creates an HBoxContainer with a label and control, wires focus
    /// neighbours, and appends the row to <paramref name="tabContainer"/>.
    /// </summary>
    internal static HBoxContainer Create<TControl>(
        VBoxContainer tabContainer,
        Button navButton,
        string name,
        string labelText,
        TControl control) where TControl : Control
    {
        HBoxContainer row = new() { Name = name };

        Label label = new()
        {
            Text = labelText,
            CustomMinimumSize = new Vector2(LabelMinWidth, 0)
        };

        row.AddChild(label);

        // Link control focus back to the nav button
        control.FocusNeighborLeft = navButton.GetPath();
        row.AddChild(control);

        tabContainer.AddChild(row);

        // First control in a tab becomes the nav button's right focus target
        if (tabContainer.GetChildCount() == 1)
            navButton.FocusNeighborRight = control.GetPath();

        return row;
    }
}
