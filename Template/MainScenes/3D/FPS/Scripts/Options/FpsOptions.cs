using __TEMPLATE__.Ui;
using System;

namespace __TEMPLATE__.FPS;

public static class FpsOptions
{
    private const int MinDifficultyIndex = (int)Difficulty.Easy;
    private const int MaxDifficultyIndex = (int)Difficulty.Hard;
    private const int DefaultDifficultyIndex = (int)Difficulty.Normal;
    private const float MinSensitivity = 0.1f;
    private const float MaxSensitivity = 2.0f;
    private const float DefaultSensitivity = 0.85f;

    public const string DifficultySaveKey = "Difficulty";
    public const string MouseSensitivitySaveKey = "MouseSensitivity";

    public static void Register(IOptionsService optionsService)
    {
        optionsService.AddOption(
            OptionDefinitions.Dropdown(
                tab: OptionsTabs.Gameplay,
                label: "DIFFICULTY",
                items: ["EASY", "NORMAL", "HARD"],
                getValue: () => GetDifficultyIndex(optionsService),
                setValue: value => optionsService.Settings.Difficulty = ClampDifficultyIndex(value),
                saveKey: DifficultySaveKey,
                defaultValue: DefaultDifficultyIndex));

        optionsService.AddOption(
            OptionDefinitions.Slider(
                tab: OptionsTabs.Gameplay,
                label: "MOUSE_SENSITIVITY",
                minValue: MinSensitivity,
                maxValue: MaxSensitivity,
                getValue: () => GetMouseSensitivity(optionsService),
                setValue: value => optionsService.Settings.MouseSensitivity = ClampMouseSensitivity(value),
                step: 0.01,
                saveKey: MouseSensitivitySaveKey,
                defaultValue: DefaultSensitivity));
    }

    public static Difficulty GetDifficulty(IOptionsService optionsService)
    {
        return (Difficulty)GetDifficultyIndex(optionsService);
    }

    public static float GetMouseSensitivity(IOptionsService optionsService)
    {
        float storedValue = optionsService.Settings.MouseSensitivity;
        return ClampMouseSensitivity(storedValue);
    }

    /// <summary>
    /// Reads and clamps the stored difficulty index.
    /// </summary>
    /// <param name="optionsService">Options service containing persisted difficulty.</param>
    /// <returns>Validated difficulty index.</returns>
    private static int GetDifficultyIndex(IOptionsService optionsService)
    {
        int storedValue = optionsService.Settings.Difficulty;
        return ClampDifficultyIndex(storedValue);
    }

    /// <summary>
    /// Clamps a difficulty index to the supported enum range.
    /// </summary>
    /// <param name="value">Difficulty index to clamp.</param>
    /// <returns>Clamped difficulty index.</returns>
    private static int ClampDifficultyIndex(int value)
    {
        return Math.Clamp(value, MinDifficultyIndex, MaxDifficultyIndex);
    }

    /// <summary>
    /// Clamps mouse sensitivity to configured minimum and maximum values.
    /// </summary>
    /// <param name="value">Sensitivity value to clamp.</param>
    /// <returns>Clamped sensitivity value.</returns>
    private static float ClampMouseSensitivity(float value)
    {
        return Math.Clamp(value, MinSensitivity, MaxSensitivity);
    }
}

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}
