using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Applies audio option values and persists them to the options store.
/// </summary>
internal sealed class OptionsAudioSettingsComponent
{
    private readonly AutoloadsFramework _autoloads;
    private readonly OptionsValueStoreComponent _valueStore;

    /// <summary>
    /// Initializes audio settings coordination dependencies.
    /// </summary>
    /// <param name="autoloads">Autoload access for runtime audio managers.</param>
    /// <param name="valueStore">Persistent options value storage.</param>
    public OptionsAudioSettingsComponent(AutoloadsFramework autoloads, OptionsValueStoreComponent valueStore)
    {
        _autoloads = autoloads;
        _valueStore = valueStore;
    }

    /// <summary>
    /// Applies and stores background music volume.
    /// </summary>
    /// <param name="volume">Requested volume value in percent.</param>
    public void SetMusicVolume(float volume)
    {
        float clamped = Math.Clamp(volume, 0f, 100f);


        // Apply to runtime manager before persisting so active audio updates now.
        _autoloads.AudioManager?.ApplyMusicVolumeFromSettings(clamped);

        _valueStore.SetFloat(FrameworkOptionsSaveKeys.MusicVolume, clamped);
    }

    /// <summary>
    /// Applies and stores sound effect volume.
    /// </summary>
    /// <param name="volume">Requested volume value in percent.</param>
    public void SetSfxVolume(float volume)
    {
        float clamped = Math.Clamp(volume, 0f, 100f);


        // Apply to runtime manager before persisting so active audio updates now.
        _autoloads.AudioManager?.ApplySfxVolumeFromSettings(clamped);

        _valueStore.SetFloat(FrameworkOptionsSaveKeys.SfxVolume, clamped);
    }
}
