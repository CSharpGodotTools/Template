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

    /// <summary>
    /// Loads options from disk or creates normalized defaults when absent.
    /// </summary>
    /// <returns>Loaded and normalized options resource.</returns>
    public static ResourceOptions Load()
    {
        // Load persisted options only when the options file exists on disk.
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

    /// <summary>
    /// Normalizes and saves options to persistent storage.
    /// </summary>
    /// <param name="options">Options resource to save.</param>
    public void Save(ResourceOptions options)
    {
        options.Normalize();

        string json = JsonSerializer.Serialize(options, _jsonOptions);
        using FileAccess file = FileAccess.Open(PathOptions, FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }

    /// <summary>
    /// Applies schema migrations and normalization to loaded options.
    /// </summary>
    /// <param name="options">Options resource to migrate.</param>
    /// <returns>Migrated and normalized options resource.</returns>
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
