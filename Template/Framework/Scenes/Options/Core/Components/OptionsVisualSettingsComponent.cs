using Godot;
using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Applies visual option values such as language and anti-aliasing.
/// </summary>
internal sealed class OptionsVisualSettingsComponent
{
    private const int DefaultLanguage = 0;
    private const int MinLanguage = 0;
    private const int MaxLanguage = 2;
    private const int MinQualityPreset = 0;
    private const int MaxQualityPreset = 2;
    private const int DefaultAntialiasing = 3;

    private static readonly string[] _supportedLocales = ["en", "fr", "ja"];

    private readonly OptionsValueStoreComponent _valueStore;

    /// <summary>
    /// Initializes visual settings storage dependency.
    /// </summary>
    /// <param name="valueStore">Persistent options value store.</param>
    public OptionsVisualSettingsComponent(OptionsValueStoreComponent valueStore)
    {
        _valueStore = valueStore;
    }

    /// <summary>
    /// Applies persisted startup visual settings.
    /// </summary>
    public void ApplyStartupSettings()
    {
        ApplyLanguage();
        ApplyAntialiasing();
    }

    /// <summary>
    /// Stores and applies selected language.
    /// </summary>
    /// <param name="language">Requested language index.</param>
    public void SetLanguage(int language)
    {
        int clamped = CoerceLanguage(language);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.Language, clamped);
        ApplyLanguage();
    }

    /// <summary>
    /// Stores selected graphics quality preset.
    /// </summary>
    /// <param name="qualityPreset">Requested quality preset index.</param>
    public void SetQualityPreset(int qualityPreset)
    {
        int clamped = Math.Clamp(qualityPreset, MinQualityPreset, MaxQualityPreset);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.QualityPreset, clamped);
    }

    /// <summary>
    /// Stores and applies anti-aliasing quality.
    /// </summary>
    /// <param name="antialiasing">Requested anti-aliasing mode index.</param>
    public void SetAntialiasing(int antialiasing)
    {
        _valueStore.SetInt(FrameworkOptionsSaveKeys.Antialiasing, Math.Clamp(antialiasing, 0, 3));
        ApplyAntialiasing();
    }

    /// <summary>
    /// Applies persisted language selection to translation server.
    /// </summary>
    private void ApplyLanguage()
    {
        int language = GetLanguage();
        int localeIndex = Math.Clamp(language, 0, _supportedLocales.Length - 1);
        TranslationServer.SetLocale(_supportedLocales[localeIndex]);
    }

    /// <summary>
    /// Applies persisted anti-aliasing values to project settings.
    /// </summary>
    private void ApplyAntialiasing()
    {
        int antialiasing = GetAntialiasing();
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_2d", antialiasing);
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_3d", antialiasing);
    }

    /// <summary>
    /// Reads and normalizes stored language index.
    /// </summary>
    /// <returns>Coerced language index.</returns>
    private int GetLanguage()
    {
        int language = CoerceLanguage(_valueStore.GetInt(FrameworkOptionsSaveKeys.Language, DefaultLanguage));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.Language, language);
        return language;
    }

    /// <summary>
    /// Reads and normalizes stored anti-aliasing value.
    /// </summary>
    /// <returns>Coerced anti-aliasing value.</returns>
    private int GetAntialiasing()
    {
        int antialiasing = Math.Clamp(_valueStore.GetInt(FrameworkOptionsSaveKeys.Antialiasing, DefaultAntialiasing), 0, 3);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.Antialiasing, antialiasing);
        return antialiasing;
    }

    /// <summary>
    /// Coerces raw language value into supported language range.
    /// </summary>
    /// <param name="raw">Raw language value.</param>
    /// <returns>Coerced language index.</returns>
    private static int CoerceLanguage(int raw)
    {
        return Math.Clamp(raw, MinLanguage, MaxLanguage);
    }
}
