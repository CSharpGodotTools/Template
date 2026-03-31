#if DEBUG
using Godot;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

internal static class VisualMemberElementBuilder
{
    private const int MemberRowSeparation = 8;
    private const int ClassContainerSeparation = 4;
    private const string LabelSpacerName = "LabelSpacer";

    public static void AddMutableControls(Control vbox, IEnumerable<MemberInfo> members, object target, string displayName)
    {
        foreach (MemberInfo member in members)
        {
            Control? element = CreateMemberInfoElement(member, target, displayName);

            if (element != null)
            {
                vbox.AddChild(element);
            }
        }
    }

    private static Control? CreateMemberInfoElement(MemberInfo member, object target, string displayName)
    {
        object? initialValue = VisualHandler.GetMemberValue(member, target);

        if (initialValue == null)
        {
            PrintUtils.Warning($"[Visualize] '{member.Name}' value in '{displayName}' is null");
            return null;
        }

        VisualControlInfo element = VisualControlTypes.CreateControlForType(VisualHandler.GetMemberType(member), member, new VisualControlContext(initialValue, v =>
        {
            VisualHandler.SetMemberValue(member, target, v);
        }));

        HBoxContainer container = new() { Alignment = BoxContainer.AlignmentMode.End };
        container.AddThemeConstantOverride("separation", MemberRowSeparation);

        Label label = new();

        if (element.VisualControl is ClassControl)
        {
            label.LabelSettings = new LabelSettings
            {
                FontSize = VisualUiLayout.MemberFontSize,
                OutlineSize = VisualUiLayout.FontOutlineSize,
                OutlineColor = Colors.Black,
            };
        }

        label.Text = VisualText.ToDisplayName(member.Name);
        label.HorizontalAlignment = HorizontalAlignment.Right;
        label.CustomMinimumSize = new Vector2(VisualUiLayout.MemberLabelMinWidth, 0);
        container.Name = member.Name;

        if (element.VisualControl == null)
        {
            return container;
        }

        if (element.VisualControl is ClassControl)
        {
            label.LabelSettings = new LabelSettings
            {
                FontSize = VisualUiLayout.MemberFontSize,
                FontColor = Colors.LightSkyBlue,
                OutlineSize = VisualUiLayout.FontOutlineSize,
                OutlineColor = Colors.Black,
            };
            label.HorizontalAlignment = HorizontalAlignment.Right;
            label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            HBoxContainer contentRow = new()
            {
                Alignment = BoxContainer.AlignmentMode.End,
                Name = $"{member.Name}_content",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            contentRow.AddThemeConstantOverride("separation", MemberRowSeparation);

            Control classSpacer = new()
            {
                Name = LabelSpacerName,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            contentRow.AddChild(classSpacer);
            contentRow.AddChild(element.VisualControl.Control);

            VBoxContainer classContainer = new()
            {
                Name = member.Name,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            classContainer.AddThemeConstantOverride("separation", ClassContainerSeparation);
            classContainer.AddChild(label);
            classContainer.AddChild(contentRow);

            return classContainer;
        }

        container.AddChild(label);
        Control spacer = new()
        {
            Name = LabelSpacerName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        container.AddChild(spacer);
        container.AddChild(element.VisualControl.Control);

        return container;
    }
}
#endif
