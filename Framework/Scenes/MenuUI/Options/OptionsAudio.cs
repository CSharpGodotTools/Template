using Godot;
using GodotUtils;
using GodotUtils.UI;

namespace __TEMPLATE__.UI;

public partial class OptionsAudio : Control
{
    private ResourceOptions _options;
    private AudioManager _audioManager;

    public override void _Ready()
    {
        _options = GetNode<OptionsManager>(AutoloadPaths.OptionsManager).Options;
        _audioManager = GetNode<AudioManager>(AutoloadPaths.AudioManager);

        SetupMusic();
        SetupSounds();
    }

    private void SetupMusic()
    {
        HSlider slider = GetNode<HSlider>("%Music");
        slider.Value = _options.MusicVolume;
    }

    private void SetupSounds()
    {
        HSlider slider = GetNode<HSlider>("%Sounds");
        slider.Value = _options.SFXVolume;
    }

    private void _OnMusicValueChanged(float v)
    {
        _audioManager.SetMusicVolume(v);
    }

    private void _OnSoundsValueChanged(float v)
    {
        _audioManager.SetSFXVolume(v);
    }
}
