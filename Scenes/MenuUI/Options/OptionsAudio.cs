using Godot;

namespace Template.UI;

public partial class OptionsAudio : Control
{
    [Export] private OptionsManager _optionsManager;
    private ResourceOptions _options;

    public override void _Ready()
    {
        _options = _optionsManager.Options;
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

    private static void _on_music_value_changed(float v)
    {
        AudioManager.SetMusicVolume(v);
    }

    private static void _on_sounds_value_changed(float v)
    {
        AudioManager.SetSFXVolume(v);
    }
}

