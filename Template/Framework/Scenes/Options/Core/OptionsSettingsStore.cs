using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using FileAccess = Godot.FileAccess;

#nullable enable

namespace Framework.UI;

/// <summary>
/// Handles loading and saving ResourceOptions from/to options.json.
/// </summary>
internal sealed class OptionsSettingsStore
{
    private const string PathOptions = "user://options.json";
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public ResourceOptions Load()
    {
        if (FileAccess.FileExists(PathOptions))
        {
            using FileAccess file = FileAccess.Open(PathOptions, FileAccess.ModeFlags.Read);
            ResourceOptions options = JsonSerializer.Deserialize<ResourceOptions>(file.GetAsText()) ?? new();
            return options;
        }

        return new ResourceOptions();
    }

    public void Save(ResourceOptions options)
    {
        // Remove any inline values that correspond to actual typed properties so we
        // don't duplicate them in the JSON.  This can happen when game code defines
        // a property and also registers a "custom" option for the same key.
        if (options.CustomOptionValues != null)
        {
            HashSet<string> propNames = new HashSet<string>(
                typeof(ResourceOptions).GetProperties()
                    .Select(p => p.Name));

            foreach (string key in propNames)
            {
                options.CustomOptionValues.Remove(key);
            }
        }

        string json = JsonSerializer.Serialize(options, _jsonOptions);
        using FileAccess file = FileAccess.Open(PathOptions, FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }
}
