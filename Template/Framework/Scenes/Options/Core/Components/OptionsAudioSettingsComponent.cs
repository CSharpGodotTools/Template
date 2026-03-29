using System;

namespace __TEMPLATE__.Ui;

internal sealed class OptionsAudioSettingsComponent
{
    private readonly AutoloadsFramework _autoloads;
    private readonly OptionsValueStoreComponent _valueStore;

    public OptionsAudioSettingsComponent(AutoloadsFramework autoloads, OptionsValueStoreComponent valueStore)
    {
        _autoloads = autoloads;
        _valueStore = valueStore;
    }

    public void SetMusicVolume(float volume)
    {
        float clamped = Math.Clamp(volume, 0f, 100f);

        if (_autoloads.AudioManager is not null)
            _autoloads.AudioManager.ApplyMusicVolumeFromSettings(clamped);

        _valueStore.SetFloat(OptionsSaveKeys.MusicVolume, clamped);
    }

    public void SetSfxVolume(float volume)
    {
        float clamped = Math.Clamp(volume, 0f, 100f);

        if (_autoloads.AudioManager is not null)
            _autoloads.AudioManager.ApplySfxVolumeFromSettings(clamped);

        _valueStore.SetFloat(OptionsSaveKeys.SfxVolume, clamped);
    }
}
