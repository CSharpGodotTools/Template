using Godot;
using System;

namespace Framework.UI;

public partial class OptionsAudio : IDisposable
{
    // Fields
    private ResourceOptions _options;
    private readonly HSlider _musicSlider;
    private readonly HSlider _sfxSlider;

    public OptionsAudio(Options options)
    {
        _musicSlider = options.GetNode<HSlider>("%Music");
        _sfxSlider = options.GetNode<HSlider>("%Sounds");

        GetOptions();
        SetupMusic();
        SetupSounds();
    }

    public void Dispose()
    {
        _musicSlider.ValueChanged -= OnMusicValueChanged;
        _sfxSlider.ValueChanged -= OnSoundsValueChanged;
        GC.SuppressFinalize(this);
    }

    private void GetOptions()
    {
        _options = GameFramework.Options.GetOptions();
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
