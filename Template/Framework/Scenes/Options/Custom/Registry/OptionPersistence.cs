using System;
using System.Collections.Generic;
using System.Text.Json;

#nullable enable

namespace __TEMPLATE__.Ui;

/// <summary>
/// Reads and writes custom option values into the JSON‑backed
/// <see cref="ResourceOptions.CustomOptionValues"/> dictionary.
/// </summary>
internal sealed class OptionPersistence(ResourceOptions options)
{
    private readonly ResourceOptions _options = options;

    /// <summary>
    /// Builds a persistence key, using the explicit key when provided.
    /// </summary>
    public string GetSaveKey(string tab, string label, string? explicitKey = null)
    {
        string key = string.IsNullOrWhiteSpace(explicitKey) ? $"{tab}_{label}" : explicitKey;
        return NormalizeKey(key);
    }

    /// <summary>
    /// Legacy key shape used before tab-qualified persistence keys.
    /// </summary>
    public string GetLegacySaveKey(string label)
    {
        return NormalizeKey(label);
    }

    // -- Typed getters (delegates to OptionValueParsers for JSON parsing) --

    public float GetSliderValue(string key, float defaultValue, params string[] fallbackKeys) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseFloat, fallbackKeys);

    public int GetDropdownValue(string key, int defaultValue, params string[] fallbackKeys) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseInt, fallbackKeys);

    public string GetLineEditValue(string key, string defaultValue, params string[] fallbackKeys) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseString, fallbackKeys);

    public bool GetToggleValue(string key, bool defaultValue, params string[] fallbackKeys) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseBool, fallbackKeys);

    // -- Typed setters --

    public void SetSliderValue(string key, float value) => SetValue(key, value);
    public void SetDropdownValue(string key, int value) => SetValue(key, value);
    public void SetLineEditValue(string key, string value) => SetValue(key, value ?? string.Empty);
    public void SetToggleValue(string key, bool value) => SetValue(key, value);

    // -- Core persistence logic --

    private T GetOrCreate<T>(
        string key,
        T defaultValue,
        Func<JsonElement, T, (bool Success, T Value)> tryParse,
        params string[] fallbackKeys)
    {
        string saveKey = NormalizeKey(key);
        Dictionary<string, JsonElement> values = _options.CustomOptionValues ??= [];

        if (TryRead(values, saveKey, defaultValue, tryParse, out T direct))
            return direct;

        foreach (string fallback in fallbackKeys)
        {
            if (string.IsNullOrWhiteSpace(fallback))
                continue;

            string fallbackKey = NormalizeKey(fallback);

            if (string.Equals(saveKey, fallbackKey, StringComparison.Ordinal))
                continue;

            if (!TryRead(values, fallbackKey, defaultValue, tryParse, out T migrated))
                continue;

            SetValue(saveKey, migrated);
            values.Remove(fallbackKey);
            return migrated;
        }

        // Missing or unparseable — persist the default and return it
        SetValue(saveKey, defaultValue);
        return defaultValue;
    }

    private void SetValue<T>(string key, T value)
    {
        string saveKey = NormalizeKey(key);
        Dictionary<string, JsonElement> values = _options.CustomOptionValues ??= [];
        values[saveKey] = JsonSerializer.SerializeToElement(value);
    }

    private static bool TryRead<T>(
        IReadOnlyDictionary<string, JsonElement> values,
        string key,
        T defaultValue,
        Func<JsonElement, T, (bool Success, T Value)> tryParse,
        out T value)
    {
        if (values.TryGetValue(key, out JsonElement element))
        {
            (bool ok, T parsed) = tryParse(element, defaultValue);

            if (ok)
            {
                value = parsed;
                return true;
            }
        }

        value = defaultValue;
        return false;
    }

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Option key cannot be empty.", nameof(key));

        string normalized = SerializationKeys.ToPascalCase(key);

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Option key resolved to an empty value.", nameof(key));

        return normalized;
    }
}
