using GodotUtils;
using VSyncMode = Godot.DisplayServer.VSyncMode;

namespace __TEMPLATE__.Ui;

internal sealed class OptionsSettingDispatcherComponent
{
    private const int DefaultWindowWidth = 0;
    private const int DefaultWindowHeight = 0;

    private readonly OptionsValueStoreComponent _valueStore;
    private readonly OptionsDisplaySettingsComponent _displaySettings;
    private readonly OptionsAudioSettingsComponent _audioSettings;

    public OptionsSettingDispatcherComponent(
        OptionsValueStoreComponent valueStore,
        OptionsDisplaySettingsComponent displaySettings,
        OptionsAudioSettingsComponent audioSettings)
    {
        _valueStore = valueStore;
        _displaySettings = displaySettings;
        _audioSettings = audioSettings;
    }

    public int ReadOptionInt(string key, int defaultValue) => _valueStore.GetInt(key, defaultValue);
    public float ReadOptionFloat(string key, float defaultValue) => _valueStore.GetFloat(key, defaultValue);
    public string ReadOptionString(string key, string defaultValue) => _valueStore.GetString(key, defaultValue);
    public bool ReadOptionBool(string key, bool defaultValue) => _valueStore.GetBool(key, defaultValue);

    public void SetIntSetting(string key, int value)
    {
        switch (key)
        {
            case FrameworkOptionsSaveKeys.Language:
                _displaySettings.SetLanguage(value);
                return;
            case FrameworkOptionsSaveKeys.QualityPreset:
                _displaySettings.SetQualityPreset(value);
                return;
            case FrameworkOptionsSaveKeys.Antialiasing:
                _displaySettings.SetAntialiasing(value);
                return;
            case FrameworkOptionsSaveKeys.WindowMode:
                _displaySettings.SetWindowMode((WindowMode)value);
                return;
            case FrameworkOptionsSaveKeys.WindowWidth:
                _displaySettings.SetWindowSize(value, ReadOptionInt(FrameworkOptionsSaveKeys.WindowHeight, DefaultWindowHeight));
                return;
            case FrameworkOptionsSaveKeys.WindowHeight:
                _displaySettings.SetWindowSize(ReadOptionInt(FrameworkOptionsSaveKeys.WindowWidth, DefaultWindowWidth), value);
                return;
            case FrameworkOptionsSaveKeys.VSyncMode:
                _displaySettings.SetVSyncMode((VSyncMode)value);
                return;
            default:
                _valueStore.SetInt(key, value);
                return;
        }
    }

    public void SetFloatSetting(string key, float value)
    {
        switch (key)
        {
            case FrameworkOptionsSaveKeys.MusicVolume:
                _audioSettings.SetMusicVolume(value);
                return;
            case FrameworkOptionsSaveKeys.SfxVolume:
                _audioSettings.SetSfxVolume(value);
                return;
            case FrameworkOptionsSaveKeys.MaxFps:
                _displaySettings.SetMaxFps((int)value);
                return;
            default:
                _valueStore.SetFloat(key, value);
                return;
        }
    }

    public void SetStringSetting(string key, string value) => _valueStore.SetString(key, value);
    public void SetBoolSetting(string key, bool value) => _valueStore.SetBool(key, value);
}
