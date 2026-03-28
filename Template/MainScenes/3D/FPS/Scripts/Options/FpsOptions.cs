namespace __TEMPLATE__.FPS;

public static class FpsOptions
{
    public static void Register()
    {
        Game.OptionsManager.AddDropdown(new DifficultyDropdown());
        Game.OptionsManager.AddSlider(new MouseSensitivitySlider());
    }
}
