using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

#nullable enable

namespace Framework.UI;

/// <summary>
/// Reads and writes custom option values into the JSON‑backed
/// <see cref="ResourceOptions.CustomOptionValues"/> dictionary.
/// </summary>
internal sealed class OptionPersistence(ResourceOptions options)
{
    private readonly ResourceOptions _options = options;

    /// <summary>
    /// Converts an option label to a PascalCase persistence key.
    /// </summary>
    public string GetSaveKey(string label) => SerializationKeys.ToPascalCase(label);

    // -- Typed getters (delegates to OptionValueParsers for JSON parsing) --

    public float GetSliderValue(string key, float defaultValue) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseFloat);

    public int GetDropdownValue(string key, int defaultValue) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseInt);

    public string GetLineEditValue(string key, string defaultValue) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseString);

    public bool GetToggleValue(string key, bool defaultValue) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseBool);

    // -- Typed setters --

    public void SetSliderValue(string key, float value) => SetValue(key, value);
    public void SetDropdownValue(string key, int value) => SetValue(key, value);
    public void SetLineEditValue(string key, string value) => SetValue(key, value ?? string.Empty);
    public void SetToggleValue(string key, bool value) => SetValue(key, value);

    // -- Core persistence logic --

    private T GetOrCreate<T>(
        string key, T defaultValue,
        Func<JsonElement, T, (bool Success, T Value)> tryParse)
    {
        // A typed CLR property on ResourceOptions takes precedence
        if (TryGetProperty(key, out object? raw) && raw is T typed)
            return typed;

        Dictionary<string, JsonElement> values = _options.CustomOptionValues ??= [];

        if (values.TryGetValue(key, out JsonElement element))
        {
            (bool ok, T val) = tryParse(element, defaultValue);
            if (ok)
                return val;
        }

        // Missing or unparseable — persist the default and return it
        SetValue(key, defaultValue);
        return defaultValue;
    }

    private void SetValue<T>(string key, T value)
    {
        // Don't overwrite a typed property that already lives on the resource
        if (TryGetProperty(key, out _))
            return;

        Dictionary<string, JsonElement> values = _options.CustomOptionValues ??= [];
        values[key] = JsonSerializer.SerializeToElement(value);
    }

    private bool TryGetProperty(string key, out object? value)
    {
        PropertyInfo? prop = typeof(ResourceOptions).GetProperty(key);

        if (prop != null)
        {
            value = prop.GetValue(_options);
            return true;
        }

        value = null;
        return false;
    }
}
