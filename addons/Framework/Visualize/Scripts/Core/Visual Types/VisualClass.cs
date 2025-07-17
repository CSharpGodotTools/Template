using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace __TEMPLATE__;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualClass(Type type, VisualControlContext context)
    {
        VBoxContainer vbox = new();

        if (context.InitialValue == null)
        {
            // Not really sure what side effects this will have but the reason I've done this is because
            // auto properties cannot have default values so their initial value will always be null.
            return new VisualControlInfo(
                new ClassControl(
                    vboxContainer: vbox,
                    visualPropertyControls: [],
                    visualFieldControls: [],
                    properties: [],
                    fields: []
                )
            );

            // Originally this is all I was doing.
            //throw new Exception($"[Visualize] Contexts initial value was null for type '{type}'");
        }

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        AddProperties(flags, vbox, type, context, out List<IVisualControl> propertyControls, out PropertyInfo[] properties);
        AddFields(flags, vbox, type, context, out List<IVisualControl> fieldControls, out FieldInfo[] fields);
        AddMethods(flags, vbox, type, context);

        return new VisualControlInfo(new ClassControl(vbox, propertyControls, fieldControls, properties, fields));
    }

    private static void AddProperties(BindingFlags flags, VBoxContainer vbox, Type type, VisualControlContext context, out List<IVisualControl> propertyControls, out PropertyInfo[] properties)
    {
        propertyControls = [];

        // Get all the class properties
        properties = type.GetProperties(flags)
            .Where(p => !(typeof(Delegate).IsAssignableFrom(p.PropertyType))) // Exclude delegate types
            .ToArray();

        FilterByVisualizeAttribute(ref properties);

        // Create the controls for each property
        foreach (PropertyInfo property in properties)
        {
            object initialValue = property.GetValue(context.InitialValue);

            MethodInfo propertySetMethod = property.GetSetMethod(true);

            VisualControlInfo control = CreateControlForType(property.PropertyType, property, new VisualControlContext(context.SpinBoxes, initialValue, v =>
            {
                property.SetValue(context.InitialValue, v);
                context.ValueChanged(context.InitialValue);
            }));

            if (control.VisualControl != null)
            {
                propertyControls.Add(control.VisualControl);

                control.VisualControl.SetEditable(propertySetMethod != null);

                vbox.AddChild(CreateHBoxForMember(property.Name, control.VisualControl.Control));
            }
        }
    }

    private static void AddFields(BindingFlags flags, VBoxContainer vbox, Type type, VisualControlContext context, out List<IVisualControl> fieldControls, out FieldInfo[] fields)
    {
        fieldControls = [];

        // Grab all the real property names, and turn each into the expected backing‑field name ("_" + lowercase‑first‑char + rest)
        string[] propNames = type.GetProperties(flags).Select(p => p.Name).ToArray();

        HashSet<string> backingFieldNames = new(
            propNames.Select(n =>
                "_" + char.ToLowerInvariant(n[0]) + n.Substring(1)
            )
        );

        // Get all the class fields
        fields = type
            .GetFields(flags)
            // Exclude delegate types
            .Where(f => !(typeof(Delegate).IsAssignableFrom(f.FieldType)))
            // Exclude fields created by properties
            .Where(f => !f.Name.StartsWith('<') || !f.Name.EndsWith(">k__BackingField"))
            // Exclude backing fields for properties
            .Where(f => !backingFieldNames.Contains(f.Name))
            .ToArray();

        FilterByVisualizeAttribute(ref fields);

        foreach (FieldInfo field in fields)
        {
            object initialValue = field.GetValue(context.InitialValue);

            VisualControlInfo control = CreateControlForType(field.FieldType, field, new VisualControlContext(context.SpinBoxes, initialValue, v =>
            {
                field.SetValue(context.InitialValue, v);
                context.ValueChanged(context.InitialValue);
            }));

            if (control.VisualControl != null)
            {
                fieldControls.Add(control.VisualControl);

                control.VisualControl.SetEditable(!field.IsLiteral);

                vbox.AddChild(CreateHBoxForMember(field.Name, control.VisualControl.Control));
            }
        }
    }

    private static void AddMethods(BindingFlags flags, VBoxContainer vbox, Type type, VisualControlContext context)
    {
        // Cannot include private methods or else we will see Godots built in methods
        flags &= ~BindingFlags.NonPublic;

        MethodInfo[] methods = type.GetMethods(flags)
            // Exclude delegates
            .Where(m => !(typeof(Delegate).IsAssignableFrom(m.ReturnType)))
            // Exclude auto property methods
            .Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
            // Exclude event add and remove event methods
            .Where(m => !m.Name.StartsWith("add_") && !m.Name.StartsWith("remove_"))
            // Exclude the override string ToString() method
            .Where(m => m.Name != "ToString")
            .ToArray();

        FilterByVisualizeAttribute(ref methods);

        foreach (MethodInfo method in methods)
        {
            ParameterInfo[] paramInfos = method.GetParameters();
            object[] providedValues = new object[paramInfos.Length];

            HBoxContainer hboxParams = VisualMethods.CreateMethodParameterControls(method, context.SpinBoxes, providedValues);
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
            members = visualizedMembers.ToArray();
        }
    }

    private static HBoxContainer CreateHBoxForMember(string memberName, Control control)
    {
        HBoxContainer hbox = new();
        hbox.AddChild(new Label { Text = memberName.ToPascalCase().AddSpaceBeforeEachCapital() });
        hbox.AddChild(control);
        return hbox;
    }
}

public class ClassControl(VBoxContainer vboxContainer, List<IVisualControl> visualPropertyControls, List<IVisualControl> visualFieldControls, PropertyInfo[] properties, FieldInfo[] fields) : IVisualControl
{
    public void SetValue(object value)
    {
        for (int i = 0; i < properties.Length; i++)
        {
            object propValue = properties[i].GetValue(value);
            visualPropertyControls[i].SetValue(propValue);
        }

        for (int i = 0; i < fields.Length; i++)
        {
            object fieldValue = fields[i].GetValue(value);
            visualFieldControls[i].SetValue(fieldValue);
        }
    }

    public Control Control => vboxContainer;

    public void SetEditable(bool editable)
    {
        foreach (IVisualControl visualControl in visualPropertyControls)
        {
            visualControl.SetEditable(editable);
        }

        foreach (IVisualControl visualControl in visualFieldControls)
        {
            visualControl.SetEditable(editable);
        }
    }
}
