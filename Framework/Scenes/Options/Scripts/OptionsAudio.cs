using Godot;

namespace __TEMPLATE__.UI;

public partial class OptionsAudio(Options options)
{
    private ResourceOptions _options;

    public void Initialize()
    {
        GetOptions();
        SetupMusic();
        SetupSounds();
    }

    private void GetOptions()
    {
        _options = Game.Options.GetOptions();
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
        Game.Audio.SetMusicVolume((float)v);
    }

    private void OnSoundsValueChanged(double v)
    {
        Game.Audio.SetSFXVolume((float)v);
    }
}
