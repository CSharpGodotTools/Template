#if DEBUG
using Godot;
using System;
using static Godot.Control;

namespace GodotUtils.Debugging;

/// <summary>
/// Shared utility helpers used by visual control factory implementations.
/// </summary>
internal static partial class VisualControlTypes
{
    private const double FloatStep = 0.1;
    private const double DecimalStep = 0.01;
    private const double IntStep = 1;

    /// <summary>
    /// Registers cleanup logic that runs once when a node exits the scene tree.
    /// </summary>
    /// <param name="node">Node whose exit triggers cleanup.</param>
    /// <param name="cleanup">Cleanup callback to execute.</param>
    private static void CleanupOnTreeExited(Node node, Action cleanup)
    {
        void OnTreeExited()
        {
            cleanup();
            node.TreeExited -= OnTreeExited;
        }

        node.TreeExited += OnTreeExited;
    }

    /// <summary>
    /// Returns a new array with the element at <paramref name="index"/> removed.
    /// </summary>
    /// <param name="source">Source array.</param>
    /// <param name="index">Zero-based index to remove.</param>
    /// <returns>New array without the removed element.</returns>
    private static Array RemoveAt(this Array source, int index)
    {
        ArgumentNullException.ThrowIfNull(source);

        // Guard against invalid indices before copying array segments.
        if (index < 0 || index >= source.Length)
            throw new ArgumentOutOfRangeException(nameof(index), index, "[Visualize] Index was out of range");

        Array dest = Array.CreateInstance(source.GetType().GetElementType()!, source.Length - 1);
        Array.Copy(source, 0, dest, 0, index);
        Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    /// <summary>
    /// Returns a new array with <paramref name="value"/> appended.
    /// </summary>
    /// <param name="source">Source array.</param>
    /// <param name="value">Value to append.</param>
    /// <returns>New array including appended value.</returns>
    private static Array Append(Array source, object value)
    {
        ArgumentNullException.ThrowIfNull(source);

        Type elementType = source.GetType().GetElementType()!;
        Array dest = Array.CreateInstance(elementType, source.Length + 1);
        Array.Copy(source, dest, source.Length);
        dest.SetValue(value, dest.Length - 1);

        return dest;
    }

    /// <summary>
    /// Creates a configured spin box for a numeric type.
    /// </summary>
    /// <param name="type">Numeric type represented by the spin box.</param>
    /// <returns>Configured spin box.</returns>
    private static SpinBox CreateSpinBox(Type type)
    {
        SpinBox spinBox = new()
        {
            UpdateOnTextChanged = true,
            AllowLesser = false,
            AllowGreater = false,
            MinValue = int.MinValue,
            MaxValue = int.MaxValue,
            Alignment = HorizontalAlignment.Center,
            Step = type switch
            {
                _ when type == typeof(float) => FloatStep,
                _ when type == typeof(double) => FloatStep,
                _ when type == typeof(decimal) => DecimalStep,
                _ when type == typeof(int) => IntStep,
                _ => IntStep
            }
        };

        return spinBox;
    }

    /// <summary>
    /// Creates a text-based control adapter using custom parse/stringify functions.
    /// </summary>
    /// <param name="context">Initial value and change callback context.</param>
    /// <param name="parse">Converts edited text to value.</param>
    /// <param name="stringify">Converts value to display text.</param>
    /// <returns>Created text-control info.</returns>
    private static VisualControlInfo CreateTextControl(VisualControlContext context, Func<string, object> parse, Func<object, string> stringify)
    {
        string initialText = stringify(context.InitialValue!);
        LineEdit lineEdit = new() { Text = initialText };

        void OnTextChanged(string text) => context.ValueChanged(parse(text));

        lineEdit.TextChanged += OnTextChanged;
        CleanupOnTreeExited(lineEdit, () => lineEdit.TextChanged -= OnTextChanged);

        return new VisualControlInfo(new TextControl(lineEdit, stringify));
    }

    /// <summary>
    /// Creates a multi-spin-box control for vector-like composite values.
    /// </summary>
    /// <typeparam name="T">Composite value type.</typeparam>
    /// <param name="context">Initial value and change callback context.</param>
    /// <param name="labels">Component labels.</param>
    /// <param name="componentType">Numeric type used by component spin boxes.</param>
    /// <param name="getComponents">Function that reads component values.</param>
    /// <param name="setComponent">Function that writes one component value.</param>
    /// <returns>Created composite control info.</returns>
    private static VisualControlInfo CreateVectorControl<T>(
        VisualControlContext context,
        string[] labels,
        Type componentType,
        Func<T, double[]> getComponents,
        Func<T, int, double, T> setComponent)
        where T : notnull
    {
        HBoxContainer container = new();
        T currentValue = (T)context.InitialValue!;
        double[] components = getComponents(currentValue);
        SpinBox[] spinBoxes = new SpinBox[labels.Length];

        for (int i = 0; i < labels.Length; i++)
        {
            SpinBox spinBox = CreateSpinBox(componentType);
            spinBox.Value = components[i];

            int index = i;

            void OnValueChanged(double value)
            {
                currentValue = setComponent(currentValue, index, value);
                context.ValueChanged(currentValue);
            }

            spinBox.ValueChanged += OnValueChanged;
            CleanupOnTreeExited(spinBox, () => spinBox.ValueChanged -= OnValueChanged);

            container.AddChild(new Label { Text = labels[i] });
            container.AddChild(spinBox);
            spinBoxes[i] = spinBox;
        }

        return new VisualControlInfo(new MultiSpinBoxControl<T>(container, spinBoxes, getComponents));
    }

    /// <summary>
    /// Creates a reusable editable UI for indexed collections (arrays/lists/dictionaries values).
    /// </summary>
    /// <param name="elementType">Collection element type.</param>
    /// <param name="getCount">Returns current item count.</param>
    /// <param name="getValue">Reads item by index.</param>
    /// <param name="setValue">Writes item by index.</param>
    /// <param name="addValue">Adds a new item.</param>
    /// <param name="removeValue">Removes item by index.</param>
    /// <param name="getCollectionValue">Returns current collection value.</param>
    /// <param name="setCollectionValue">Optional collection replacement callback.</param>
    /// <param name="context">Initial value and change callback context.</param>
    /// <returns>Container-based visual control info.</returns>
    private static VisualControlInfo CreateIndexedCollectionControl(
        Type elementType,
        Func<int> getCount,
        Func<int, object?> getValue,
        Action<int, object> setValue,
        Action<object> addValue,
        Action<int> removeValue,
        Func<object> getCollectionValue,
        Action<object>? setCollectionValue,
        VisualControlContext context)
    {
        VBoxContainer listVBox = new() { SizeFlagsHorizontal = SizeFlags.ShrinkEnd };
        Button addButton = new() { Text = "+" };
        const string IndexMetaKey = "Visualize_Index";
        // Shared between mutable and readonly views so controls can be toggled at runtime.
        bool isEditable = true;

        void AddEntry(object? value, int index)
        {
            HBoxContainer row = new();
            row.SetMeta(IndexMetaKey, index);
            VisualControlInfo control = CreateControlForType(elementType, null, new VisualControlContext(value, v =>
            {
                int currentIndex = GetRowIndex(row);
                setValue(currentIndex, v);
                context.ValueChanged(getCollectionValue());
            }));

            // Skip rows when no visual editor can be created for the element type.
            if (control.VisualControl == null)
                return;

            control.VisualControl.SetValue(value!);
            control.VisualControl.SetEditable(isEditable);

            Button removeButton = new() { Text = "-", Disabled = !isEditable };

            void OnRemovePressed()
            {
                // Ignore remove clicks while the collection is in readonly mode.
                if (!isEditable)
                    return;

                int currentIndex = GetRowIndex(row);
                listVBox.RemoveChild(row);
                row.QueueFree();
                removeValue(currentIndex);
                context.ValueChanged(getCollectionValue());
                UpdateIndicesAfterRemoval(currentIndex);
            }

            removeButton.Pressed += OnRemovePressed;
            CleanupOnTreeExited(removeButton, () => removeButton.Pressed -= OnRemovePressed);

            row.AddChild(control.VisualControl.Control);
            row.AddChild(removeButton);
            listVBox.AddChild(row);
        }

        for (int i = 0; i < getCount(); i++)
            AddEntry(getValue(i), i);

        void OnAddPressed()
        {
            // Ignore add clicks while the collection is in readonly mode.
            if (!isEditable)
                return;

            object newValue = VisualMethods.CreateDefaultValue(elementType);
            addValue(newValue);
            context.ValueChanged(getCollectionValue());
            AddEntry(newValue, getCount() - 1);
            listVBox.MoveChild(addButton, listVBox.GetChildCount() - 1);
        }

        addButton.Pressed += OnAddPressed;
        CleanupOnTreeExited(addButton, () => addButton.Pressed -= OnAddPressed);

        listVBox.AddChild(addButton);

        return new VisualControlInfo(new VBoxContainerControl(
            listVBox,
            // Readonly polling refreshes the rendered rows from the latest collection state.
            value =>
            {
                setCollectionValue?.Invoke(value);
                RefreshEntries();
            },
            editable =>
            {
                isEditable = editable;
                addButton.Disabled = !editable;
                RefreshEntries();
            }));

        void UpdateIndicesAfterRemoval(int removedIndex)
        {
            foreach (Node child in listVBox.GetChildren())
            {
                // Only indexed entry rows participate in index re-numbering.
                if (child is not HBoxContainer row || !row.HasMeta(IndexMetaKey))
                    continue;

                int currentIndex = GetRowIndex(row);

                // Shift rows that were after the removed element.
                if (currentIndex > removedIndex)
                    row.SetMeta(IndexMetaKey, currentIndex - 1);
            }
        }

        static int GetRowIndex(HBoxContainer row)
        {
            // Metadata is stored as Variant in Godot.
            Variant indexVariant = row.GetMeta(IndexMetaKey);
            return indexVariant.AsInt32();
        }

        void RefreshEntries()
        {
            foreach (Node child in listVBox.GetChildren())
            {
                // Keep the add button and rebuild only entry rows.
                if (child == addButton)
                    continue;

                listVBox.RemoveChild(child);
                child.QueueFree();
            }

            for (int i = 0; i < getCount(); i++)
                AddEntry(getValue(i), i);

            listVBox.MoveChild(addButton, listVBox.GetChildCount() - 1);
        }
    }
}
#endif
