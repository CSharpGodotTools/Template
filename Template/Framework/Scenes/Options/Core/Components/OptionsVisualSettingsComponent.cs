using Godot;
using System;

namespace __TEMPLATE__.Ui;

internal sealed class OptionsVisualSettingsComponent
{
    private const int DefaultLanguage = (int)Language.English;
    private const int DefaultAntialiasing = 3;

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

    public void SetLanguage(Language language)
    {
        Language clamped = CoerceLanguage((int)language);
        _valueStore.SetInt(OptionsSaveKeys.Language, (int)clamped);
        ApplyLanguage();
    }

    public void SetQualityPreset(QualityPreset qualityPreset)
    {
        int clamped = Math.Clamp((int)qualityPreset, (int)QualityPreset.Low, (int)QualityPreset.High);
        _valueStore.SetInt(OptionsSaveKeys.QualityPreset, clamped);
    }

    public void SetAntialiasing(int antialiasing)
    {
        _valueStore.SetInt(OptionsSaveKeys.Antialiasing, Math.Clamp(antialiasing, 0, 3));
        ApplyAntialiasing();
    }

    private void ApplyLanguage()
    {
        Language language = GetLanguage();
        TranslationServer.SetLocale(language.ToString()[..2].ToLower());
    }

    private void ApplyAntialiasing()
    {
        int antialiasing = GetAntialiasing();
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_2d", antialiasing);
        ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_3d", antialiasing);
    }

    private Language GetLanguage()
    {
        Language language = CoerceLanguage(_valueStore.GetInt(OptionsSaveKeys.Language, DefaultLanguage));
        _valueStore.SetInt(OptionsSaveKeys.Language, (int)language);
        return language;
    }

    private int GetAntialiasing()
    {
        int antialiasing = Math.Clamp(_valueStore.GetInt(OptionsSaveKeys.Antialiasing, DefaultAntialiasing), 0, 3);
        _valueStore.SetInt(OptionsSaveKeys.Antialiasing, antialiasing);
        return antialiasing;
    }

    private static Language CoerceLanguage(int raw)
    {
        int clamped = Math.Clamp(raw, (int)Language.English, (int)Language.Japanese);
        return (Language)clamped;
    }
}
