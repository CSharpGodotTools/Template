using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;

namespace __TEMPLATE__;

public class AudioManager : IDisposable
{
    private const float MinDefaultRandomPitch = 0.8f;
    private const float MaxDefaultRandomPitch = 1.2f;
    private const float RandomPitchThreshold  = 0.1f;
    private const int   MutedVolume           = -80;
    private const int   MutedVolumeNormalized = -40;

    private AudioStreamPlayer   _musicPlayer;
    private ResourceOptions     _options;
    private Autoloads           _autoloads;
    private float               _lastPitch;

    private List<AudioStreamPlayer2D> _activeSfxPlayers = [];

    public AudioManager(Autoloads autoloads)
    {
        _autoloads = autoloads;
        _options = Game.Options.GetOptions();

        _musicPlayer = new AudioStreamPlayer();
        autoloads.AddChild(_musicPlayer);
    }

    public void Dispose()
    {
        _musicPlayer.QueueFree();

        foreach (AudioStreamPlayer2D sfxPlayer in _activeSfxPlayers)
            sfxPlayer?.QueueFree();

        _activeSfxPlayers.Clear();
    }

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
    /// Plays a <paramref name="sound"/> at <paramref name="position"/>.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="sound"></param>
    public void PlaySFX(AudioStream sound, Vector2 position, float minPitch = MinDefaultRandomPitch, float maxPitch = MaxDefaultRandomPitch)
    {
        AudioStreamPlayer2D sfxPlayer = new()
        {
            Stream = sound,
            VolumeDb = NormalizeConfigVolume(_options.SFXVolume),
            PitchScale = GetRandomPitch(minPitch, maxPitch)
        };

        sfxPlayer.Finished += DestroyPlayer;
        sfxPlayer.TreeExited += OnPlayerExitedTree;
        
        _autoloads.AddChild(sfxPlayer);
        _activeSfxPlayers.Add(sfxPlayer);

        sfxPlayer.GlobalPosition = position;
        sfxPlayer.Play();

        void OnPlayerExitedTree()
        {
            sfxPlayer.Finished -= DestroyPlayer;
            sfxPlayer.TreeExited -= OnPlayerExitedTree;
        }

        void DestroyPlayer()
        {
            sfxPlayer.QueueFree();
            _activeSfxPlayers.Remove(sfxPlayer);
        }
    }

    public void FadeOutSFX(double fadeTime = 1)
    {
        foreach (AudioStreamPlayer2D sfxPlayer in _activeSfxPlayers)
        {
            new GodotTween(sfxPlayer).Animate(AudioStreamPlayer.PropertyName.VolumeDb, MutedVolume, fadeTime);
        }
    }

    public void SetMusicVolume(float volume)
    {
        _musicPlayer.VolumeDb = NormalizeConfigVolume(volume);
        _options.MusicVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        _options.SFXVolume = volume;

        float mappedVolume = NormalizeConfigVolume(volume);

        foreach (AudioStreamPlayer2D sfxPlayer in _activeSfxPlayers)
        {
            sfxPlayer.VolumeDb = mappedVolume;
        }
    }

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
}
