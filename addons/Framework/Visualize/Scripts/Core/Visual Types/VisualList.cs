using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using static Godot.Control;

namespace __TEMPLATE__;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualList(Type type, VisualControlContext context)
    {
        VBoxContainer listVBox = new() { SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand };
        Button addButton = new() { Text = "+" };

        Type elementType = type.GetGenericArguments()[0];
        IList list = context.InitialValue as IList ?? (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

        void AddNewEntryToList()
        {
            object newValue = VisualMethods.CreateDefaultValue(elementType);
            list.Add(newValue);
            context.ValueChanged(list);

            int newIndex = list.Count - 1;

            VisualControlInfo control = CreateControlForType(elementType, null, new VisualControlContext(context.SpinBoxes, newValue, v =>
            {
                list[newIndex] = v;
                context.ValueChanged(list);
            }));

            if (control.VisualControl != null)
            {
                HBoxContainer hbox = new();
                Button minusButton = new() { Text = "-" };

                minusButton.Pressed += () =>
                {
                    int indexToRemove = minusButton.GetParent().GetIndex();
                    listVBox.RemoveChild(hbox);
                    list.RemoveAt(indexToRemove);
                    context.ValueChanged(list);
                };

                hbox.AddChild(control.VisualControl.Control);
                hbox.AddChild(minusButton);
                listVBox.AddChild(hbox);
                listVBox.MoveChild(addButton, listVBox.GetChildCount() - 1);
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            object value = list[i];

            int i1 = i;
            VisualControlInfo control = CreateControlForType(elementType, null, new VisualControlContext(context.SpinBoxes, value, v =>
            {
                list[i1] = v;
                context.ValueChanged(list);
            }));

            if (control.VisualControl != null)
            {
                SetControlValue(control.VisualControl.Control, value);

                Button minusButton = new() { Text = "-" };
                HBoxContainer hbox = new();

                minusButton.Pressed += () =>
                {
                    int indexToRemove = minusButton.GetParent().GetIndex();
                    listVBox.RemoveChild(hbox);
                    list.RemoveAt(indexToRemove);
                    context.ValueChanged(list);
                };

                hbox.AddChild(control.VisualControl.Control);
                hbox.AddChild(minusButton);
                listVBox.AddChild(hbox);
            }
        }

        addButton.Pressed += AddNewEntryToList;
        listVBox.AddChild(addButton);

        return new VisualControlInfo(new VBoxContainerControl(listVBox));
    }
}
