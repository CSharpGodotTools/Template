namespace Framework.Mods;

/// <summary>
/// Immutable metadata snapshot for a loaded mod.
/// </summary>
public sealed class ModMetadata(string id, string name, string author, string modVersion, string gameVersion)
{
    /// <summary>
    /// Unique mod identifier.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Display name of the mod.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Author declared by the mod.
    /// </summary>
    public string Author { get; } = author;

    /// <summary>
    /// Version string declared by the mod package.
    /// </summary>
    public string ModVersion { get; } = modVersion;

    /// <summary>
    /// Target game version declared by the mod package.
    /// </summary>
    public string GameVersion { get; } = gameVersion;
}
