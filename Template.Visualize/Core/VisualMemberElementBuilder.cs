#if DEBUG
using Godot;
using System.Collections.Generic;
using System.Reflection;

namespace GodotUtils.Debugging;

internal static class VisualMemberElementBuilder
{
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

        Control container;
        Label label = new();

        if (element.VisualControl is ClassControl)
        {
            container = new VBoxContainer();
            label.LabelSettings = new LabelSettings
            {
                FontSize = VisualUiLayout.MemberFontSize,
                OutlineSize = VisualUiLayout.FontOutlineSize,
                OutlineColor = Colors.Black,
            };
        }
        else
        {
            container = new HBoxContainer();
        }

        label.Text = VisualText.ToDisplayName(member.Name);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        container.Name = member.Name;

        if (element.VisualControl == null)
        {
            return container;
        }

        container.AddChild(label);
        container.AddChild(element.VisualControl.Control);

        return container;
    }
}
#endif
