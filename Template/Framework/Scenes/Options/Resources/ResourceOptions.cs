using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace __TEMPLATE__;

public sealed class ResourceOptions
{
    public const int CurrentSchemaVersion = 3;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    // Custom options are persisted inline at the root of options.json.
    [JsonExtensionData]
    public Dictionary<string, JsonElement> CustomOptionValues { get; set; } = [];

    public void Normalize()
    {
        SchemaVersion = CurrentSchemaVersion;
        CustomOptionValues ??= [];
    }
}
