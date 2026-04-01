#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Applies title-bar toggle state to mutable, readonly, and methods columns.
/// </summary>
internal sealed class VisualTitleBarColumnVisibilityController : IVisualTitleBarColumnVisibilityController
{
    private const float ToggleInactiveModulateFactor = 0.65f;
    private const float HiddenColumnAlpha = 0f;
    private static readonly Color _hiddenColumnColor = new(1, 1, 1, HiddenColumnAlpha);

    private float _mutableColumnWidth;
    private float _readonlyColumnWidth;

    /// <summary>
    /// Updates column visibility, colors, and alignment based on current toggle states.
    /// </summary>
    /// <param name="mutableMembersVbox">Mutable members column root.</param>
    /// <param name="readonlyMembersVbox">Readonly members column root.</param>
    /// <param name="methodsVbox">Methods column root.</param>
    /// <param name="title">Title label control.</param>
    /// <param name="mutableButton">Mutable column toggle.</param>
    /// <param name="readonlyButton">Readonly column toggle.</param>
    /// <param name="methodsButton">Methods column toggle.</param>
    public void Update(
        Control mutableMembersVbox,
        Control readonlyMembersVbox,
        Control methodsVbox,
        Label title,
        Button? mutableButton,
        Button? readonlyButton,
        Button? methodsButton)
    {
        bool mutableVisible = mutableButton?.ButtonPressed ?? false;
        bool readonlyVisible = readonlyButton?.ButtonPressed ?? false;
        bool methodsVisible = methodsButton?.ButtonPressed ?? false;

        _mutableColumnWidth = Mathf.Max(_mutableColumnWidth, mutableMembersVbox.GetCombinedMinimumSize().X);
        _readonlyColumnWidth = Mathf.Max(_readonlyColumnWidth, readonlyMembersVbox.GetCombinedMinimumSize().X);

        mutableMembersVbox.CustomMinimumSize = new Vector2(_mutableColumnWidth, 0);
        readonlyMembersVbox.CustomMinimumSize = new Vector2(_readonlyColumnWidth, 0);

        SetToggleVisualState(mutableButton, mutableVisible);
        SetToggleVisualState(readonlyButton, readonlyVisible);
        SetToggleVisualState(methodsButton, methodsVisible);

        mutableMembersVbox.Visible = true;
        readonlyMembersVbox.Visible = true;
        SetMutableLabelAlignmentMode(mutableMembersVbox, mutableVisible);

        // Mutable column fully visible: show editable controls and mutable color treatment.
        if (mutableVisible)
        {
            mutableMembersVbox.Modulate = VisualUiResources.MutableMembersColor;
            SetMutableLabelsOnlyMode(mutableMembersVbox, false);
            SyncLabelRowHeights(mutableMembersVbox, readonlyMembersVbox, false);
        }
        // Mutable hidden but readonly visible: keep labels and align heights with readonly rows.
        else if (readonlyVisible)
        {
            mutableMembersVbox.Modulate = VisualUiResources.ReadonlyMembersColor;
            SetMutableLabelsOnlyMode(mutableMembersVbox, true);
            SyncLabelRowHeights(mutableMembersVbox, readonlyMembersVbox, true);
        }
        // Both hidden: suppress mutable content entirely.
        else
        {
            mutableMembersVbox.Modulate = _hiddenColumnColor;
            SetColumnContentVisible(mutableMembersVbox, false);
            SyncLabelRowHeights(mutableMembersVbox, readonlyMembersVbox, false);
        }

        // Readonly column visible: show value widgets while keeping labels in mutable side.
        if (readonlyVisible)
        {
            readonlyMembersVbox.Modulate = VisualUiResources.ReadonlyMembersColor;
            SetColumnContentVisible(readonlyMembersVbox, true);
            SetReadonlyLabelsVisible(readonlyMembersVbox, false);
        }
        // Readonly hidden: remove content and apply hidden styling.
        else
        {
            readonlyMembersVbox.Modulate = _hiddenColumnColor;
            SetColumnContentVisible(readonlyMembersVbox, false);
            SetReadonlyLabelsVisible(readonlyMembersVbox, false);
        }

        methodsVbox.Visible = methodsVisible;
        methodsVbox.Modulate = methodsVisible ? Colors.White : _hiddenColumnColor;
        title.Visible = true;
    }

    /// <summary>
    /// Applies active/inactive tinting to a visibility toggle button.
    /// </summary>
    /// <param name="toggleButton">Button to style.</param>
    /// <param name="isVisible">Whether the associated column is currently visible.</param>
    private static void SetToggleVisualState(Button? toggleButton, bool isVisible)
    {
        // Missing toggle means that column is unavailable in this panel.
        if (toggleButton == null)
        {
            return;
        }

        toggleButton.SelfModulate = isVisible ? Colors.White : Colors.White * ToggleInactiveModulateFactor;
    }

    /// <summary>
    /// Shows or hides leading readonly labels for each readonly row.
    /// </summary>
    /// <param name="readonlyMembersVbox">Readonly column root.</param>
    /// <param name="visible">Desired label visibility.</param>
    private static void SetReadonlyLabelsVisible(Control readonlyMembersVbox, bool visible)
    {
        foreach (Node node in readonlyMembersVbox.GetChildren())
        {
            // Ignore rows that do not follow the expected HBox label/value layout.
            if (node is not HBoxContainer row || row.GetChildCount() == 0)
            {
                continue;
            }

            // Toggle the first label when present.
            if (row.GetChild(0) is Label label)
            {
                label.Visible = visible;
            }
        }
    }

    /// <summary>
    /// Sets visibility for all canvas-item children beneath a column root.
    /// </summary>
    /// <param name="columnRoot">Column root to traverse.</param>
    /// <param name="visible">Desired content visibility.</param>
    private static void SetColumnContentVisible(Control columnRoot, bool visible)
    {
        foreach (Node child in columnRoot.GetChildren())
        {
            // Only canvas items participate in render visibility toggling.
            if (child is CanvasItem canvasItem)
            {
                canvasItem.Visible = visible;
            }
        }
    }

    /// <summary>
    /// Toggles mutable rows between full-control mode and labels-only mode.
    /// </summary>
    /// <param name="mutableMembersVbox">Mutable column root.</param>
    /// <param name="labelsOnly">Whether only labels should remain visible.</param>
    private static void SetMutableLabelsOnlyMode(Control mutableMembersVbox, bool labelsOnly)
    {
        foreach (Node node in mutableMembersVbox.GetChildren())
        {
            // Skip nodes that cannot be treated as row controls.
            if (node is not Control row)
            {
                continue;
            }

            // Rows without children are shown only when not in labels-only mode.
            if (row.GetChildCount() == 0)
            {
                row.Visible = !labelsOnly;
                continue;
            }

            // Rows without a leading label are treated as value-only rows.
            if (row.GetChild(0) is not Label)
            {
                row.Visible = !labelsOnly;
                continue;
            }

            row.Visible = true;

            for (int i = 1; i < row.GetChildCount(); i++)
            {
                // Hide non-label canvas children while retaining the label itself.
                if (row.GetChild(i) is CanvasItem canvasItem)
                {
                    canvasItem.Visible = !labelsOnly;
                }
            }
        }
    }

    /// <summary>
    /// Optionally aligns mutable row heights to readonly rows when mutable values are hidden.
    /// </summary>
    /// <param name="mutableMembersVbox">Mutable column root.</param>
    /// <param name="readonlyMembersVbox">Readonly column root.</param>
    /// <param name="matchReadonly">Whether mutable rows should mirror readonly row heights.</param>
    private static void SyncLabelRowHeights(Control mutableMembersVbox, Control readonlyMembersVbox, bool matchReadonly)
    {
        int rowCount = Mathf.Min(mutableMembersVbox.GetChildCount(), readonlyMembersVbox.GetChildCount());

        for (int i = 0; i < rowCount; i++)
        {
            // Skip rows that are not controls.
            if (mutableMembersVbox.GetChild(i) is not Control mutableRow)
            {
                continue;
            }

            // In full mode, clear explicit height overrides.
            if (!matchReadonly)
            {
                mutableRow.CustomMinimumSize = new Vector2(mutableRow.CustomMinimumSize.X, 0);
                continue;
            }

            // Skip readonly rows that cannot provide a minimum height.
            if (readonlyMembersVbox.GetChild(i) is not Control readonlyRow)
            {
                continue;
            }

            float readonlyHeight = readonlyRow.GetCombinedMinimumSize().Y;
            mutableRow.CustomMinimumSize = new Vector2(mutableRow.CustomMinimumSize.X, readonlyHeight);
        }
    }

    /// <summary>
    /// Sets mutable row alignment so labels anchor correctly for visible/hidden value modes.
    /// </summary>
    /// <param name="mutableMembersVbox">Mutable column root.</param>
    /// <param name="mutableVisible">Whether mutable values are currently visible.</param>
    private static void SetMutableLabelAlignmentMode(Control mutableMembersVbox, bool mutableVisible)
    {
        foreach (Node node in mutableMembersVbox.GetChildren())
        {
            // Alignment updates only apply to standard HBox row layouts.
            if (node is not HBoxContainer row || row.GetChildCount() == 0)
            {
                continue;
            }

            // Rows without a leading label are not part of label alignment adjustments.
            if (row.GetChild(0) is not Label label)
            {
                continue;
            }

            row.Alignment = mutableVisible ? BoxContainer.AlignmentMode.Begin : BoxContainer.AlignmentMode.End;
            label.HorizontalAlignment = HorizontalAlignment.Right;
            label.SizeFlagsHorizontal = Control.SizeFlags.Fill;
            label.CustomMinimumSize = new Vector2(VisualUiLayout.MemberLabelMinWidth, label.CustomMinimumSize.Y);
        }
    }
}
#endif
