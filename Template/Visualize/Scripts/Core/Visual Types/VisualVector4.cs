using Godot;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector4(VisualControlContext context)
    {
        HBoxContainer vector4HBox = new();

        Vector4 vector4 = (Vector4)context.InitialValue;

        SpinBox spinBoxX = CreateSpinBox(typeof(float), null);
        SpinBox spinBoxY = CreateSpinBox(typeof(float), null);
        SpinBox spinBoxZ = CreateSpinBox(typeof(float), null);
        SpinBox spinBoxW = CreateSpinBox(typeof(float), null);

        spinBoxX.Value = vector4.X;
        spinBoxY.Value = vector4.Y;
        spinBoxZ.Value = vector4.Z;
        spinBoxW.Value = vector4.W;

        spinBoxX.ValueChanged += value =>
        {
            vector4.X = (float)value;
            context.ValueChanged(vector4);
        };

        spinBoxY.ValueChanged += value =>
        {
            vector4.Y = (float)value;
            context.ValueChanged(vector4);
        };

        spinBoxZ.ValueChanged += value =>
        {
            vector4.Z = (float)value;
            context.ValueChanged(vector4);
        };

        spinBoxW.ValueChanged += value =>
        {
            vector4.W = (float)value;
            context.ValueChanged(vector4);
        };

        vector4HBox.AddChild(new Label { Text = "X" });
        vector4HBox.AddChild(spinBoxX);
        vector4HBox.AddChild(new Label { Text = "Y" });
        vector4HBox.AddChild(spinBoxY);
        vector4HBox.AddChild(new Label { Text = "Z" });
        vector4HBox.AddChild(spinBoxZ);
        vector4HBox.AddChild(new Label { Text = "W" });
        vector4HBox.AddChild(spinBoxW);

        return new VisualControlInfo(new Vector4Control(vector4HBox, spinBoxX, spinBoxY, spinBoxZ, spinBoxW));
    }
}

public class Vector4Control(HBoxContainer vector4HBox, SpinBox spinBoxX, SpinBox spinBoxY, SpinBox spinBoxZ, SpinBox spinBoxW) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is Vector4 vector4)
        {
            spinBoxX.Value = vector4.X;
            spinBoxY.Value = vector4.Y;
            spinBoxZ.Value = vector4.Z;
            spinBoxW.Value = vector4.W;
        }
    }

    public Control Control => vector4HBox;

    public void SetEditable(bool editable)
    {
        spinBoxX.Editable = editable;
        spinBoxY.Editable = editable;
        spinBoxZ.Editable = editable;
        spinBoxW.Editable = editable;
    }
}
