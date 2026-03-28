#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal sealed class VisualTitleBarValueSyncService : IVisualTitleBarValueSyncService
{
    public void SyncMutableFromReadonly(Control mutableMembersVbox, Control readonlyMembersVbox)
    {
        int rowCount = Mathf.Min(mutableMembersVbox.GetChildCount(), readonlyMembersVbox.GetChildCount());

        for (int i = 0; i < rowCount; i++)
        {
            Control? mutableValueControl = ResolveValueControl(mutableMembersVbox.GetChild(i), true);
            Control? readonlyValueControl = ResolveValueControl(readonlyMembersVbox.GetChild(i), false);

            if (mutableValueControl == null || readonlyValueControl == null)
            {
                continue;
            }

            CopyControlValues(readonlyValueControl, mutableValueControl);
        }
    }

    private static Control? ResolveValueControl(Node rowNode, bool isMutable)
    {
        if (rowNode is VBoxContainer vbox && vbox.GetChildCount() >= 2)
        {
            if (vbox.GetChild(1) is HBoxContainer nestedRow && nestedRow.GetChildCount() > 0)
            {
                int controlIndex = isMutable ? nestedRow.GetChildCount() - 1 : 1;
                return nestedRow.GetChild(controlIndex) as Control;
            }
        }

        if (rowNode is HBoxContainer row && row.GetChildCount() > 0)
        {
            int controlIndex = isMutable ? row.GetChildCount() - 1 : 1;
            return row.GetChild(controlIndex) as Control;
        }

        return null;
    }

    private static void CopyControlValues(Control source, Control target)
    {
        if (source is LineEdit sourceLineEdit && target is LineEdit targetLineEdit)
        {
            targetLineEdit.Text = sourceLineEdit.Text;
            return;
        }

        if (source is SpinBox sourceSpinBox && target is SpinBox targetSpinBox)
        {
            targetSpinBox.Value = sourceSpinBox.Value;
            return;
        }

        if (source is CheckBox sourceCheckBox && target is CheckBox targetCheckBox)
        {
            targetCheckBox.ButtonPressed = sourceCheckBox.ButtonPressed;
            return;
        }

        if (source is OptionButton sourceOptionButton && target is OptionButton targetOptionButton)
        {
            targetOptionButton.Select(sourceOptionButton.Selected);
            return;
        }

        if (source is ColorPickerButton sourceColorPicker && target is ColorPickerButton targetColorPicker)
        {
            targetColorPicker.Color = sourceColorPicker.Color;
            return;
        }

        int childCount = Mathf.Min(source.GetChildCount(), target.GetChildCount());
        for (int i = 0; i < childCount; i++)
        {
            if (source.GetChild(i) is Control sourceChild && target.GetChild(i) is Control targetChild)
            {
                CopyControlValues(sourceChild, targetChild);
            }
        }
    }
}
#endif
