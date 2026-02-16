#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace GodotUtils.Debugging;

internal sealed class ReadonlyMemberBinder
{
    private readonly List<Action> _updateActions = [];

    public IReadOnlyList<Action> UpdateActions => _updateActions;

    public void AddReadonlyControls(string[] visualizeMembers, Node node, Control readonlyMembers)
    {
        if (visualizeMembers == null)
        {
            return;
        }

        foreach (string visualMember in visualizeMembers)
        {
            if (!TryCreateMemberAccessor(node, visualMember, out MemberAccessor accessor))
            {
                continue;
            }

            object initialValue = accessor.GetValue(node);

            if (initialValue != null)
            {
                AddReadonlyControl(accessor, readonlyMembers, node, initialValue);
            }
            else
            {
                _ = TryAddReadonlyControlAsync(accessor, readonlyMembers, node);
            }
        }
    }

    private static bool TryCreateMemberAccessor(Node node, string visualMember, out MemberAccessor accessor)
    {
        BindingFlags memberTypes = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        PropertyInfo property = node.GetType().GetProperty(visualMember, memberTypes);
        if (property != null && property.GetGetMethod(true) != null)
        {
            accessor = new MemberAccessor(visualMember, property, property.PropertyType);
            return true;
        }

        FieldInfo field = node.GetType().GetField(visualMember, memberTypes);
        if (field != null)
        {
            accessor = new MemberAccessor(visualMember, field, field.FieldType);
            return true;
        }

        accessor = null;
        return false;
    }

    private async Task TryAddReadonlyControlAsync(MemberAccessor accessor, Control readonlyMembers, Node node)
    {
        int elapsedSeconds = 0;

        while (true)
        {
            object value = accessor.GetValue(node);

            if (value != null)
            {
                AddReadonlyControl(accessor, readonlyMembers, node, value);
                break;
            }

            const int OneSecondInMs = 1000;
            await Task.Delay(OneSecondInMs);
            elapsedSeconds++;

            if (elapsedSeconds == VisualUiLayout.MaxSecondsToWaitForInitialValues)
            {
                GD.PrintRich($"[color=orange][Visualize] Tracking '{node.Name}' to see if '{accessor.Name}' value changes[/color]");
            }
        }
    }

    private void AddReadonlyControl(MemberAccessor accessor, Control readonlyMembers, Node node, object initialValue)
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
            visualControlInfo.VisualControl.SetValue(accessor.GetValue(node));
        });

        HBoxContainer hbox = new();
        hbox.Name = accessor.Name;

        hbox.AddChild(new Label { Text = accessor.Name });
        hbox.AddChild(visualControlInfo.VisualControl.Control);

        readonlyMembers.AddChild(hbox);
    }

    private sealed class MemberAccessor
    {
        public MemberAccessor(string name, MemberInfo member, Type memberType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Member = member ?? throw new ArgumentNullException(nameof(member));
            MemberType = memberType ?? throw new ArgumentNullException(nameof(memberType));
        }

        public string Name { get; }
        public MemberInfo Member { get; }
        public Type MemberType { get; }

        public object GetValue(Node node) => VisualHandler.GetMemberValue(Member, node);
    }
}
#endif
