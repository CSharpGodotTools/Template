using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace __TEMPLATE__;

/// <summary>
/// Serializable options root that stores schema version and custom values.
/// </summary>
public sealed class ResourceOptions
{
    /// <summary>
    /// Current schema version for persisted options files.
    /// </summary>
    public const int CurrentSchemaVersion = 3;

    /// <summary>
    /// Gets or sets schema version of the serialized options payload.
    /// </summary>
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    // Custom options are persisted inline at the root of options.json.
    /// <summary>
    /// Gets or sets custom option values persisted as extension JSON fields.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> CustomOptionValues { get; set; } = [];

    /// <summary>
    /// Normalizes schema version and ensures custom value storage exists.
    /// </summary>
    public void Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        CustomOptionValues ??= [];
    }
}
