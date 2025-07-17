using Godot;

namespace __TEMPLATE__;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector4I(VisualControlContext context)
    {
        HBoxContainer vector4IHBox = new();

        Vector4I vector4I = (Vector4I)context.InitialValue;

        SpinBox spinBoxX = CreateSpinBox(typeof(int));
        SpinBox spinBoxY = CreateSpinBox(typeof(int));
        SpinBox spinBoxZ = CreateSpinBox(typeof(int));
        SpinBox spinBoxW = CreateSpinBox(typeof(int));

        spinBoxX.Value = vector4I.X;
        spinBoxY.Value = vector4I.Y;
        spinBoxZ.Value = vector4I.Z;
        spinBoxW.Value = vector4I.W;

        spinBoxX.ValueChanged += value =>
        {
            vector4I.X = (int)value;
            context.ValueChanged(vector4I);
        };

        spinBoxY.ValueChanged += value =>
        {
            vector4I.Y = (int)value;
            context.ValueChanged(vector4I);
        };

        spinBoxZ.ValueChanged += value =>
        {
            vector4I.Z = (int)value;
            context.ValueChanged(vector4I);
        };

        spinBoxW.ValueChanged += value =>
        {
            vector4I.W = (int)value;
            context.ValueChanged(vector4I);
        };

        vector4IHBox.AddChild(new Label { Text = "X" });
        vector4IHBox.AddChild(spinBoxX);
        vector4IHBox.AddChild(new Label { Text = "Y" });
        vector4IHBox.AddChild(spinBoxY);
        vector4IHBox.AddChild(new Label { Text = "Z" });
        vector4IHBox.AddChild(spinBoxZ);
        vector4IHBox.AddChild(new Label { Text = "W" });
        vector4IHBox.AddChild(spinBoxW);

        return new VisualControlInfo(new Vector4IControl(vector4IHBox, spinBoxX, spinBoxY, spinBoxZ, spinBoxW));
    }
}

public class Vector4IControl(HBoxContainer vector4IHBox, SpinBox spinBoxX, SpinBox spinBoxY, SpinBox spinBoxZ, SpinBox spinBoxW) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is Vector4I vector4I)
        {
            spinBoxX.Value = vector4I.X;
            spinBoxY.Value = vector4I.Y;
            spinBoxZ.Value = vector4I.Z;
            spinBoxW.Value = vector4I.W;
        }
    }

    public Control Control => vector4IHBox;

    public void SetEditable(bool editable)
    {
        spinBoxX.Editable = editable;
        spinBoxY.Editable = editable;
        spinBoxZ.Editable = editable;
        spinBoxW.Editable = editable;
    }
}
