using __TEMPLATE__.Ui;
using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__;

/// <summary>
/// Manages music and sound-effect playback, pooling, and volume normalization from user settings.
/// </summary>
public class AudioManager : IDisposable, IAudioService
{
    // Config
    private const float MinDefaultRandomPitch = 0.8f; // Default minimum pitch value for SFX.
    private const float MaxDefaultRandomPitch = 1.2f; // Default maximum pitch value for SFX.
    private const float RandomPitchThreshold = 0.1f; // Minimum difference in pitch between repeated sounds.
    private const int MutedVolume = -80; // dB value representing mute.
    private const int MutedVolumeNormalized = -40; // Normalized muted volume for volume mapping.
    private const float DefaultMusicVolume = 100f;
    private const float DefaultSfxVolume = 100f;

    // Variables
    private readonly RandomNumberGenerator _randomNumberGenerator = new();
    private NodePool<AudioStreamPlayer2D> _sfxPool = null!;
    private AudioStreamPlayer _musicPlayer = null!;
    private OptionsManager _optionsManager = null!;
    private AutoloadsFramework _autoloads = null!;
    private float _lastPitch;

    /// <summary>
    /// Initializes the AudioManager by attaching a music player to the given autoload node.
    /// </summary>
    /// <param name="autoloads">Framework autoload root used to host audio nodes.</param>
    /// <param name="optionsManager">Options manager used to read and persist volume settings.</param>
    public AudioManager(AutoloadsFramework autoloads, OptionsManager optionsManager)
    {
        SetupFields(autoloads, optionsManager);
        _randomNumberGenerator.Randomize();
        SetupSfxPool();
        SetupMusicPlayer();
    }

    // API
    /// <summary>
    /// Plays a music track, instantly or with optional fade between tracks. Music volume is in config scale (0-100).
    /// </summary>
    /// <param name="song">Music stream to play.</param>
    /// <param name="instant">Whether to switch immediately instead of crossfading.</param>
    /// <param name="fadeOut">Fade-out duration in seconds when crossfading.</param>
    /// <param name="fadeIn">Fade-in duration in seconds when crossfading.</param>
    public void PlayMusic(AudioStream song, bool instant = true, double fadeOut = 1.5, double fadeIn = 0.5)
    {
        float musicVolume = _optionsManager.Settings.GetFloat(FrameworkOptionsSaveKeys.MusicVolume, DefaultMusicVolume);

        // Crossfade only when instant switch is disabled and music is currently playing.
        if (!instant && _musicPlayer.Playing)
        {
            // Slowly transition to the new song
            PlayAudioCrossfade(_musicPlayer, song, musicVolume, fadeOut, fadeIn);
        }
        else
        {
            // Instantly switch to the new song
            PlayAudio(_musicPlayer, song, musicVolume);
        }
    }

    /// <summary>
    /// Plays a sound effect at the specified global position with randomized pitch to reduce repetition. Volume is normalized (0-100).
    /// </summary>
    /// <param name="sound">Sound-effect stream to play.</param>
    /// <param name="position">Global world position where the sound should originate.</param>
    /// <param name="minPitch">Minimum randomized pitch multiplier.</param>
    /// <param name="maxPitch">Maximum randomized pitch multiplier.</param>
    public void PlaySFX(AudioStream sound, Vector2 position, float minPitch = MinDefaultRandomPitch, float maxPitch = MaxDefaultRandomPitch)
    {
        float sfxVolume = _optionsManager.Settings.GetFloat(FrameworkOptionsSaveKeys.SfxVolume, DefaultSfxVolume);
        AudioStreamPlayer2D sfxPlayer = _sfxPool.Acquire();

        sfxPlayer.GlobalPosition = position;
        sfxPlayer.Stream = sound;
        sfxPlayer.VolumeDb = NormalizeConfigVolume(sfxVolume);
        sfxPlayer.PitchScale = GetRandomPitch(minPitch, maxPitch);
        sfxPlayer.Finished += OnFinished;
        sfxPlayer.Play();

        void OnFinished()
        {
            // Return pooled players once playback completes so future SFX can reuse them.
            sfxPlayer.Finished -= OnFinished;
            _sfxPool.Release(sfxPlayer);
        }
    }

    /// <summary>
    /// Fades out all currently playing sound effects over the specified duration in seconds.
    /// </summary>
    /// <param name="fadeTime">Fade duration in seconds.</param>
    public void FadeOutSFX(double fadeTime = 1)
    {
        foreach (AudioStreamPlayer2D sfxPlayer in _sfxPool.ActiveNodes)
            Tweens.Animate(sfxPlayer).Property(AudioStreamPlayer.PropertyName.VolumeDb, MutedVolume, fadeTime);
    }

    /// <summary>
    /// Sets the music volume, affecting current playback. Volume is in config scale (0-100).
    /// </summary>
    /// <param name="volume">Music volume in config scale (0-100).</param>
    public void SetMusicVolume(float volume)
    {
        _optionsManager.Settings.SetFloat(FrameworkOptionsSaveKeys.MusicVolume, volume);
    }

    /// <summary>
    /// Sets the SFX volume for all active sound effect players. Volume is in config scale (0-100).
    /// </summary>
    /// <param name="volume">SFX volume in config scale (0-100).</param>
    public void SetSFXVolume(float volume)
    {
        _optionsManager.Settings.SetFloat(FrameworkOptionsSaveKeys.SfxVolume, volume);
    }

    internal void ApplyMusicVolumeFromSettings(float volume)
    {
        _musicPlayer.VolumeDb = NormalizeConfigVolume(volume);
    }

    internal void ApplySfxVolumeFromSettings(float volume)
    {
        float mappedVolume = NormalizeConfigVolume(volume);

        foreach (AudioStreamPlayer2D sfxPlayer in _sfxPool.ActiveNodes)
            sfxPlayer.VolumeDb = mappedVolume;
    }

    // Private Methods
    /// <summary>
    /// Stores constructor dependencies.
    /// </summary>
    /// <param name="autoloads">Framework autoload root.</param>
    /// <param name="optionsManager">Options manager used for persisted volume settings.</param>
    private void SetupFields(AutoloadsFramework autoloads, OptionsManager optionsManager)
    {
        _autoloads = autoloads;
        _optionsManager = optionsManager;
    }

    /// <summary>
    /// Creates the pooled sound-effect player collection.
    /// </summary>
    private void SetupSfxPool()
    {
        _sfxPool = new NodePool<AudioStreamPlayer2D>(_autoloads, () => new AudioStreamPlayer2D());
    }

    /// <summary>
    /// Creates and attaches the dedicated music player node.
    /// </summary>
    private void SetupMusicPlayer()
    {
        _musicPlayer = new AudioStreamPlayer();
        _autoloads.AddChild(_musicPlayer);
    }

    /// <summary>
    /// Generates a random pitch between min and max, avoiding values too similar to the previous sound.
    /// </summary>
    /// <param name="min">Minimum randomized pitch multiplier.</param>
    /// <param name="max">Maximum randomized pitch multiplier.</param>
    /// <returns>Randomized pitch value constrained by the provided range.</returns>
    private float GetRandomPitch(float min, float max)
    {
        float pitch = _randomNumberGenerator.RandfRange(min, max);
        int attempts = 0;
        const int MaxAttempts = 8;

        // Avoid near-identical consecutive pitch values to reduce repetitive SFX artifacts.
        while (Mathf.Abs(pitch - _lastPitch) < RandomPitchThreshold && attempts < MaxAttempts)
        {
            pitch = _randomNumberGenerator.RandfRange(min, max);
            attempts++;
        }

        _lastPitch = pitch;
        return pitch;
    }

    /// <summary>
    /// Instantly plays the given audio stream with the specified player and volume.
    /// </summary>
    /// <param name="player">Audio player used for playback.</param>
    /// <param name="song">Audio stream to play.</param>
    /// <param name="volume">Volume in config scale (0-100).</param>
    private static void PlayAudio(AudioStreamPlayer player, AudioStream song, float volume)
    {
        player.Stream = song;
        player.VolumeDb = NormalizeConfigVolume(volume);
        player.Play();
    }

    /// <summary>
    /// Smoothly crossfades between songs by fading out the current and fading in the new one. Volume is in config scale (0-100).
    /// </summary>
    /// <param name="player">Audio player used for playback.</param>
    /// <param name="song">Audio stream to play.</param>
    /// <param name="volume">Volume in config scale (0-100).</param>
    /// <param name="fadeOut">Fade-out duration in seconds.</param>
    /// <param name="fadeIn">Fade-in duration in seconds.</param>
    private static void PlayAudioCrossfade(AudioStreamPlayer player, AudioStream song, float volume, double fadeOut, double fadeIn)
    {
        Tweens.Animate(player, AudioStreamPlayer.PropertyName.VolumeDb)
            .PropertyTo(MutedVolume, fadeOut).EaseIn()
            .Then(() => PlayAudio(player, song, volume))
            .PropertyTo(NormalizeConfigVolume(volume), fadeIn).EaseIn();
    }

    /// <summary>
    /// Maps a config volume value (0-100) to an AudioStreamPlayer VolumeDb value, returning mute if zero.
    /// </summary>
    /// <param name="volume">Volume in config scale (0-100).</param>
    /// <returns>Mapped decibel value for audio players.</returns>
    private static float NormalizeConfigVolume(float volume)
    {
        return volume == 0 ? MutedVolume : volume.Remap(0, 100, MutedVolumeNormalized, 0);
    }

    // Dispose
    /// <summary>
    /// Frees all managed players and clears references for cleanup.
    /// </summary>
    public void Dispose()
    {
        _musicPlayer.QueueFree();
        _sfxPool.QueueFreeAll();
    }
}
