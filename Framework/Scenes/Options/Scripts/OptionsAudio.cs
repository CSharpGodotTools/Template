using Godot;

namespace __TEMPLATE__.UI;

public partial class OptionsAudio
{
    private ResourceOptions _options;
    private readonly Options options;

    public OptionsAudio(Options options)
    {
        this.options = options;

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
