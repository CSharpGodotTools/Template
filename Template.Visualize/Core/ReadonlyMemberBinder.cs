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
    private readonly List<Action> _updateActions = [];

    public IReadOnlyList<Action> UpdateActions => _updateActions;

    public void AddReadonlyControls(string[] visualizeMembers, object target, Control readonlyMembers, string displayName)
    {
        if (visualizeMembers == null)
        {
            GD.PrintErr($"[Visualize] AddReadonlyControls called with null members on node '{displayName}'");
            return;
        }

        foreach (string visualMember in visualizeMembers)
        {
            if (!TryCreateMemberAccessor(target, visualMember, out MemberAccessor? accessor))
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

    private static bool TryCreateMemberAccessor(object target, string visualMember, [NotNullWhen(true)] out MemberAccessor? accessor)
    {
        BindingFlags memberTypes = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        Type targetType = target.GetType();

        PropertyInfo? property = targetType.GetProperty(visualMember, memberTypes);
        if (property != null && property.GetGetMethod(true) != null)
        {
            accessor = new MemberAccessor(visualMember, property, property.PropertyType);
            return true;
        }

        FieldInfo? field = targetType.GetField(visualMember, memberTypes);
        if (field != null)
        {
            accessor = new MemberAccessor(visualMember, field, field.FieldType);
            return true;
        }

        accessor = null;
        return false;
    }

    private async Task TryAddReadonlyControlAsync(MemberAccessor accessor, Control readonlyMembers, object target, string displayName)
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

            const int OneSecondInMs = 1000;
            await Task.Delay(OneSecondInMs);
            elapsedSeconds++;

            if (elapsedSeconds == VisualUiLayout.MaxSecondsToWaitForInitialValues)
            {
                GD.PrintRich($"[color=orange][Visualize] Tracking '{displayName}' to see if '{accessor.Name}' value changes[/color]");
                break;
            }
        }
    }

    private void AddReadonlyControl(MemberAccessor accessor, Control readonlyMembers, object target, object initialValue)
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

        visualControlInfo.VisualControl.SetEditable(false);

        _updateActions.Add(() =>
        {
            object? current = accessor.GetValue(target);
            if (current is not null)
            {
                visualControlInfo.VisualControl.SetValue(current);
            }
        });

        HBoxContainer hbox = new()
        {
            Name = accessor.Name
        };

        hbox.AddChild(new Label { Text = accessor.Name });
        hbox.AddChild(visualControlInfo.VisualControl.Control);

        readonlyMembers.AddChild(hbox);
    }

    private sealed class MemberAccessor(string name, MemberInfo member, Type memberType)
    {
        public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
        public MemberInfo Member { get; } = member ?? throw new ArgumentNullException(nameof(member));
        public Type MemberType { get; } = memberType ?? throw new ArgumentNullException(nameof(memberType));

        public object? GetValue(object target) => VisualHandler.GetMemberValue(Member, target);
    }
}
#endif
