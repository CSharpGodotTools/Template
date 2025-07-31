using Godot;
using GodotUtils;

namespace __TEMPLATE__;

public partial class AudioManager : Component
{
    private const float MinRandomPitch        = 0.8f;
    private const float MaxRandomPitch        = 1.2f;
    private const float RandomPitchThreshold  = 0.1f;
    private const int   MutedVolume           = -80;
    private const int   MutedVolumeNormalized = -40;

    private GAudioPlayer    _musicPlayer;
    private ResourceOptions _options;
    private Node            _sfxPlayersParent;
    private float           _lastPitch;

    public override void Ready()
    {
        _options = GetNode<OptionsManager>(AutoloadPaths.OptionsManager).Options;
        _musicPlayer = new GAudioPlayer(this);

        _sfxPlayersParent = new Node();
        AddChild(_sfxPlayersParent);
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

    public void PlaySFX(AudioStream sound)
    {
        // Setup the SFX stream player
        GAudioPlayer sfxPlayer = new(_sfxPlayersParent, true)
        {
            Stream = sound,
            Volume = _options.SFXVolume
        };

        // Randomize the pitch
        RandomNumberGenerator rng = new();

        rng.Randomize();

        float pitch = rng.RandfRange(MinRandomPitch, MaxRandomPitch);

        // Ensure the current pitch is not the same as the last
        while (Mathf.Abs(pitch - _lastPitch) < RandomPitchThreshold)
        {
            rng.Randomize();
            pitch = rng.RandfRange(MinRandomPitch, MaxRandomPitch);
        }

        _lastPitch = pitch;

        // Play the sound
        sfxPlayer.Pitch = pitch;
        sfxPlayer.Play();
    }

    /// <summary>
    /// Gradually fade out all sounds
    /// </summary>
    public void FadeOutSFX(double fadeTime = 1)
    {
        foreach (AudioStreamPlayer audioPlayer in _sfxPlayersParent.GetChildren<AudioStreamPlayer>())
        {
            new GTween(audioPlayer).Animate(AudioStreamPlayer.PropertyName.VolumeDb, MutedVolume, fadeTime);
        }
    }

    public void SetMusicVolume(float v)
    {
        _musicPlayer.Volume = v;
        _options.MusicVolume = _musicPlayer.Volume;
    }

    public void SetSFXVolume(float v)
    {
        // Set volume for future SFX players
        _options.SFXVolume = v;

        // Can't cast to GAudioPlayer so will have to remap manually again
        v = NormalizeConfigVolume(v);

        // Set volume of all SFX players currently in the scene
        foreach (AudioStreamPlayer audioPlayer in _sfxPlayersParent.GetChildren())
        {
            audioPlayer.VolumeDb = v;
        }
    }

    private static void PlayAudio(GAudioPlayer player, AudioStream song)
    {
        player.Stream = song;
        player.Play();
    }

    private static void PlayAudio(GAudioPlayer player, AudioStream song, float volume)
    {
        player.Stream = song;
        player.Volume = volume;
        player.Play();
    }

    private static void PlayAudioCrossfade(GAudioPlayer player, AudioStream song, float volume, double fadeOut, double fadeIn)
    {
        // Transition from current song being played to new song
        new GTween(player.Internal)
            .SetAnimatingProp(AudioStreamPlayer.PropertyName.VolumeDb)
            // Fade out current song
            .AnimateProp(MutedVolume, fadeOut).EaseIn()
            // Set to new song
            .Callback(() => PlayAudio(player, song))
            // Fade in to current song
            .AnimateProp(NormalizeConfigVolume(volume), fadeIn).EaseIn();
    }

    private static float NormalizeConfigVolume(float volume)
    {
        return volume == 0 ? MutedVolume : volume.Remap(0, 100, MutedVolumeNormalized, 0);
    }
}
