using Godot;

namespace __TEMPLATE__;

/// <summary>
/// Defines audio playback and volume control operations.
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Plays background music with optional fade behavior.
    /// </summary>
    /// <param name="song">Music stream to play.</param>
    /// <param name="instant">True to switch instantly without fade-out.</param>
    /// <param name="fadeOut">Fade-out duration for current music.</param>
    /// <param name="fadeIn">Fade-in duration for new music.</param>
    void PlayMusic(AudioStream song, bool instant = true, double fadeOut = 1.5, double fadeIn = 0.5);

    /// <summary>
    /// Plays a sound effect at a world position with randomized pitch.
    /// </summary>
    /// <param name="sound">Sound effect stream.</param>
    /// <param name="position">World position for spatial playback.</param>
    /// <param name="minPitch">Minimum randomized pitch scale.</param>
    /// <param name="maxPitch">Maximum randomized pitch scale.</param>
    void PlaySFX(AudioStream sound, Vector2 position, float minPitch = 0.8f, float maxPitch = 1.2f);

    /// <summary>
    /// Fades out currently playing sound effects.
    /// </summary>
    /// <param name="fadeTime">Fade duration in seconds.</param>
    void FadeOutSFX(double fadeTime = 1);

    /// <summary>
    /// Sets background music volume.
    /// </summary>
    /// <param name="volume">Target volume value.</param>
    void SetMusicVolume(float volume);

    /// <summary>
    /// Sets sound effects volume.
    /// </summary>
    /// <param name="volume">Target volume value.</param>
    void SetSFXVolume(float volume);
}
