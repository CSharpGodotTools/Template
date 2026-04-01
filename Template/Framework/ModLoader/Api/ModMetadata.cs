namespace __TEMPLATE__.Mods;

/// <summary>
/// Immutable metadata snapshot for a loaded mod.
/// </summary>
/// <param name="id">Unique mod identifier.</param>
/// <param name="name">Display name of the mod.</param>
/// <param name="author">Author declared by the mod.</param>
/// <param name="modVersion">Version string declared by the mod package.</param>
/// <param name="gameVersion">Target game version declared by the mod package.</param>
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
