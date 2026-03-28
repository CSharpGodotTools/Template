using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FileAccess = Godot.FileAccess;

#nullable enable

namespace __TEMPLATE__.Ui;

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
            return Migrate(options);
        }

        ResourceOptions defaults = new();
        defaults.Normalize();
        return defaults;
    }

    public void Save(ResourceOptions options)
    {
        options.Normalize();

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

    private static ResourceOptions Migrate(ResourceOptions options)
    {
        // Schema version 0 is treated as legacy unversioned options files.
        if (options.SchemaVersion <= 0)
            options.SchemaVersion = 1;

        // Current migration path keeps data shape stable and normalizes invalid values.
        // Future schema transitions should be handled here in ascending-version steps.
        options.Normalize();
        return options;
    }
}
