using Godot;

namespace __TEMPLATE__;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualQuaternion(VisualControlContext context)
    {
        HBoxContainer quaternionHBox = new();

        Quaternion quaternion = (Quaternion)context.InitialValue;

        SpinBox spinBoxX = CreateSpinBox(typeof(float));
        SpinBox spinBoxY = CreateSpinBox(typeof(float));
        SpinBox spinBoxZ = CreateSpinBox(typeof(float));
        SpinBox spinBoxW = CreateSpinBox(typeof(float));

        spinBoxX.Value = quaternion.X;
        spinBoxY.Value = quaternion.Y;
        spinBoxZ.Value = quaternion.Z;
        spinBoxW.Value = quaternion.W;

        spinBoxX.ValueChanged += value =>
        {
            quaternion.X = (float)value;
            context.ValueChanged(quaternion);
        };

        spinBoxY.ValueChanged += value =>
        {
            quaternion.Y = (float)value;
            context.ValueChanged(quaternion);
        };

        spinBoxZ.ValueChanged += value =>
        {
            quaternion.Z = (float)value;
            context.ValueChanged(quaternion);
        };

        spinBoxW.ValueChanged += value =>
        {
            quaternion.W = (float)value;
            context.ValueChanged(quaternion);
        };

        quaternionHBox.AddChild(new Label { Text = "X" });
        quaternionHBox.AddChild(spinBoxX);
        quaternionHBox.AddChild(new Label { Text = "Y" });
        quaternionHBox.AddChild(spinBoxY);
        quaternionHBox.AddChild(new Label { Text = "Z" });
        quaternionHBox.AddChild(spinBoxZ);
        quaternionHBox.AddChild(new Label { Text = "W" });
        quaternionHBox.AddChild(spinBoxW);

        return new VisualControlInfo(new QuaternionControl(quaternionHBox, spinBoxX, spinBoxY, spinBoxZ, spinBoxW));
    }
}

public class QuaternionControl(HBoxContainer quaternionHBox, SpinBox spinBoxX, SpinBox spinBoxY, SpinBox spinBoxZ, SpinBox spinBoxW) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is Quaternion quaternion)
        {
            spinBoxX.Value = quaternion.X;
            spinBoxY.Value = quaternion.Y;
            spinBoxZ.Value = quaternion.Z;
            spinBoxW.Value = quaternion.W;
        }
    }

    public Control Control => quaternionHBox;

    public void SetEditable(bool editable)
    {
        spinBoxX.Editable = editable;
        spinBoxY.Editable = editable;
        spinBoxZ.Editable = editable;
        spinBoxW.Editable = editable;
    }
}
