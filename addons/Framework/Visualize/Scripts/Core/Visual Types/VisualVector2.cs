using Godot;

namespace __TEMPLATE__;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualVector2(VisualControlContext context)
    {
        HBoxContainer vector2HBox = new();

        Vector2 vector2 = (Vector2)context.InitialValue;

        SpinBox spinBoxX = CreateSpinBox(typeof(float));
        SpinBox spinBoxY = CreateSpinBox(typeof(float));

        spinBoxX.Value = vector2.X;
        spinBoxY.Value = vector2.Y;

        spinBoxX.ValueChanged += value =>
        {
            vector2.X = (float)value;
            context.ValueChanged(vector2);
        };

        spinBoxY.ValueChanged += value =>
        {
            vector2.Y = (float)value;
            context.ValueChanged(vector2);
        };

        vector2HBox.AddChild(new Label { Text = "X" });
        vector2HBox.AddChild(spinBoxX);
        vector2HBox.AddChild(new Label { Text = "Y" });
        vector2HBox.AddChild(spinBoxY);

        return new VisualControlInfo(new Vector2Control(vector2HBox, spinBoxX, spinBoxY));
    }

    public class Vector2Control(HBoxContainer vector2HBox, SpinBox spinBoxX, SpinBox spinBoxY) : IVisualControl
    {
        public void SetValue(object value)
        {
            if (value is Vector2 vector2)
            {
                spinBoxX.Value = vector2.X;
                spinBoxY.Value = vector2.Y;
            }
        }

        public Control Control => vector2HBox;

        public void SetEditable(bool editable)
        {
            spinBoxX.Editable = editable;
            spinBoxY.Editable = editable;
        }
    }
}
