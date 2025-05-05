using Godot;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector3(VisualControlContext context)
    {
        HBoxContainer vector3HBox = new();

        Vector3 vector3 = (Vector3)context.InitialValue;

        SpinBox spinBoxX = CreateSpinBox(typeof(float), null);
        SpinBox spinBoxY = CreateSpinBox(typeof(float), null);
        SpinBox spinBoxZ = CreateSpinBox(typeof(float), null);

        spinBoxX.Value = vector3.X;
        spinBoxY.Value = vector3.Y;
        spinBoxZ.Value = vector3.Z;

        spinBoxX.ValueChanged += value =>
        {
            vector3.X = (float)value;
            context.ValueChanged(vector3);
        };

        spinBoxY.ValueChanged += value =>
        {
            vector3.Y = (float)value;
            context.ValueChanged(vector3);
        };

        spinBoxZ.ValueChanged += value =>
        {
            vector3.Z = (float)value;
            context.ValueChanged(vector3);
        };

        vector3HBox.AddChild(new Label { Text = "X" });
        vector3HBox.AddChild(spinBoxX);
        vector3HBox.AddChild(new Label { Text = "Y" });
        vector3HBox.AddChild(spinBoxY);
        vector3HBox.AddChild(new Label { Text = "Z" });
        vector3HBox.AddChild(spinBoxZ);

        return new VisualControlInfo(new Vector3Control(vector3HBox, spinBoxX, spinBoxY, spinBoxZ));
    }
}

public class Vector3Control(HBoxContainer vector3HBox, SpinBox spinBoxX, SpinBox spinBoxY, SpinBox spinBoxZ) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is Vector3 vector3)
        {
            spinBoxX.Value = vector3.X;
            spinBoxY.Value = vector3.Y;
            spinBoxZ.Value = vector3.Z;
        }
    }

    public Control Control => vector3HBox;

    public void SetEditable(bool editable)
    {
        spinBoxX.Editable = editable;
        spinBoxY.Editable = editable;
        spinBoxZ.Editable = editable;
    }
}
