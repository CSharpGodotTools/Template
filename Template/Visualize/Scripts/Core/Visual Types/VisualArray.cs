using Godot;
using System;
using static Godot.Control;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualArray(Type type, VisualControlContext context)
    {
        VBoxContainer arrayVBox = new() { SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand };
        Button addButton = new() { Text = "+" };

        Type elementType = type.GetElementType();
        Array array = context.InitialValue as Array ?? Array.CreateInstance(elementType, 0);

        void AddNewEntryToArray()
        {
            Array newArray = Array.CreateInstance(elementType, array.Length + 1);
            Array.Copy(array, newArray, array.Length);
            array = newArray;
            context.ValueChanged(array);

            object newValue = VisualMethods.CreateDefaultValue(elementType);
            int newIndex = array.Length - 1;

            VisualControlInfo control = CreateControlForType(elementType, null, new VisualControlContext(context.SpinBoxes, newValue, v =>
            {
                array.SetValue(v, newIndex);
                context.ValueChanged(array);
            }));

            if (control.VisualControl != null)
            {
                Button minusButton = new() { Text = "-" };
                HBoxContainer hbox = new();

                minusButton.Pressed += () =>
                {
                    int indexToRemove = minusButton.GetParent().GetIndex();
                    arrayVBox.RemoveChild(hbox);
                    array = array.RemoveAt(indexToRemove);
                    context.ValueChanged(array);
                };

                hbox.AddChild(control.VisualControl.Control);
                hbox.AddChild(minusButton);
                arrayVBox.AddChild(hbox);
                arrayVBox.MoveChild(addButton, arrayVBox.GetChildCount() - 1);
            }
        }

        for (int i = 0; i < array.Length; i++)
        {
            object value = array.GetValue(i);

            VisualControlInfo control = CreateControlForType(elementType, null, new VisualControlContext(context.SpinBoxes, value, v =>
            {
                array.SetValue(v, i);
                context.ValueChanged(array);
            }));

            if (control.VisualControl != null)
            {
                SetControlValue(control.VisualControl.Control, value);

                Button minusButton = new() { Text = "-" };
                HBoxContainer hbox = new();

                minusButton.Pressed += () =>
                {
                    int indexToRemove = minusButton.GetParent().GetIndex();
                    arrayVBox.RemoveChild(hbox);
                    array = array.RemoveAt(indexToRemove);
                    context.ValueChanged(array);
                };

                hbox.AddChild(control.VisualControl.Control);
                hbox.AddChild(minusButton);
                arrayVBox.AddChild(hbox);
            }
        }

        addButton.Pressed += AddNewEntryToArray;
        arrayVBox.AddChild(addButton);

        return new VisualControlInfo(new VBoxContainerControl(arrayVBox));
    }
}

public class VBoxContainerControl(VBoxContainer vboxContainer) : IVisualControl
{
    public void SetValue(object value)
    {
        // No specific value setting for VBoxContainer
    }

    public Control Control => vboxContainer;

    public void SetEditable(bool editable)
    {
        // No specific editable setting for VBoxContainer
    }
}
