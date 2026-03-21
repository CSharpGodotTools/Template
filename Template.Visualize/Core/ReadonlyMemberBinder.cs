#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace GodotUtils.Debugging;

internal sealed class ReadonlyMemberBinder
{
    private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    private const int PollDelayMilliseconds = 1000;
    private const string NullMembersMessageTemplate = "[Visualize] AddReadonlyControls called with null members on node '{0}'";
    private const string TrackingMessageTemplate = "[color=orange][Visualize] Tracking '{0}' to see if '{1}' value changes[/color]";

    private readonly List<Action> _updateActions = [];

    public IReadOnlyList<Action> UpdateActions => _updateActions;

    public void AddReadonlyControls(string[] visualizeMembers, object target, Control readonlyMembers, string displayName)
    {
        if (visualizeMembers == null)
        {
            GD.PrintErr(string.Format(NullMembersMessageTemplate, displayName));
            return;
        }

        foreach (string visualMember in visualizeMembers)
        {
            if (!TryCreateMemberAccessor(target, visualMember, out ReadonlyMemberAccessor? accessor))
            {
                continue;
            }

            object? initialValue = accessor.GetValue(target);

            if (initialValue != null)
            {
                AddReadonlyControl(accessor, readonlyMembers, target, initialValue);
            }
            else
            {
                _ = TryAddReadonlyControlAsync(accessor, readonlyMembers, target, displayName);
            }
        }
    }

    private static bool TryCreateMemberAccessor(object target, string visualMember, [NotNullWhen(true)] out ReadonlyMemberAccessor? accessor)
    {
        Type targetType = target.GetType();

        PropertyInfo? property = targetType.GetProperty(visualMember, MemberBindingFlags);
        if (property != null && property.GetGetMethod(true) != null)
        {
            accessor = new ReadonlyMemberAccessor(visualMember, property, property.PropertyType);
            return true;
        }

        FieldInfo? field = targetType.GetField(visualMember, MemberBindingFlags);
        if (field != null)
        {
            accessor = new ReadonlyMemberAccessor(visualMember, field, field.FieldType);
            return true;
        }

        accessor = null;
        return false;
    }

    private async Task TryAddReadonlyControlAsync(ReadonlyMemberAccessor accessor, Control readonlyMembers, object target, string displayName)
    {
        int elapsedSeconds = 0;

        while (true)
        {
            object? value = accessor.GetValue(target);

            if (value != null)
            {
                AddReadonlyControl(accessor, readonlyMembers, target, value);
                break;
            }

            await Task.Delay(PollDelayMilliseconds);
            elapsedSeconds++;

            if (elapsedSeconds == VisualUiLayout.MaxSecondsToWaitForInitialValues)
            {
                GD.PrintRich(string.Format(TrackingMessageTemplate, displayName, accessor.Name));
                break;
            }
        }
    }

    private void AddReadonlyControl(ReadonlyMemberAccessor accessor, Control readonlyMembers, object target, object initialValue)
    {
        VisualControlContext context = new(initialValue, _ =>
        {
            // Do nothing
        });

        VisualControlInfo visualControlInfo = VisualControlTypes.CreateControlForType(accessor.MemberType, accessor.Member, context);

        if (visualControlInfo.VisualControl == null)
        {
            return;
        }

        IVisualControl visualControl = visualControlInfo.VisualControl;

        // Readonly column should never accept user edits.
        visualControl.SetEditable(false);

        _updateActions.Add(() =>
        {
            object? current = accessor.GetValue(target);
            if (current is not null)
            {
                visualControl.SetValue(current);
            }
        });

        HBoxContainer hbox = new()
        {
            Name = accessor.Name,
            Alignment = BoxContainer.AlignmentMode.Begin
        };

        Label label = new()
        {
            Text = VisualText.ToDisplayName(accessor.Name),
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(VisualUiLayout.MemberLabelMinWidth, 0),
            Visible = false
        };

        hbox.AddChild(label);

        hbox.AddChild(visualControl.Control);

        readonlyMembers.AddChild(hbox);
    }

}
#endif
