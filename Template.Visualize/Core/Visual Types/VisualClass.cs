#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GodotUtils.Debugging;

internal static partial class VisualControlTypes
{
    private static VisualControlInfo VisualClass(Type type, VisualControlContext context)
    {
        GridContainer container = new() { Columns = 1 };

        if (context.InitialValue == null)
        {
            return new VisualControlInfo(new ClassControl(container, []));
        }

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        List<MemberControlBinding> memberBindings = [];
        memberBindings.AddRange(AddMembers(container, context, CollectPropertyMembers(type, flags)));
        memberBindings.AddRange(AddMembers(container, context, CollectFieldMembers(type, flags)));
        AddMethods(flags, container, type, context);

        return new VisualControlInfo(new ClassControl(container, memberBindings));
    }

    private static IEnumerable<MemberDescriptor> CollectPropertyMembers(Type type, BindingFlags flags)
    {
        PropertyInfo[] properties = [.. type.GetProperties(flags).Where(p => !(typeof(Delegate).IsAssignableFrom(p.PropertyType)))];
        FilterByVisualizeAttribute(ref properties);

        foreach (PropertyInfo property in properties)
        {
            yield return new MemberDescriptor(property, property.PropertyType, property.GetSetMethod(true) != null);
        }
    }

    private static IEnumerable<MemberDescriptor> CollectFieldMembers(Type type, BindingFlags flags)
    {
        string[] propNames = [.. type.GetProperties(flags).Select(p => p.Name)];
        HashSet<string> backingFieldNames = new(
            propNames.Select(n => "_" + char.ToLowerInvariant(n[0]) + n.Substring(1))
        );

        FieldInfo[] fields = [.. type
            .GetFields(flags)
            // Exclude delegate types
            .Where(f => !(typeof(Delegate).IsAssignableFrom(f.FieldType)))
            // Exclude fields created by properties
            .Where(f => !f.Name.StartsWith('<') || !f.Name.EndsWith(">k__BackingField"))
            // Exclude backing fields for properties
            .Where(f => !backingFieldNames.Contains(f.Name))];

        FilterByVisualizeAttribute(ref fields);

        foreach (FieldInfo field in fields)
        {
            yield return new MemberDescriptor(field, field.FieldType, !field.IsLiteral);
        }
    }

    private static IEnumerable<MemberControlBinding> AddMembers(Control vbox, VisualControlContext context, IEnumerable<MemberDescriptor> members)
    {
        List<MemberControlBinding> bindings = [];

        foreach (MemberDescriptor member in members)
        {
            object initialValue = VisualHandler.GetMemberValue(member.Member, context.InitialValue);

            VisualControlInfo control = CreateControlForType(member.MemberType, member.Member, new VisualControlContext(initialValue, v =>
            {
                if (!member.IsEditable)
                {
                    return;
                }

                VisualHandler.SetMemberValue(member.Member, context.InitialValue, v);
                context.ValueChanged(context.InitialValue);
            }));

            if (control.VisualControl == null)
            {
                continue;
            }

            control.VisualControl.SetEditable(member.IsEditable);

            HBoxContainer hbox = CreateHBoxForMember(member.Member.Name, control.VisualControl.Control);
            hbox.Name = member.Member.Name;
            vbox.AddChild(hbox);

            bindings.Add(new MemberControlBinding(member.Member, control.VisualControl, member.IsEditable));
        }

        return bindings;
    }

    private static void AddMethods(BindingFlags flags, Control vbox, Type type, VisualControlContext context)
    {
        // Cannot include private methods or else we will see Godot's built-in methods
        flags &= ~BindingFlags.NonPublic;

        MethodInfo[] methods = [.. type.GetMethods(flags)
            // Exclude delegates
            .Where(m => !(typeof(Delegate).IsAssignableFrom(m.ReturnType)))
            // Exclude auto property methods
            .Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
            // Exclude event add and remove event methods
            .Where(m => !m.Name.StartsWith("add_") && !m.Name.StartsWith("remove_"))
            // Exclude the override string ToString() method
            .Where(m => m.Name != "ToString")];

        FilterByVisualizeAttribute(ref methods);

        foreach (MethodInfo method in methods)
        {
            ParameterInfo[] paramInfos = method.GetParameters();
            object[] providedValues = new object[paramInfos.Length];

            HBoxContainer hboxParams = VisualMethods.CreateMethodParameterControls(method, providedValues);
            Button button = VisualMethods.CreateMethodButton(method, context.InitialValue, paramInfos, providedValues);

            vbox.AddChild(hboxParams);
            vbox.AddChild(button);
        }
    }

    private static void FilterByVisualizeAttribute<T>(ref T[] members) where T : MemberInfo
    {
        // Lets say we are visualizing [Visualize] [Export] public TurretRecoilConfig Recoil { get; set; }
        // The TurretRecoilConfig has an overwhelming amount of properties, so we have implemented it so only
        // properties with the [Visualize] attribute are visualized. Likewise if there are no properties with
        // the [Visualize] attribute, all properties will be visualized.
        List<T> visualizedMembers = [];

        foreach (T member in members)
        {
            if (member.GetCustomAttribute<VisualizeAttribute>() != null)
            {
                visualizedMembers.Add(member);
            }
        }

        // If any properties are marked with [Visualize] then we only visualize those properties.
        if (visualizedMembers.Count != 0)
        {
            members = [.. visualizedMembers];
        }
    }

    private static HBoxContainer CreateHBoxForMember(string memberName, Control control)
    {
        Label label = new() { Text = VisualText.ToDisplayName(memberName) };
        label.CustomMinimumSize = new Vector2(200, 0);

        HBoxContainer hbox = new();
        hbox.AddChild(label);
        hbox.AddChild(control);
        return hbox;
    }

    private sealed class MemberDescriptor(MemberInfo member, Type memberType, bool isEditable)
    {
        public MemberInfo Member { get; } = member;
        public Type MemberType { get; } = memberType;
        public bool IsEditable { get; } = isEditable;
    }

    internal sealed class MemberControlBinding(MemberInfo member, IVisualControl control, bool isEditable)
    {
        public MemberInfo Member { get; } = member;
        public IVisualControl Control { get; } = control;
        public bool IsEditable { get; } = isEditable;
    }
}

internal sealed class ClassControl(Control container, List<VisualControlTypes.MemberControlBinding> bindings) : IVisualControl
{
    public void SetValue(object value)
    {
        foreach (VisualControlTypes.MemberControlBinding binding in bindings)
        {
            object memberValue = VisualHandler.GetMemberValue(binding.Member, value);
            binding.Control.SetValue(memberValue);
        }
    }

    public Control Control => container;

    public void SetEditable(bool editable)
    {
        foreach (VisualControlTypes.MemberControlBinding binding in bindings)
        {
            binding.Control.SetEditable(editable && binding.IsEditable);
        }
    }
}
#endif
