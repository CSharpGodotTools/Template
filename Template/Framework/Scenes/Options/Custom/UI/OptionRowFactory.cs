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
    /// <typeparam name="TControl">Control type used for the row's primary editor.</typeparam>
    /// <param name="tabContainer">Destination tab container.</param>
    /// <param name="navButton">Navigation button used for left focus neighbor.</param>
    /// <param name="name">Row node name.</param>
    /// <param name="labelText">Label text shown on the left side.</param>
    /// <param name="control">Primary control added to the row.</param>
    /// <returns>Created row container.</returns>
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

    /// <summary>
    /// Attempts to retrieve the right-side controls container from a row.
    /// </summary>
    /// <param name="row">Row container to inspect.</param>
    /// <param name="controlsContainer">Resolved controls container when found.</param>
    /// <returns><see langword="true"/> when controls container exists.</returns>
    internal static bool TryGetControlsContainer(HBoxContainer row, out HBoxContainer controlsContainer)
    {
        // Resolve controls container only when row has the expected second child.
        if (row.GetChildCount() >= 2 && row.GetChild(1) is HBoxContainer controls)
        {
            controlsContainer = controls;
            return true;
        }

        controlsContainer = null!;
        return false;
    }

    /// <summary>
    /// Attempts to retrieve the first control inside a row's controls container.
    /// </summary>
    /// <param name="row">Row container to inspect.</param>
    /// <param name="control">Resolved primary control when found.</param>
    /// <returns><see langword="true"/> when a primary control exists.</returns>
    internal static bool TryGetPrimaryControl(HBoxContainer row, out Control control)
    {
        // Resolve primary control only when a controls container and first control exist.
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
