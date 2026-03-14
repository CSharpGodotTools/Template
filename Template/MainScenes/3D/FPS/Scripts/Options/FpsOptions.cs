namespace __TEMPLATE__.FPS;

public static class FpsOptions
{
    public static void Register()
    {
        Game.Options.AddDropdown(new DifficultyDropdown());
        Game.Options.AddSlider(new MouseSensitivitySlider());
    }
}
