namespace Framework.Mods;

/// <summary>
/// Contract implemented by managed C# mods.
/// </summary>
public interface IModEntrypoint
{
    /// <summary>
    /// Called after the mod assembly is loaded and the mod context has been created.
    /// </summary>
    /// <param name="context">Runtime context exposed to the mod entrypoint.</param>
    void OnLoad(IModContext context);
}
