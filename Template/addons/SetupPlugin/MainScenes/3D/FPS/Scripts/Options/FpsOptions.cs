using Framework;

namespace __TEMPLATE__.FPS;

public static class FpsOptions
{
    public static void Register()
    {
        GameFramework.Options.AddDropdown(new DifficultyDropdown());
        GameFramework.Options.AddSlider(new MouseSensitivitySlider());
    }
}
