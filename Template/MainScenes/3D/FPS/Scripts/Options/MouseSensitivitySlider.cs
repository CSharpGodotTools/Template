using __TEMPLATE__;
using __TEMPLATE__.Ui;

namespace __TEMPLATE__.FPS;

public sealed class MouseSensitivitySlider : SliderOptionDefinition
{
    private readonly IOptionsService _optionsService;

    public MouseSensitivitySlider(IOptionsService optionsService)
    {
        _optionsService = optionsService;
    }

    public override OptionsTab Tab => OptionsTab.Gameplay;
    public override string Label => "MOUSE_SENSITIVITY";
    public override double MinValue => 0.1;
    public override double MaxValue => 2.0;
    public override double Step => 0.01;
    public override float DefaultValue => 0.85f;

    public override float GetValue()
    {
        return _optionsService.Settings.MouseSensitivity;
    }

    public override void SetValue(float value)
    {
        _optionsService.SetMouseSensitivity(value);
    }
}
