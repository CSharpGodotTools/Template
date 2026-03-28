#if DEBUG
using Godot;

namespace GodotUtils.Debugging;

internal sealed class VisualTitleBarColumnVisibilityController : IVisualTitleBarColumnVisibilityController
{
    private const float ToggleInactiveModulateFactor = 0.65f;
    private const float HiddenColumnAlpha = 0f;
    private static readonly Color _hiddenColumnColor = new(1, 1, 1, HiddenColumnAlpha);

    private float _mutableColumnWidth;
    private float _readonlyColumnWidth;

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

        if (mutableVisible)
        {
            mutableMembersVbox.Modulate = VisualUiResources.MutableMembersColor;
            SetMutableLabelsOnlyMode(mutableMembersVbox, false);
            SyncLabelRowHeights(mutableMembersVbox, readonlyMembersVbox, false);
        }
        else if (readonlyVisible)
        {
            mutableMembersVbox.Modulate = VisualUiResources.ReadonlyMembersColor;
            SetMutableLabelsOnlyMode(mutableMembersVbox, true);
            SyncLabelRowHeights(mutableMembersVbox, readonlyMembersVbox, true);
        }
        else
        {
            mutableMembersVbox.Modulate = _hiddenColumnColor;
            SetColumnContentVisible(mutableMembersVbox, false);
            SyncLabelRowHeights(mutableMembersVbox, readonlyMembersVbox, false);
        }

        if (readonlyVisible)
        {
            readonlyMembersVbox.Modulate = VisualUiResources.ReadonlyMembersColor;
            SetColumnContentVisible(readonlyMembersVbox, true);
            SetReadonlyLabelsVisible(readonlyMembersVbox, false);
        }
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

    private static void SetToggleVisualState(Button? toggleButton, bool isVisible)
    {
        if (toggleButton == null)
        {
            return;
        }

        toggleButton.SelfModulate = isVisible ? Colors.White : Colors.White * ToggleInactiveModulateFactor;
    }

    private static void SetReadonlyLabelsVisible(Control readonlyMembersVbox, bool visible)
    {
        foreach (Node node in readonlyMembersVbox.GetChildren())
        {
            if (node is not HBoxContainer row || row.GetChildCount() == 0)
            {
                continue;
            }

            if (row.GetChild(0) is Label label)
            {
                label.Visible = visible;
            }
        }
    }

    private static void SetColumnContentVisible(Control columnRoot, bool visible)
    {
        foreach (Node child in columnRoot.GetChildren())
        {
            if (child is CanvasItem canvasItem)
            {
                canvasItem.Visible = visible;
            }
        }
    }

    private static void SetMutableLabelsOnlyMode(Control mutableMembersVbox, bool labelsOnly)
    {
        foreach (Node node in mutableMembersVbox.GetChildren())
        {
            if (node is not Control row)
            {
                continue;
            }

            if (row.GetChildCount() == 0)
            {
                row.Visible = !labelsOnly;
                continue;
            }

            if (row.GetChild(0) is not Label)
            {
                row.Visible = !labelsOnly;
                continue;
            }

            row.Visible = true;

            for (int i = 1; i < row.GetChildCount(); i++)
            {
                if (row.GetChild(i) is CanvasItem canvasItem)
                {
                    canvasItem.Visible = !labelsOnly;
                }
            }
        }
    }

    private static void SyncLabelRowHeights(Control mutableMembersVbox, Control readonlyMembersVbox, bool matchReadonly)
    {
        int rowCount = Mathf.Min(mutableMembersVbox.GetChildCount(), readonlyMembersVbox.GetChildCount());

        for (int i = 0; i < rowCount; i++)
        {
            if (mutableMembersVbox.GetChild(i) is not Control mutableRow)
            {
                continue;
            }

            if (!matchReadonly)
            {
                mutableRow.CustomMinimumSize = new Vector2(mutableRow.CustomMinimumSize.X, 0);
                continue;
            }

            if (readonlyMembersVbox.GetChild(i) is not Control readonlyRow)
            {
                continue;
            }

            float readonlyHeight = readonlyRow.GetCombinedMinimumSize().Y;
            mutableRow.CustomMinimumSize = new Vector2(mutableRow.CustomMinimumSize.X, readonlyHeight);
        }
    }

    private static void SetMutableLabelAlignmentMode(Control mutableMembersVbox, bool mutableVisible)
    {
        foreach (Node node in mutableMembersVbox.GetChildren())
        {
            if (node is not HBoxContainer row || row.GetChildCount() == 0)
            {
                continue;
            }

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
