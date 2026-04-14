#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

/// <summary>
/// Synchronizes displayed values between readonly and mutable title-bar member controls.
/// </summary>
internal sealed class VisualTitleBarValueSyncService : IVisualTitleBarValueSyncService
{
    /// <summary>
    /// Copies readonly member values into mutable controls row-by-row.
    /// </summary>
    /// <param name="mutableMembersVbox">Container with mutable member controls.</param>
    /// <param name="readonlyMembersVbox">Container with readonly member controls.</param>
    public void SyncMutableFromReadonly(Control mutableMembersVbox, Control readonlyMembersVbox)
    {
        int rowCount = Mathf.Min(mutableMembersVbox.GetChildCount(), readonlyMembersVbox.GetChildCount());

        for (int i = 0; i < rowCount; i++)
        {
            Control? mutableValueControl = ResolveValueControl(mutableMembersVbox.GetChild(i), true);
            Control? readonlyValueControl = ResolveValueControl(readonlyMembersVbox.GetChild(i), false);

            // Skip rows where either side cannot provide a value control.
            if (mutableValueControl == null || readonlyValueControl == null)
                continue;

            CopyControlValues(readonlyValueControl, mutableValueControl);
        }
    }

    /// <summary>
    /// Resolves the editable/readable value control for a row layout.
    /// </summary>
    /// <param name="rowNode">Row node that contains value controls.</param>
    /// <param name="isMutable">Whether to resolve the mutable-side control index.</param>
    /// <returns>Resolved control, or <see langword="null"/> when row shape is unsupported.</returns>
    private static Control? ResolveValueControl(Node rowNode, bool isMutable)
    {
        // Handle nested VBox -> HBox row layouts used by grouped title-bar entries.
        if (rowNode is VBoxContainer vbox && vbox.GetChildCount() >= 2)
        {
            // Only nested rows with value controls can be synchronized.
            if (vbox.GetChild(1) is HBoxContainer nestedRow && nestedRow.GetChildCount() > 0)
            {
                int controlIndex = isMutable ? nestedRow.GetChildCount() - 1 : 1;
                return nestedRow.GetChild(controlIndex) as Control;
            }
        }

        // Handle flat HBox rows used by non-grouped entries.
        if (rowNode is HBoxContainer row && row.GetChildCount() > 0)
        {
            int controlIndex = isMutable ? row.GetChildCount() - 1 : 1;
            return row.GetChild(controlIndex) as Control;
        }

        return null;
    }

    /// <summary>
    /// Copies values between compatible control types, recursing into child controls when needed.
    /// </summary>
    /// <param name="source">Readonly source control.</param>
    /// <param name="target">Mutable target control.</param>
    private static void CopyControlValues(Control source, Control target)
    {
        // Copy text-based controls directly.
        if (source is LineEdit sourceLineEdit && target is LineEdit targetLineEdit)
        {
            targetLineEdit.Text = sourceLineEdit.Text;
            return;
        }

        // Copy numeric controls directly.
        if (source is SpinBox sourceSpinBox && target is SpinBox targetSpinBox)
        {
            targetSpinBox.Value = sourceSpinBox.Value;
            return;
        }

        // Copy toggle state controls directly.
        if (source is CheckBox sourceCheckBox && target is CheckBox targetCheckBox)
        {
            targetCheckBox.ButtonPressed = sourceCheckBox.ButtonPressed;
            return;
        }

        // Copy selected index for option controls.
        if (source is OptionButton sourceOptionButton && target is OptionButton targetOptionButton)
        {
            targetOptionButton.Select(sourceOptionButton.Selected);
            return;
        }

        // Copy selected color for color picker controls.
        if (source is ColorPickerButton sourceColorPicker && target is ColorPickerButton targetColorPicker)
        {
            targetColorPicker.Color = sourceColorPicker.Color;
            return;
        }

        int childCount = Mathf.Min(source.GetChildCount(), target.GetChildCount());
        for (int i = 0; i < childCount; i++)
        {
            // Recurse only when both child nodes are controls.
            if (source.GetChild(i) is Control sourceChild && target.GetChild(i) is Control targetChild)
                CopyControlValues(sourceChild, targetChild);
        }
    }
}
#endif
