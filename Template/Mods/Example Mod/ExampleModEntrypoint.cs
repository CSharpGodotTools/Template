using __TEMPLATE__.Mods;

namespace ExampleMod;

/// <summary>
/// Minimal sample mod entrypoint used by the example mod package.
/// </summary>
public sealed class ExampleModEntrypoint : IModEntrypoint
{
    /// <summary>
    /// Runs when the mod is loaded by the framework.
    /// </summary>
    /// <param name="context">Mod context exposing logging and game integration APIs.</param>
    public void OnLoad(IModContext context)
    {
        context.Log("Hello from managed C# mod entrypoint.");
    }
}
