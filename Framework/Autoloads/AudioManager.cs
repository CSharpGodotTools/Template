using Godot;
using GodotUtils.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GodotUtils;

public class AudioManager : IDisposable
{
    private const float MinDefaultRandomPitch        = 0.8f;
    private const float MaxDefaultRandomPitch        = 1.2f;
    private const float RandomPitchThreshold  = 0.1f;
    private const int   MutedVolume           = -80;
    private const int   MutedVolumeNormalized = -40;

    private static AudioManager _instance;
    private AudioStreamPlayer   _musicPlayer;
    private ResourceOptions     _options;
    private Autoloads           _autoloads;
    private float               _lastPitch;

    private List<AudioStreamPlayer2D> _activeSfxPlayers = [];

    public AudioManager(Autoloads autoloads)
    {
        if (_instance != null)
            throw new InvalidOperationException($"{nameof(AudioManager)} was initialized already");

        _instance = this;
        _autoloads = autoloads;
        _options = OptionsManager.GetOptions();

        _musicPlayer = new AudioStreamPlayer();
        autoloads.AddChild(_musicPlayer);
    }

    public void Dispose()
    {
        _musicPlayer.QueueFree();
        _activeSfxPlayers.Clear();

        _instance = null;
    }

    public static void PlayMusic(AudioStream song, bool instant = true, double fadeOut = 1.5, double fadeIn = 0.5)
    {
        if (!instant && _instance._musicPlayer.Playing)
        {
            // Slowly transition to the new song
            PlayAudioCrossfade(_instance._musicPlayer, song, _instance._options.MusicVolume, fadeOut, fadeIn);
        }
        else
        {
            // Instantly switch to the new song
            PlayAudio(_instance._musicPlayer, song, _instance._options.MusicVolume);
        }
    }

    /// <summary>
    /// Plays a <paramref name="sound"/> at <paramref name="position"/>.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="sound"></param>
    public static void PlaySFX(AudioStream sound, Vector2 position, float minPitch = MinDefaultRandomPitch, float maxPitch = MaxDefaultRandomPitch)
    {
        AudioStreamPlayer2D sfxPlayer = new()
        {
            Stream = sound,
            VolumeDb = NormalizeConfigVolume(_instance._options.SFXVolume),
            PitchScale = GetRandomPitch(minPitch, maxPitch)
        };

        sfxPlayer.Finished += () =>
        {
            sfxPlayer.QueueFree();
            _instance._activeSfxPlayers.Remove(sfxPlayer);
        };
        
        _instance._autoloads.AddChild(sfxPlayer);
        _instance._activeSfxPlayers.Add(sfxPlayer);

        sfxPlayer.GlobalPosition = position;
        sfxPlayer.Play();
    }

    public static void FadeOutSFX(double fadeTime = 1)
    {
        foreach (AudioStreamPlayer2D sfxPlayer in _instance._activeSfxPlayers)
        {
            new GodotTween(sfxPlayer).Animate(AudioStreamPlayer.PropertyName.VolumeDb, MutedVolume, fadeTime);
        }
    }

    public static void SetMusicVolume(float volume)
    {
        _instance._musicPlayer.VolumeDb = NormalizeConfigVolume(volume);
        _instance._options.MusicVolume = volume;
    }

    public static void SetSFXVolume(float volume)
    {
        _instance._options.SFXVolume = volume;

        float mappedVolume = NormalizeConfigVolume(volume);

        foreach (AudioStreamPlayer2D sfxPlayer in _instance._activeSfxPlayers)
        {
            sfxPlayer.VolumeDb = mappedVolume;
        }
    }

    private static void PlayAudio(AudioStreamPlayer player, AudioStream song, float volume)
    {
        player.Stream = song;
        player.VolumeDb = NormalizeConfigVolume(volume);
        player.Play();
    }

    private static void PlayAudioCrossfade(AudioStreamPlayer player, AudioStream song, float volume, double fadeOut, double fadeIn)
    {
        new GodotTween(player)
            .SetAnimatingProp(AudioStreamPlayer.PropertyName.VolumeDb)
            .AnimateProp(MutedVolume, fadeOut).EaseIn()
            .Callback(() => PlayAudio(player, song, volume))
            .AnimateProp(NormalizeConfigVolume(volume), fadeIn).EaseIn();
    }

    private static float NormalizeConfigVolume(float volume)
    {
        return volume == 0 ? MutedVolume : volume.Remap(0, 100, MutedVolumeNormalized, 0);
    }

    private static float GetRandomPitch(float min, float max)
    {
        RandomNumberGenerator rng = new();
        rng.Randomize();

        float pitch = rng.RandfRange(min, max);

        while (Mathf.Abs(pitch - _instance._lastPitch) < RandomPitchThreshold)
        {
            rng.Randomize();
            pitch = rng.RandfRange(min, max);
        }

        _instance._lastPitch = pitch;
        return pitch;
    }
}
