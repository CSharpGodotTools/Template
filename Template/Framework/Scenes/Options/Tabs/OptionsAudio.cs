using Godot;
using System;

namespace __TEMPLATE__.Ui;

public partial class OptionsAudio : IDisposable
{
    // Fields
    private readonly ResourceOptions _options;
    private readonly HSlider _musicSlider;
    private readonly HSlider _sfxSlider;

    public OptionsAudio(Options options)
    {
        _musicSlider = options.GetNode<HSlider>("%Music");
        _sfxSlider = options.GetNode<HSlider>("%Sounds");
        _options = Game.Settings;

        SetupMusic();
        SetupSounds();
    }

    public void Dispose()
    {
        _musicSlider.ValueChanged -= OnMusicValueChanged;
        _sfxSlider.ValueChanged -= OnSoundsValueChanged;
    }

    private void SetupMusic()
    {
        _musicSlider.Value = _options.MusicVolume;
        _musicSlider.ValueChanged += OnMusicValueChanged;
    }

    private void SetupSounds()
    {
        _sfxSlider.Value = _options.SFXVolume;
        _sfxSlider.ValueChanged += OnSoundsValueChanged;
    }

    private void OnMusicValueChanged(double v)
    {
        Game.Audio.SetMusicVolume((float)v);
    }

    private void OnSoundsValueChanged(double v)
    {
        Game.Audio.SetSFXVolume((float)v);
    }
}
