#if DEBUG
using Godot;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Builds UI rows for mutable visualized members.
/// </summary>
internal static class VisualMemberElementBuilder
{
    private const int MemberRowSeparation = 8;
    private const int ClassContainerSeparation = 4;
    private const string LabelSpacerName = "LabelSpacer";

    /// <summary>
    /// Adds mutable controls for each member to the target container.
    /// </summary>
    /// <param name="vbox">Container that receives generated controls.</param>
    /// <param name="members">Members to visualize.</param>
    /// <param name="target">Instance owning member values.</param>
    /// <param name="displayName">Display name used in diagnostics.</param>
    public static void AddMutableControls(Control vbox, IEnumerable<MemberInfo> members, object target, string displayName)
    {
        foreach (MemberInfo member in members)
        {
            Control? element = CreateMemberInfoElement(member, target, displayName);

            // Add only successfully created controls.
            if (element != null)
            {
                vbox.AddChild(element);
            }
        }
    }

    /// <summary>
    /// Creates the mutable UI element for a single member.
    /// </summary>
    /// <param name="member">Member metadata.</param>
    /// <param name="target">Instance owning member value.</param>
    /// <param name="displayName">Display name used for warnings.</param>
    /// <returns>Generated control element, or <see langword="null"/> when value/control cannot be created.</returns>
    private static Control? CreateMemberInfoElement(MemberInfo member, object target, string displayName)
    {
        object? initialValue = VisualHandler.GetMemberValue(member, target);

        // Null member values cannot initialize editable controls reliably.
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

        // Class controls use title-style labels rather than standard row labels.
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

        // Return label-only container when no editable control exists for this type.
        if (element.VisualControl == null)
        {
            return container;
        }

        // Class controls render in nested layout with title row and content row.
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
