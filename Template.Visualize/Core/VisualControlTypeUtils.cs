#if DEBUG
using Godot;
using System;
using static Godot.Control;

namespace GodotUtils.Debugging;

/// <summary>
/// More utility methods
/// </summary>
internal static partial class VisualControlTypes
{
    // Helper method to remove an element from an array
    private static Array RemoveAt(this Array source, int index)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (index < 0 || index >= source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"[Visualize] Index was out of range");
        }

        Array dest = Array.CreateInstance(source.GetType().GetElementType(), source.Length - 1);
        Array.Copy(source, 0, dest, 0, index);
        Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    private static Array Append(Array source, object value)
    {
        ArgumentNullException.ThrowIfNull(source);

        Type elementType = source.GetType().GetElementType();
        Array dest = Array.CreateInstance(elementType, source.Length + 1);
        Array.Copy(source, dest, source.Length);
        dest.SetValue(value, dest.Length - 1);

        return dest;
    }

    private static SpinBox CreateSpinBox(Type type)
    {
        SpinBox spinBox = new()
        {
            UpdateOnTextChanged = true,
            AllowLesser = false,
            AllowGreater = false,
            MinValue = int.MinValue,
            MaxValue = int.MaxValue,
            Alignment = HorizontalAlignment.Center
        };

        spinBox.Step = type switch
        {
            _ when type == typeof(float) => 0.1,
            _ when type == typeof(double) => 0.1,
            _ when type == typeof(decimal) => 0.01,
            _ when type == typeof(int) => 1,
            _ => 1
        };

        return spinBox;
    }

    private static VisualControlInfo CreateTextControl(VisualControlContext context, Func<string, object> parse, Func<object, string> stringify)
    {
        string initialText = stringify(context.InitialValue);
        LineEdit lineEdit = new() { Text = initialText };
        lineEdit.TextChanged += text => context.ValueChanged(parse(text));

        return new VisualControlInfo(new TextControl(lineEdit, stringify));
    }

    private static VisualControlInfo CreateVectorControl<T>(
        VisualControlContext context,
        string[] labels,
        Type componentType,
        Func<T, double[]> getComponents,
        Func<T, int, double, T> setComponent)
    {
        HBoxContainer container = new();
        T currentValue = (T)context.InitialValue;
        double[] components = getComponents(currentValue);
        SpinBox[] spinBoxes = new SpinBox[labels.Length];

        for (int i = 0; i < labels.Length; i++)
        {
            SpinBox spinBox = CreateSpinBox(componentType);
            spinBox.Value = components[i];

            int index = i;
            spinBox.ValueChanged += value =>
            {
                currentValue = setComponent(currentValue, index, value);
                context.ValueChanged(currentValue);
            };

            container.AddChild(new Label { Text = labels[i] });
            container.AddChild(spinBox);
            spinBoxes[i] = spinBox;
        }

        return new VisualControlInfo(new MultiSpinBoxControl<T>(container, spinBoxes, getComponents));
    }

    private static VisualControlInfo CreateIndexedCollectionControl(
        Type elementType,
        Func<int> getCount,
        Func<int, object> getValue,
        Action<int, object> setValue,
        Action<object> addValue,
        Action<int> removeValue,
        Func<object> getCollectionValue,
        VisualControlContext context)
    {
        VBoxContainer listVBox = new() { SizeFlagsHorizontal = SizeFlags.ShrinkEnd | SizeFlags.Expand };
        Button addButton = new() { Text = "+" };
        const string IndexMetaKey = "Visualize_Index";

        void AddEntry(object value, int index)
        {
            HBoxContainer row = new();
            row.SetMeta(IndexMetaKey, index);
            VisualControlInfo control = CreateControlForType(elementType, null, new VisualControlContext(value, v =>
            {
                int currentIndex = Convert.ToInt32(row.GetMeta(IndexMetaKey));
                setValue(currentIndex, v);
                context.ValueChanged(getCollectionValue());
            }));

            if (control.VisualControl == null)
                return;

            control.VisualControl.SetValue(value);

            Button removeButton = new() { Text = "-" };
            removeButton.Pressed += () =>
            {
                int currentIndex = Convert.ToInt32(row.GetMeta(IndexMetaKey));
                listVBox.RemoveChild(row);
                removeValue(currentIndex);
                context.ValueChanged(getCollectionValue());
                UpdateIndicesAfterRemoval(currentIndex);
            };

            row.AddChild(control.VisualControl.Control);
            row.AddChild(removeButton);
            listVBox.AddChild(row);
        }

        for (int i = 0; i < getCount(); i++)
        {
            AddEntry(getValue(i), i);
        }

        addButton.Pressed += () =>
        {
            object newValue = VisualMethods.CreateDefaultValue(elementType);
            addValue(newValue);
            context.ValueChanged(getCollectionValue());
            AddEntry(newValue, getCount() - 1);
            listVBox.MoveChild(addButton, listVBox.GetChildCount() - 1);
        };

        listVBox.AddChild(addButton);

        return new VisualControlInfo(new VBoxContainerControl(listVBox));

        void UpdateIndicesAfterRemoval(int removedIndex)
        {
            foreach (Node child in listVBox.GetChildren())
            {
                if (child is not HBoxContainer row || !row.HasMeta(IndexMetaKey))
                {
                    continue;
                }

                int currentIndex = Convert.ToInt32(row.GetMeta(IndexMetaKey));
                if (currentIndex > removedIndex)
                {
                    row.SetMeta(IndexMetaKey, currentIndex - 1);
                }
            }
        }
    }
}

internal sealed class TextControl(LineEdit lineEdit, Func<object, string> stringify) : IVisualControl
{
    public void SetValue(object value)
    {
        lineEdit.Text = stringify(value);
    }

    public Control Control => lineEdit;

    public void SetEditable(bool editable)
    {
        lineEdit.Editable = editable;
    }
}

internal sealed class MultiSpinBoxControl<T>(Control container, SpinBox[] spinBoxes, Func<T, double[]> getComponents) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is not T typedValue)
            return;

        double[] components = getComponents(typedValue);

        for (int i = 0; i < spinBoxes.Length && i < components.Length; i++)
        {
            spinBoxes[i].Value = components[i];
        }
    }

    public Control Control => container;

    public void SetEditable(bool editable)
    {
        foreach (SpinBox spinBox in spinBoxes)
        {
            spinBox.Editable = editable;
        }
    }
}

internal sealed class VBoxContainerControl(VBoxContainer vboxContainer) : IVisualControl
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
#endif
