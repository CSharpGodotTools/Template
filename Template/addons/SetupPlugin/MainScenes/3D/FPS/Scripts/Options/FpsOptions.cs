using Framework;
using Framework.UI;

namespace __TEMPLATE__.FPS;

public static class FpsOptions
{
    public static void Register()
    {
        GameFramework.Options.AddDropdown(
            tab: OptionsTab.Gameplay,
            label: "DIFFICULTY",
            items: ["EASY", "NORMAL", "HARD"],
            getValue: () => (int)GameFramework.Settings.Difficulty,
            setValue: value => GameFramework.Settings.Difficulty = (Difficulty)value,
            defaultValue: (int)Difficulty.Normal);

        GameFramework.Options.AddSlider(
            tab: OptionsTab.Gameplay,
            label: "MOUSE_SENSITIVITY",
            getValue: () => GameFramework.Settings.MouseSensitivity,
            setValue: value => GameFramework.Settings.MouseSensitivity = value,
            minValue: 0.1f,
            maxValue: 2.0f,
            step: 0.01f,
            defaultValue: 0.85f);
    }
}
