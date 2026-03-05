using Godot;
using System;

namespace Framework.Ui;

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
        _options = GameFramework.Settings;

        SetupMusic();
        SetupSounds();
    }

    public void Dispose()
    {
        _musicSlider.ValueChanged -= OnMusicValueChanged;
        _sfxSlider.ValueChanged -= OnSoundsValueChanged;
        GC.SuppressFinalize(this);
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
        GameFramework.Audio.SetMusicVolume((float)v);
    }

    private void OnSoundsValueChanged(double v)
    {
        GameFramework.Audio.SetSFXVolume((float)v);
    }
}
