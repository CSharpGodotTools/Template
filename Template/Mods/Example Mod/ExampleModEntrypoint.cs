using __TEMPLATE__.Mods;

namespace ExampleMod;

public sealed class ExampleModEntrypoint : IModEntrypoint
{
    public void OnLoad(IModContext context)
    {
        context.Log("Hello from managed C# mod entrypoint.");
    }
}
