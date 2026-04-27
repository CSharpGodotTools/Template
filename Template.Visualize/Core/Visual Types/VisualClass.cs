#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GodotUtils.Debugging;

/// <summary>
/// Class/struct control builders for reflected members and visualize-marked methods.
/// </summary>
internal static partial class VisualControlTypes
{
    private const int ClassControlColumns = 1;
    private const string GetterPrefix = "get_";
    private const string SetterPrefix = "set_";
    private const string EventAddPrefix = "add_";
    private const string EventRemovePrefix = "remove_";
    private const string ToStringMethodName = "ToString";

    /// <summary>
    /// Creates a composite class control for reflected properties, fields, and methods.
    /// </summary>
    /// <param name="type">Class or struct type to render.</param>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Created class-control info.</returns>
    private static VisualControlInfo VisualClass(Type type, VisualControlContext context)
    {
        GridContainer container = new() { Columns = ClassControlColumns, SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin };

        // Return an empty control when there is no object instance to inspect.
        if (context.InitialValue == null)
            return new VisualControlInfo(new ClassControl(container, []));

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        List<MemberControlBinding> memberBindings = [];
        memberBindings.AddRange(AddMembers(container, context, CollectPropertyMembers(type, flags)));
        memberBindings.AddRange(AddMembers(container, context, CollectFieldMembers(type, flags)));
        AddMethods(flags, container, type, context);

        return new VisualControlInfo(new ClassControl(container, memberBindings));
    }

    /// <summary>
    /// Collects candidate property members and editability metadata.
    /// </summary>
    /// <param name="type">Type to inspect.</param>
    /// <param name="flags">Binding flags for reflection lookup.</param>
    /// <returns>Property member descriptors.</returns>
    private static IEnumerable<MemberDescriptor> CollectPropertyMembers(Type type, BindingFlags flags)
    {
        PropertyInfo[] properties = [.. type.GetProperties(flags).Where(p => !typeof(Delegate).IsAssignableFrom(p.PropertyType))];
        FilterByVisualizeAttribute(ref properties);

        foreach (PropertyInfo property in properties)
            yield return new MemberDescriptor(property, property.PropertyType, property.GetSetMethod(true) != null);
    }

    /// <summary>
    /// Collects candidate field members and editability metadata.
    /// </summary>
    /// <param name="type">Type to inspect.</param>
    /// <param name="flags">Binding flags for reflection lookup.</param>
    /// <returns>Field member descriptors.</returns>
    private static IEnumerable<MemberDescriptor> CollectFieldMembers(Type type, BindingFlags flags)
    {
        string[] propNames = [.. type.GetProperties(flags).Select(p => p.Name)];
        HashSet<string> backingFieldNames = [.. propNames.Select(n => "_" + char.ToLowerInvariant(n[0]) + n[1..])];

        FieldInfo[] fields = [.. type
            .GetFields(flags)
            // Exclude delegate types
            .Where(f => !typeof(Delegate).IsAssignableFrom(f.FieldType) && (!f.Name.StartsWith('<') || !f.Name.EndsWith(">k__BackingField")) && !backingFieldNames.Contains(f.Name))
            // Exclude fields created by properties
            
            // Exclude backing fields for properties
            ];

        FilterByVisualizeAttribute(ref fields);

        foreach (FieldInfo field in fields)
            yield return new MemberDescriptor(field, field.FieldType, !field.IsLiteral);
    }

    /// <summary>
    /// Adds reflected members as editable/non-editable controls to the target container.
    /// </summary>
    /// <param name="vbox">Container that receives member rows.</param>
    /// <param name="context">Initial value and change callback context.</param>
    /// <param name="members">Member descriptors to render.</param>
    /// <returns>Bindings used for subsequent value/editability synchronization.</returns>
    private static List<MemberControlBinding> AddMembers(Control vbox, VisualControlContext context, IEnumerable<MemberDescriptor> members)
    {
        List<MemberControlBinding> bindings = [];

        foreach (MemberDescriptor member in members)
        {
            object? initialValue = VisualHandler.GetMemberValue(member.Member, context.InitialValue);

            VisualControlInfo control = CreateControlForType(member.MemberType, member.Member, new VisualControlContext(initialValue, v =>
            {
                // Ignore writes for members that are intentionally readonly.
                if (!member.IsEditable)
                    return;

                VisualHandler.SetMemberValue(member.Member, context.InitialValue!, v);
                context.ValueChanged(context.InitialValue!);
            }));

            // Skip rows for member types that have no supported visual editor.
            if (control.VisualControl == null)
                continue;

            control.VisualControl.SetEditable(member.IsEditable);

            HBoxContainer hbox = CreateHBoxForMember(member.Member.Name, control.VisualControl.Control);
            hbox.Name = member.Member.Name;
            vbox.AddChild(hbox);

            bindings.Add(new MemberControlBinding(member.Member, control.VisualControl, member.IsEditable));
        }

        return bindings;
    }

    /// <summary>
    /// Adds visualize-marked methods as invoke buttons.
    /// </summary>
    /// <param name="flags">Binding flags used for method discovery.</param>
    /// <param name="vbox">Container that receives method buttons.</param>
    /// <param name="type">Type to inspect.</param>
    /// <param name="context">Initial value and change callback context.</param>
    private static void AddMethods(BindingFlags flags, Control vbox, Type type, VisualControlContext context)
    {
        // Cannot include private methods or else we will see Godot's built-in methods
        flags &= ~BindingFlags.NonPublic;

        MethodInfo[] methods = [.. type.GetMethods(flags)
            // Exclude delegates
            .Where(m => !typeof(Delegate).IsAssignableFrom(m.ReturnType) && !m.Name.StartsWith(GetterPrefix) && !m.Name.StartsWith(SetterPrefix) && !m.Name.StartsWith(EventAddPrefix) && !m.Name.StartsWith(EventRemovePrefix) && m.Name != ToStringMethodName)
            // Exclude auto property methods
            
            // Exclude event add and remove event methods
            
            // Exclude the override string ToString() method
            ];

        FilterByVisualizeAttribute(ref methods);

        foreach (MethodInfo method in methods)
        {
            ParameterInfo[] paramInfos = method.GetParameters();
            object[] providedValues = new object[paramInfos.Length];
            Button button = VisualMethods.CreateMethodButton(method, context.InitialValue!, paramInfos, providedValues);
            vbox.AddChild(button);
        }
    }

    /// <summary>
    /// Filters member arrays to only visualize-marked members when at least one exists.
    /// </summary>
    /// <typeparam name="T">Member info type.</typeparam>
    /// <param name="members">Members to filter.</param>
    private static void FilterByVisualizeAttribute<T>(ref T[] members) where T : MemberInfo
    {
        // Lets say we are visualizing [Visualize] [Export] public TurretRecoilConfig Recoil { get; set; }
        // The TurretRecoilConfig has an overwhelming amount of properties, so we have implemented it so only
        // properties with the [Visualize] attribute are visualized. Likewise if there are no properties with
        // the [Visualize] attribute, all properties will be visualized.
        List<T> visualizedMembers = [];

        foreach (T member in members)
        {
            // Keep only members explicitly marked for visualization.
            if (member.GetCustomAttribute<VisualizeAttribute>() != null)
                visualizedMembers.Add(member);
        }

        // If any properties are marked with [Visualize] then we only visualize those properties.
        if (visualizedMembers.Count != 0)
            members = [.. visualizedMembers];
    }

    /// <summary>
    /// Creates a standard label-plus-control row for member rendering.
    /// </summary>
    /// <param name="memberName">Member name used as row label.</param>
    /// <param name="control">Member value control.</param>
    /// <returns>Configured row container.</returns>
    private static HBoxContainer CreateHBoxForMember(string memberName, Control control)
    {
        Label label = new()
        {
            Text = VisualText.ToDisplayName(memberName),
            HorizontalAlignment = HorizontalAlignment.Right
        };

        HBoxContainer hbox = new() { Alignment = BoxContainer.AlignmentMode.End };
        hbox.AddThemeConstantOverride("separation", 8);
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

}
#endif
