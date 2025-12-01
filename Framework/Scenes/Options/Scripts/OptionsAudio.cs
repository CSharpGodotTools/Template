using Godot;

namespace GodotUtils.UI;

public partial class OptionsAudio(Options options)
{
    private ResourceOptions _options;

    public void Initialize()
    {
        _options = OptionsManager.GetOptions();

        SetupMusic();
        SetupSounds();
    }

    private void SetupMusic()
    {
        HSlider slider = options.GetNode<HSlider>("%Music");
        slider.Value = _options.MusicVolume;
        slider.ValueChanged += OnMusicValueChanged;
    }

    private void SetupSounds()
    {
        HSlider slider = options.GetNode<HSlider>("%Sounds");
        slider.Value = _options.SFXVolume;
        slider.ValueChanged += OnSoundsValueChanged;
    }

    private void OnMusicValueChanged(double v)
    {
        AudioManager.SetMusicVolume((float)v);
    }

    private void OnSoundsValueChanged(double v)
    {
        AudioManager.SetSFXVolume((float)v);
    }
}
