using Godot;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector2I(VisualControlContext context)
    {
        HBoxContainer vector2IHBox = new();

        Vector2I vector2I = (Vector2I)context.InitialValue;

        SpinBox spinBoxX = CreateSpinBox(typeof(int), null);
        SpinBox spinBoxY = CreateSpinBox(typeof(int), null);

        spinBoxX.Value = vector2I.X;
        spinBoxY.Value = vector2I.Y;

        spinBoxX.ValueChanged += value =>
        {
            vector2I.X = (int)value;
            context.ValueChanged(vector2I);
        };

        spinBoxY.ValueChanged += value =>
        {
            vector2I.Y = (int)value;
            context.ValueChanged(vector2I);
        };

        vector2IHBox.AddChild(new Label { Text = "X" });
        vector2IHBox.AddChild(spinBoxX);
        vector2IHBox.AddChild(new Label { Text = "Y" });
        vector2IHBox.AddChild(spinBoxY);

        return new VisualControlInfo(new Vector2IControl(vector2IHBox, spinBoxX, spinBoxY));
    }

    public class Vector2IControl(HBoxContainer vector2IHBox, SpinBox spinBoxX, SpinBox spinBoxY) : IVisualControl
    {
        public void SetValue(object value)
        {
            if (value is Vector2I vector2I)
            {
                spinBoxX.Value = vector2I.X;
                spinBoxY.Value = vector2I.Y;
            }
        }

        public Control Control => vector2IHBox;

        public void SetEditable(bool editable)
        {
            spinBoxX.Editable = editable;
            spinBoxY.Editable = editable;
        }
    }
}
