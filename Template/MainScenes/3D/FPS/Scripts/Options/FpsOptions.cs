using __TEMPLATE__;

namespace __TEMPLATE__.FPS;

public static class FpsOptions
{
    public static void Register(IOptionsService optionsService)
    {
        optionsService.AddDropdown(new DifficultyDropdown(optionsService));
        optionsService.AddSlider(new MouseSensitivitySlider(optionsService));
    }
}
