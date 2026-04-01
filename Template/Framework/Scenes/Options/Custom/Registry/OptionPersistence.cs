using System;
using System.Collections.Generic;
using System.Text.Json;

#nullable enable

namespace __TEMPLATE__.Ui;

/// <summary>
/// Reads and writes custom option values into the JSON‑backed
/// <see cref="ResourceOptions.CustomOptionValues"/> dictionary.
/// </summary>
/// <param name="options">Resource options store that owns custom persisted values.</param>
internal sealed class OptionPersistence(ResourceOptions options)
{
    private readonly ResourceOptions _options = options;

    /// <summary>
    /// Builds a persistence key, using the explicit key when provided.
    /// </summary>
    /// <param name="tab">Option tab name.</param>
    /// <param name="label">Option label key.</param>
    /// <param name="explicitKey">Explicit key override, if provided.</param>
    /// <returns>Normalized persistence key.</returns>
    public static string GetSaveKey(string tab, string label, string? explicitKey = null)
    {
        string key = string.IsNullOrWhiteSpace(explicitKey) ? $"{tab}_{label}" : explicitKey;
        return NormalizeKey(key);
    }

    /// <summary>
    /// Legacy key shape used before tab-qualified persistence keys.
    /// </summary>
    /// <param name="label">Option label key.</param>
    /// <returns>Normalized legacy persistence key.</returns>
    public static string GetLegacySaveKey(string label)
    {
        return NormalizeKey(label);
    }

    // -- Typed getters (delegates to OptionValueParsers for JSON parsing) --

    /// <summary>
    /// Reads persisted slider value or stores default when missing.
    /// </summary>
    /// <param name="key">Primary persistence key.</param>
    /// <param name="defaultValue">Default value when missing or invalid.</param>
    /// <param name="fallbackKeys">Fallback keys checked for migration.</param>
    /// <returns>Resolved slider value.</returns>
    public float GetSliderValue(string key, float defaultValue, params string[] fallbackKeys) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseFloat, fallbackKeys);

    /// <summary>
    /// Reads persisted dropdown value or stores default when missing.
    /// </summary>
    /// <param name="key">Primary persistence key.</param>
    /// <param name="defaultValue">Default value when missing or invalid.</param>
    /// <param name="fallbackKeys">Fallback keys checked for migration.</param>
    /// <returns>Resolved dropdown value.</returns>
    public int GetDropdownValue(string key, int defaultValue, params string[] fallbackKeys) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseInt, fallbackKeys);

    /// <summary>
    /// Reads persisted line-edit value or stores default when missing.
    /// </summary>
    /// <param name="key">Primary persistence key.</param>
    /// <param name="defaultValue">Default value when missing or invalid.</param>
    /// <param name="fallbackKeys">Fallback keys checked for migration.</param>
    /// <returns>Resolved line-edit value.</returns>
    public string GetLineEditValue(string key, string defaultValue, params string[] fallbackKeys) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseString, fallbackKeys);

    /// <summary>
    /// Reads persisted toggle value or stores default when missing.
    /// </summary>
    /// <param name="key">Primary persistence key.</param>
    /// <param name="defaultValue">Default value when missing or invalid.</param>
    /// <param name="fallbackKeys">Fallback keys checked for migration.</param>
    /// <returns>Resolved toggle value.</returns>
    public bool GetToggleValue(string key, bool defaultValue, params string[] fallbackKeys) =>
        GetOrCreate(key, defaultValue, OptionValueParsers.ParseBool, fallbackKeys);

    // -- Typed setters --

    /// <summary>
    /// Stores a slider value.
    /// </summary>
    /// <param name="key">Persistence key.</param>
    /// <param name="value">Value to store.</param>
    public void SetSliderValue(string key, float value) => SetValue(key, value);

    /// <summary>
    /// Stores a dropdown value.
    /// </summary>
    /// <param name="key">Persistence key.</param>
    /// <param name="value">Value to store.</param>
    public void SetDropdownValue(string key, int value) => SetValue(key, value);

    /// <summary>
    /// Stores a line-edit value.
    /// </summary>
    /// <param name="key">Persistence key.</param>
    /// <param name="value">Value to store.</param>
    public void SetLineEditValue(string key, string value) => SetValue(key, value ?? string.Empty);

    /// <summary>
    /// Stores a toggle value.
    /// </summary>
    /// <param name="key">Persistence key.</param>
    /// <param name="value">Value to store.</param>
    public void SetToggleValue(string key, bool value) => SetValue(key, value);

    // -- Core persistence logic --

    /// <summary>
    /// Gets a typed value, migrating from fallback keys when necessary.
    /// </summary>
    /// <typeparam name="T">Typed value being retrieved.</typeparam>
    /// <param name="key">Primary persistence key.</param>
    /// <param name="defaultValue">Default value when missing or invalid.</param>
    /// <param name="tryParse">Parser for the stored JSON element.</param>
    /// <param name="fallbackKeys">Fallback keys checked for migration.</param>
    /// <returns>Resolved typed value.</returns>
    private T GetOrCreate<T>(
        string key,
        T defaultValue,
        Func<JsonElement, T, (bool Success, T Value)> tryParse,
        params string[] fallbackKeys)
    {
        string saveKey = NormalizeKey(key);
        Dictionary<string, JsonElement> values = _options.CustomOptionValues ??= [];

        // Return direct value when primary key resolves successfully.
        if (TryRead(values, saveKey, defaultValue, tryParse, out T direct))
            return direct;

        foreach (string fallback in fallbackKeys)
        {
            // Ignore empty fallback entries.
            if (string.IsNullOrWhiteSpace(fallback))
                continue;

            string fallbackKey = NormalizeKey(fallback);

            // Skip fallback when it resolves to the same normalized key.
            if (string.Equals(saveKey, fallbackKey, StringComparison.Ordinal))
                continue;

            // Skip fallback keys that do not contain a parseable value.
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

    /// <summary>
    /// Stores a typed value under a normalized key.
    /// </summary>
    /// <typeparam name="T">Value type to store.</typeparam>
    /// <param name="key">Persistence key.</param>
    /// <param name="value">Value to store.</param>
    private void SetValue<T>(string key, T value)
    {
        string saveKey = NormalizeKey(key);
        Dictionary<string, JsonElement> values = _options.CustomOptionValues ??= [];
        values[saveKey] = JsonSerializer.SerializeToElement(value);
    }

    /// <summary>
    /// Attempts to parse a stored JSON value into the requested type.
    /// </summary>
    /// <typeparam name="T">Requested value type.</typeparam>
    /// <param name="values">Dictionary containing persisted values.</param>
    /// <param name="key">Key to read.</param>
    /// <param name="defaultValue">Fallback value when parse fails.</param>
    /// <param name="tryParse">Parser delegate.</param>
    /// <param name="value">Parsed output value.</param>
    /// <returns><see langword="true"/> when parse succeeded.</returns>
    private static bool TryRead<T>(
        Dictionary<string, JsonElement> values,
        string key,
        T defaultValue,
        Func<JsonElement, T, (bool Success, T Value)> tryParse,
        out T value)
    {
        // Attempt parse only when the key exists in the persisted dictionary.
        if (values.TryGetValue(key, out JsonElement element))
        {
            (bool ok, T parsed) = tryParse(element, defaultValue);

            // Return parsed value when parser reports success.
            if (ok)
            {
                value = parsed;
                return true;
            }
        }

        value = defaultValue;
        return false;
    }

    /// <summary>
    /// Normalizes a persistence key and validates that it is non-empty.
    /// </summary>
    /// <param name="key">Raw key text.</param>
    /// <returns>Normalized key in PascalCase.</returns>
    private static string NormalizeKey(string key)
    {
        // Reject null/blank keys before normalization.
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Option key cannot be empty.", nameof(key));

        string normalized = SerializationKeys.ToPascalCase(key);

        // Reject keys that normalize to empty output.
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Option key resolved to an empty value.", nameof(key));

        return normalized;
    }
}
