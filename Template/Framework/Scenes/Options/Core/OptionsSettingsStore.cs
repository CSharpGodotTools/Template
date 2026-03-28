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
