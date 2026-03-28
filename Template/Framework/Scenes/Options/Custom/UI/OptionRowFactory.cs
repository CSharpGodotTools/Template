using Godot;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Builds labelled HBox rows used by the custom option bindings.
/// </summary>
internal static class OptionRowFactory
{
    private const float LabelMinWidth = 200f;
    private const float ControlSeparation = 10f;

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
        HBoxContainer controlsContainer = new() { Name = "Controls" };
        controlsContainer.AddThemeConstantOverride("separation", (int)ControlSeparation);

        Label label = new()
        {
            Text = labelText,
            CustomMinimumSize = new Vector2(LabelMinWidth, 0)
        };

        row.AddChild(label);

        // Link control focus back to the nav button
        control.FocusNeighborLeft = navButton.GetPath();
        controlsContainer.AddChild(control);
        row.AddChild(controlsContainer);

        tabContainer.AddChild(row);

        // First control in a tab becomes the nav button's right focus target
        if (tabContainer.GetChildCount() == 1)
            navButton.FocusNeighborRight = control.GetPath();

        return row;
    }

    internal static bool TryGetControlsContainer(HBoxContainer row, out HBoxContainer controlsContainer)
    {
        if (row.GetChildCount() >= 2 && row.GetChild(1) is HBoxContainer controls)
        {
            controlsContainer = controls;
            return true;
        }

        controlsContainer = null!;
        return false;
    }

    internal static bool TryGetPrimaryControl(HBoxContainer row, out Control control)
    {
        if (TryGetControlsContainer(row, out HBoxContainer controlsContainer)
            && controlsContainer.GetChildCount() > 0
            && controlsContainer.GetChild(0) is Control primary)
        {
            control = primary;
            return true;
        }

        control = null!;
        return false;
    }
}
