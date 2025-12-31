using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__;

public class AudioManager : IDisposable
{
    private const float MinDefaultRandomPitch = 0.8f;   // Default minimum pitch value for SFX.
    private const float MaxDefaultRandomPitch = 1.2f;   // Default maximum pitch value for SFX.
    private const float RandomPitchThreshold  = 0.1f;   // Minimum difference in pitch between repeated sounds.
    private const int   MutedVolume           = -80;    // dB value representing mute.
    private const int   MutedVolumeNormalized = -40;    // Normalized muted volume for volume mapping.

    private AudioStreamPlayer   _musicPlayer;
    private ResourceOptions     _options;
    private BaseAutoloads       _autoloads;
    private float               _lastPitch;

    private GodotNodePool<AudioStreamPlayer2D> _sfxPool;

    /// <summary>
    /// Initializes the AudioManager by attaching a music player to the given autoload node.
    /// </summary>
    public AudioManager(BaseAutoloads autoloads)
    {
        SetupFields(autoloads);
        SetupSfxPool();
        SetupMusicPlayer();
    }

    /// <summary>
    /// Frees all managed players and clears references for cleanup.
    /// </summary>
    public void Dispose()
    {
        _musicPlayer.QueueFree();
        _sfxPool.Clear();
    }

    /// <summary>
    /// Plays a music track, instantly or with optional fade between tracks. Music volume is in config scale (0-100).
    /// </summary>
    public void PlayMusic(AudioStream song, bool instant = true, double fadeOut = 1.5, double fadeIn = 0.5)
    {
        if (!instant && _musicPlayer.Playing)
        {
            // Slowly transition to the new song
            PlayAudioCrossfade(_musicPlayer, song, _options.MusicVolume, fadeOut, fadeIn);
        }
        else
        {
            // Instantly switch to the new song
            PlayAudio(_musicPlayer, song, _options.MusicVolume);
        }
    }

    /// <summary>
    /// Plays a sound effect at the specified global position with randomized pitch to reduce repetition. Volume is normalized (0-100).
    /// </summary>
    public void PlaySFX(AudioStream sound, Vector2 position, float minPitch = MinDefaultRandomPitch, float maxPitch = MaxDefaultRandomPitch)
    {
        AudioStreamPlayer2D sfxPlayer = _sfxPool.Get();

        sfxPlayer.GlobalPosition = position;
        sfxPlayer.Stream = sound;
        sfxPlayer.VolumeDb = NormalizeConfigVolume(_options.SFXVolume);
        sfxPlayer.PitchScale = GetRandomPitch(minPitch, maxPitch);
        sfxPlayer.Finished += OnFinished;
        sfxPlayer.Play();

        void OnFinished()
        {
            sfxPlayer.Finished -= OnFinished;
            _sfxPool.Release(sfxPlayer);
        }
    }

    /// <summary>
    /// Fades out all currently playing sound effects over the specified duration in seconds.
    /// </summary>
    public void FadeOutSFX(double fadeTime = 1)
    {
        foreach (AudioStreamPlayer2D sfxPlayer in _sfxPool.ActiveNodes)
        {
            new NodeTween(sfxPlayer).Animate(AudioStreamPlayer.PropertyName.VolumeDb, MutedVolume, fadeTime);
        }
    }

    /// <summary>
    /// Sets the music volume, affecting current playback. Volume is in config scale (0-100).
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        _musicPlayer.VolumeDb = NormalizeConfigVolume(volume);
        _options.MusicVolume = volume;
    }

    /// <summary>
    /// Sets the SFX volume for all active sound effect players. Volume is in config scale (0-100).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        _options.SFXVolume = volume;

        float mappedVolume = NormalizeConfigVolume(volume);

        foreach (AudioStreamPlayer2D sfxPlayer in _sfxPool.ActiveNodes)
        {
            sfxPlayer.VolumeDb = mappedVolume;
        }
    }

    private void SetupFields(BaseAutoloads autoloads)
    {
        _autoloads = autoloads;
        _options = Game.Options.GetOptions();
    }

    private void SetupSfxPool()
    {
        _sfxPool = new GodotNodePool<AudioStreamPlayer2D>(_autoloads,
            () => new AudioStreamPlayer2D());
    }

    private void SetupMusicPlayer()
    {
        _musicPlayer = new AudioStreamPlayer();
        _autoloads.AddChild(_musicPlayer);
    }

    /// <summary>
    /// Generates a random pitch between min and max, avoiding values too similar to the previous sound.
    /// </summary>
    private float GetRandomPitch(float min, float max)
    {
        RandomNumberGenerator rng = new();
        rng.Randomize();

        float pitch = rng.RandfRange(min, max);

        while (Mathf.Abs(pitch - _lastPitch) < RandomPitchThreshold)
        {
            rng.Randomize();
            pitch = rng.RandfRange(min, max);
        }

        _lastPitch = pitch;
        return pitch;
    }

    /// <summary>
    /// Instantly plays the given audio stream with the specified player and volume.
    /// </summary>
    private static void PlayAudio(AudioStreamPlayer player, AudioStream song, float volume)
    {
        player.Stream = song;
        player.VolumeDb = NormalizeConfigVolume(volume);
        player.Play();
    }

    /// <summary>
    /// Smoothly crossfades between songs by fading out the current and fading in the new one. Volume is in config scale (0-100).
    /// </summary>
    private static void PlayAudioCrossfade(AudioStreamPlayer player, AudioStream song, float volume, double fadeOut, double fadeIn)
    {
        new NodeTweenProp(player, AudioStreamPlayer.PropertyName.VolumeDb)
            .Animate(MutedVolume, fadeOut).EaseIn()
            .Callback(() => PlayAudio(player, song, volume))
            .Animate(NormalizeConfigVolume(volume), fadeIn).EaseIn();
    }

    /// <summary>
    /// Maps a config volume value (0-100) to an AudioStreamPlayer VolumeDb value, returning mute if zero.
    /// </summary>
    private static float NormalizeConfigVolume(float volume)
    {
        return volume == 0 ? MutedVolume : volume.Remap(0, 100, MutedVolumeNormalized, 0);
    }
}
