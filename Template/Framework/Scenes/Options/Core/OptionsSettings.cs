using System;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Strongly typed settings access surface used by scripts and game systems.
/// Additional properties are source-generated from OptionDefinitions saveKey entries.
/// </summary>
public sealed partial class OptionsSettings
{
    private readonly Func<string, int, int> _getInt;
    private readonly Func<string, float, float> _getFloat;
    private readonly Func<string, string, string> _getString;
    private readonly Func<string, bool, bool> _getBool;

    private readonly Action<string, int> _setInt;
    private readonly Action<string, float> _setFloat;
    private readonly Action<string, string> _setString;
    private readonly Action<string, bool> _setBool;

    internal OptionsSettings(
        Func<string, int, int> getInt,
        Func<string, float, float> getFloat,
        Func<string, string, string> getString,
        Func<string, bool, bool> getBool,
        Action<string, int> setInt,
        Action<string, float> setFloat,
        Action<string, string> setString,
        Action<string, bool> setBool)
    {
        _getInt = getInt;
        _getFloat = getFloat;
        _getString = getString;
        _getBool = getBool;
        _setInt = setInt;
        _setFloat = setFloat;
        _setString = setString;
        _setBool = setBool;
    }

    internal int GetInt(string key, int defaultValue) => _getInt(key, defaultValue);
    internal float GetFloat(string key, float defaultValue) => _getFloat(key, defaultValue);
    internal string GetString(string key, string defaultValue) => _getString(key, defaultValue);
    internal bool GetBool(string key, bool defaultValue) => _getBool(key, defaultValue);

    internal void SetInt(string key, int value) => _setInt(key, value);
    internal void SetFloat(string key, float value) => _setFloat(key, value);
    internal void SetString(string key, string value) => _setString(key, value);
    internal void SetBool(string key, bool value) => _setBool(key, value);

    // These keys are persisted by the options manager but are not declared through OptionDefinitions.
    public int WindowWidth
    {
        get => GetInt(FrameworkOptionsSaveKeys.WindowWidth, 0);
        set => SetInt(FrameworkOptionsSaveKeys.WindowWidth, Math.Max(0, value));
    }

    public int WindowHeight
    {
        get => GetInt(FrameworkOptionsSaveKeys.WindowHeight, 0);
        set => SetInt(FrameworkOptionsSaveKeys.WindowHeight, Math.Max(0, value));
    }
}
