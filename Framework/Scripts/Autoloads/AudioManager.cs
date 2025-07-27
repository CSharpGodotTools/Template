using Godot;
using GodotUtils;

namespace __TEMPLATE__;

public partial class AudioManager : Node
{
    [Export] 
    private OptionsManager         _optionsManager;
    private static GAudioPlayer    _musicPlayer;
    private static Node            _sfxPlayersParent;
    private static float           _lastPitch;
    private static ResourceOptions _options;

    public override void _Ready()
    {
        _options = _optionsManager.Options;
        _musicPlayer = new GAudioPlayer(this);

        _sfxPlayersParent = new Node();
        AddChild(_sfxPlayersParent);

        //PlayMusic(Music.Menu);
    }

    public static void PlayMusic(AudioStream song, bool instant = true, double fadeOut = 1.5, double fadeIn = 0.5)
    {
        if (!instant && _musicPlayer.Playing)
        {
            float volume = _options.MusicVolume;
            float volumeRemapped =
                volume == 0 ? -80 : volume.Remap(0, 100, -40, 0);

            // Transition from current song being played to new song
            new GTween(_musicPlayer.Internal)
                .SetAnimatingProp(AudioStreamPlayer.PropertyName.VolumeDb)
                // Fade out current song
                .AnimateProp(-80, fadeOut).EaseIn()
                // Set to new song
                .Callback(() =>
                {
                    _musicPlayer.Stream = song;
                    _musicPlayer.Play();
                })
                // Fade in to current song
                .AnimateProp(volumeRemapped, fadeIn).EaseIn();
        }
        else
        {
            // Instantly switch to and play new song
            _musicPlayer.Stream = song;
            _musicPlayer.Volume = _options.MusicVolume;
            _musicPlayer.Play();
        }
    }

    public static void PlaySFX(AudioStream sound)
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
        float pitch = rng.RandfRange(0.8f, 1.2f);

        // Ensure the current pitch is not the same as the last
        while (Mathf.Abs(pitch - _lastPitch) < 0.1f)
        {
            rng.Randomize();
            pitch = rng.RandfRange(0.8f, 1.2f);
        }

        _lastPitch = pitch;

        // Play the sound
        sfxPlayer.Pitch = pitch;
        sfxPlayer.Play();
    }

    /// <summary>
    /// Gradually fade out all sounds
    /// </summary>
    public static void FadeOutSFX(double fadeTime = 1)
    {
        foreach (AudioStreamPlayer audioPlayer in _sfxPlayersParent.GetChildren<AudioStreamPlayer>())
        {
            new GTween(audioPlayer)
                .Animate(AudioStreamPlayer.PropertyName.VolumeDb, -80, fadeTime);
        }
    }

    public static void SetMusicVolume(float v)
    {
        _musicPlayer.Volume = v;
        _options.MusicVolume = _musicPlayer.Volume;
    }

    public static void SetSFXVolume(float v)
    {
        // Set volume for future SFX players
        _options.SFXVolume = v;

        // Can't cast to GAudioPlayer so will have to remap manually again
        v = v == 0 ? -80 : v.Remap(0, 100, -40, 0);

        // Set volume of all SFX players currently in the scene
        foreach (AudioStreamPlayer audioPlayer in _sfxPlayersParent.GetChildren())
        {
            audioPlayer.VolumeDb = v;
        }
    }
}

