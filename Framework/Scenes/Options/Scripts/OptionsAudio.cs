using Godot;
using GodotUtils;
using GodotUtils.UI;

namespace __TEMPLATE__.UI;

public partial class OptionsAudio : Control
{
    private ResourceOptions _options;

    public override void _Ready()
    {
        _options = OptionsManager.GetOptions();

        SetupMusic();
        SetupSounds();
    }

    private void _OnMusicValueChanged(float v)
    {
        AudioManager.SetMusicVolume(v);
    }

    private void _OnSoundsValueChanged(float v)
    {
        AudioManager.SetSFXVolume(v);
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
}
