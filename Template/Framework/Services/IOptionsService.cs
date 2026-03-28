using __TEMPLATE__.Ui;
using GodotUtils;
using System;
using VSyncMode = Godot.DisplayServer.VSyncMode;

namespace __TEMPLATE__;

public interface IOptionsService
{
    event Action<WindowMode> WindowModeChanged;

    ResourceOptions Settings { get; }

    string GetCurrentTab();
    void SetCurrentTab(string tab);

    ResourceHotkeys GetHotkeys();
    void ResetHotkeys();

    void AddSlider(SliderOptionDefinition option);
    void AddDropdown(DropdownOptionDefinition option);
    void AddLineEdit(LineEditOptionDefinition option);
    void AddToggle(ToggleOptionDefinition option);

    void SetMusicVolume(float volume);
    void SetSFXVolume(float volume);
    void SetLanguage(Language language);
    void SetQualityPreset(QualityPreset qualityPreset);
    void SetAntialiasing(int antialiasing);
    void SetWindowMode(WindowMode windowMode);
    void SetWindowSize(int width, int height);
    void SetResolution(int resolution);
    void SetVSyncMode(VSyncMode vsyncMode);
    void SetMaxFPS(int maxFps);
    void SetDifficulty(Difficulty difficulty);
    void SetMouseSensitivity(float sensitivity);
}
