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

    /// <summary>
    /// Initializes typed options settings delegates.
    /// </summary>
    /// <param name="getInt">Integer getter delegate.</param>
    /// <param name="getFloat">Float getter delegate.</param>
    /// <param name="getString">String getter delegate.</param>
    /// <param name="getBool">Boolean getter delegate.</param>
    /// <param name="setInt">Integer setter delegate.</param>
    /// <param name="setFloat">Float setter delegate.</param>
    /// <param name="setString">String setter delegate.</param>
    /// <param name="setBool">Boolean setter delegate.</param>
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

    /// <summary>
    /// Reads an integer option by key.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when key is absent.</param>
    /// <returns>Resolved integer option value.</returns>
    internal int GetInt(string key, int defaultValue) => _getInt(key, defaultValue);

    /// <summary>
    /// Reads a float option by key.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when key is absent.</param>
    /// <returns>Resolved float option value.</returns>
    internal float GetFloat(string key, float defaultValue) => _getFloat(key, defaultValue);

    /// <summary>
    /// Reads a string option by key.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when key is absent.</param>
    /// <returns>Resolved string option value.</returns>
    internal string GetString(string key, string defaultValue) => _getString(key, defaultValue);

    /// <summary>
    /// Reads a boolean option by key.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="defaultValue">Fallback value when key is absent.</param>
    /// <returns>Resolved boolean option value.</returns>
    internal bool GetBool(string key, bool defaultValue) => _getBool(key, defaultValue);

    /// <summary>
    /// Writes an integer option by key.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    internal void SetInt(string key, int value) => _setInt(key, value);

    /// <summary>
    /// Writes a float option by key.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    internal void SetFloat(string key, float value) => _setFloat(key, value);

    /// <summary>
    /// Writes a string option by key.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    internal void SetString(string key, string value) => _setString(key, value);

    /// <summary>
    /// Writes a boolean option by key.
    /// </summary>
    /// <param name="key">Option key.</param>
    /// <param name="value">Value to store.</param>
    internal void SetBool(string key, bool value) => _setBool(key, value);

    // These keys are persisted by the options manager but are not declared through OptionDefinitions.
    /// <summary>
    /// Gets or sets persisted window width.
    /// </summary>
    public int WindowWidth
    {
        get => GetInt(FrameworkOptionsSaveKeys.WindowWidth, 0);
        set => SetInt(FrameworkOptionsSaveKeys.WindowWidth, Math.Max(0, value));
    }

    /// <summary>
    /// Gets or sets persisted window height.
    /// </summary>
    public int WindowHeight
    {
        get => GetInt(FrameworkOptionsSaveKeys.WindowHeight, 0);
        set => SetInt(FrameworkOptionsSaveKeys.WindowHeight, Math.Max(0, value));
    }
}
