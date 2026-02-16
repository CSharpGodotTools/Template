#if DEBUG
using Godot;
using System.Linq;
using static Godot.Control;

namespace GodotUtils.Debugging;

internal static class VisualTitleBarBuilder
{
    public static VBoxContainer Build(string name, Control mutableMembersVbox, Control readonlyMembersVbox, VisualData visualData, string[] readonlyMembers)
    {
        VBoxContainer vboxParent = new();

        HBoxContainer hbox = new()
        {
            Name = "Title Bar",
            Alignment = BoxContainer.AlignmentMode.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };

        Label title = new()
        {
            Name = "Title",
            Text = name,
            Visible = true,
            LabelSettings = new LabelSettings
            {
                FontSize = VisualUiLayout.TitleFontSize,
                FontColor = Colors.LightSkyBlue,
                OutlineColor = Colors.Black,
                OutlineSize = VisualUiLayout.FontOutlineSize,
            }
        };

        hbox.AddChild(title);

        Button readonlyBtn = null;
        Button mutableBtn = null;

        if (readonlyMembers != null)
        {
            readonlyBtn = VisualUiElementFactory.CreateVisibilityButton(VisualUiResources.EyeOpen, Colors.Pink);
            readonlyBtn.ButtonPressed = true;
            hbox.AddChild(readonlyBtn);
        }

        if (visualData.Properties.Any() || visualData.Fields.Any())
        {
            mutableBtn = VisualUiElementFactory.CreateVisibilityButton(VisualUiResources.Wrench, Colors.Gray);
            mutableBtn.ButtonPressed = true;
            hbox.AddChild(mutableBtn);
        }

        if (readonlyBtn != null)
        {
            readonlyBtn.Pressed += () =>
            {
                readonlyBtn.Icon = readonlyBtn.ButtonPressed ? VisualUiResources.EyeOpen : VisualUiResources.EyeClosed;
                readonlyMembersVbox.Visible = readonlyBtn.ButtonPressed;
                title.Visible = readonlyBtn.ButtonPressed || (mutableBtn != null && mutableBtn.ButtonPressed);
            };
        }

        if (mutableBtn != null)
        {
            mutableBtn.Pressed += () =>
            {
                mutableMembersVbox.Visible = mutableBtn.ButtonPressed;
                title.Visible = mutableBtn.ButtonPressed || (readonlyBtn != null && readonlyBtn.ButtonPressed);
            };
        }

        vboxParent.AddChild(hbox);
        VisualUiElementFactory.SetButtonsToReleaseFocusOnPress(vboxParent);

        return vboxParent;
    }
}
#endif
