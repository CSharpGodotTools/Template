using Godot;
using System;

namespace __TEMPLATE__.Ui;

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

    public OptionsVisualSettingsComponent(OptionsValueStoreComponent valueStore)
    {
        _valueStore = valueStore;
    }

    public void ApplyStartupSettings()
    {
        ApplyLanguage();
        ApplyAntialiasing();
    }

    public void SetLanguage(int language)
    {
        int clamped = CoerceLanguage(language);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.Language, clamped);
        ApplyLanguage();
    }

    public void SetQualityPreset(int qualityPreset)
    {
        int clamped = Math.Clamp(qualityPreset, MinQualityPreset, MaxQualityPreset);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.QualityPreset, clamped);
    }

    public void SetAntialiasing(int antialiasing)
    {
        _valueStore.SetInt(FrameworkOptionsSaveKeys.Antialiasing, Math.Clamp(antialiasing, 0, 3));
        ApplyAntialiasing();
    }

    private void ApplyLanguage()
    {
        int language = GetLanguage();
        int localeIndex = Math.Clamp(language, 0, _supportedLocales.Length - 1);
        TranslationServer.SetLocale(_supportedLocales[localeIndex]);
    }

    private void ApplyAntialiasing()
    {
        int antialiasing = GetAntialiasing();
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_2d", antialiasing);
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_3d", antialiasing);
    }

    private int GetLanguage()
    {
        int language = CoerceLanguage(_valueStore.GetInt(FrameworkOptionsSaveKeys.Language, DefaultLanguage));
        _valueStore.SetInt(FrameworkOptionsSaveKeys.Language, language);
        return language;
    }

    private int GetAntialiasing()
    {
        int antialiasing = Math.Clamp(_valueStore.GetInt(FrameworkOptionsSaveKeys.Antialiasing, DefaultAntialiasing), 0, 3);
        _valueStore.SetInt(FrameworkOptionsSaveKeys.Antialiasing, antialiasing);
        return antialiasing;
    }

    private static int CoerceLanguage(int raw)
    {
        return Math.Clamp(raw, MinLanguage, MaxLanguage);
    }
}
