using Godot;

namespace __TEMPLATE__;

public interface IAudioService
{
    void PlayMusic(AudioStream song, bool instant = true, double fadeOut = 1.5, double fadeIn = 0.5);
    void PlaySFX(AudioStream sound, Vector2 position, float minPitch = 0.8f, float maxPitch = 1.2f);
    void FadeOutSFX(double fadeTime = 1);
    void SetMusicVolume(float volume);
    void SetSFXVolume(float volume);
}
