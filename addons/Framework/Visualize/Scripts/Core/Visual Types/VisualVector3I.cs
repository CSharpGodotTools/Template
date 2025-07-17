using Godot;

namespace __TEMPLATE__;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector3I(VisualControlContext context)
    {
        HBoxContainer vector3IHBox = new();

        Vector3I vector3I = (Vector3I)context.InitialValue;

        SpinBox spinBoxX = CreateSpinBox(typeof(int));
        SpinBox spinBoxY = CreateSpinBox(typeof(int));
        SpinBox spinBoxZ = CreateSpinBox(typeof(int));

        spinBoxX.Value = vector3I.X;
        spinBoxY.Value = vector3I.Y;
        spinBoxZ.Value = vector3I.Z;

        spinBoxX.ValueChanged += value =>
        {
            vector3I.X = (int)value;
            context.ValueChanged(vector3I);
        };

        spinBoxY.ValueChanged += value =>
        {
            vector3I.Y = (int)value;
            context.ValueChanged(vector3I);
        };

        spinBoxZ.ValueChanged += value =>
        {
            vector3I.Z = (int)value;
            context.ValueChanged(vector3I);
        };

        vector3IHBox.AddChild(new Label { Text = "X" });
        vector3IHBox.AddChild(spinBoxX);
        vector3IHBox.AddChild(new Label { Text = "Y" });
        vector3IHBox.AddChild(spinBoxY);
        vector3IHBox.AddChild(new Label { Text = "Z" });
        vector3IHBox.AddChild(spinBoxZ);

        return new VisualControlInfo(new Vector3IControl(vector3IHBox, spinBoxX, spinBoxY, spinBoxZ));
    }
}

public class Vector3IControl(HBoxContainer vector3IHBox, SpinBox spinBoxX, SpinBox spinBoxY, SpinBox spinBoxZ) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is Vector3I vector3I)
        {
            spinBoxX.Value = vector3I.X;
            spinBoxY.Value = vector3I.Y;
            spinBoxZ.Value = vector3I.Z;
        }
    }

    public Control Control => vector3IHBox;

    public void SetEditable(bool editable)
    {
        spinBoxX.Editable = editable;
        spinBoxY.Editable = editable;
        spinBoxZ.Editable = editable;
    }
}
