using Framework;
using Framework.UI;

namespace __TEMPLATE__.FPS;

public sealed class MouseSensitivitySlider : SliderOptionDefinition
{
    public override OptionsTab Tab => OptionsTab.Gameplay;
    public override string Label => "MOUSE_SENSITIVITY";
    public override double MinValue => 0.1;
    public override double MaxValue => 2.0;
    public override double Step => 0.01;
    public override float DefaultValue => 0.85f;
    public override bool SaveInCustomValues => false;

    public override float GetValue()
    {
        return GameFramework.Settings.MouseSensitivity;
    }

    public override void SetValue(float value)
    {
        GameFramework.Settings.MouseSensitivity = value;
    }
}
