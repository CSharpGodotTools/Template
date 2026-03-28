using Godot;
using System;

namespace __TEMPLATE__.Ui;

public partial class OptionsAudio : IDisposable
{
    // Fields
    private readonly OptionsManager _optionsManager;
    private readonly AudioManager _audioManager;
    private readonly HSlider _musicSlider;
    private readonly HSlider _sfxSlider;

    public OptionsAudio(Options options, OptionsManager optionsManager, AudioManager audioManager)
    {
        _musicSlider = options.GetNode<HSlider>("%Music");
        _sfxSlider = options.GetNode<HSlider>("%Sounds");
        _optionsManager = optionsManager;
        _audioManager = audioManager;

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
        _musicSlider.Value = _optionsManager.Settings.MusicVolume;
        _musicSlider.ValueChanged += OnMusicValueChanged;
    }

    private void SetupSounds()
    {
        _sfxSlider.Value = _optionsManager.Settings.SFXVolume;
        _sfxSlider.ValueChanged += OnSoundsValueChanged;
    }

    private void OnMusicValueChanged(double v)
    {
        _audioManager.SetMusicVolume((float)v);
    }

    private void OnSoundsValueChanged(double v)
    {
        _audioManager.SetSFXVolume((float)v);
    }
}
