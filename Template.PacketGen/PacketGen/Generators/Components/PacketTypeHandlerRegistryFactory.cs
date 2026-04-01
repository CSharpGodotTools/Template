using PacketGen.Generators.TypeHandlers;

namespace PacketGen.Generators;

/// <summary>
/// Creates fully-wired type-handler registries for packet generation.
/// </summary>
internal sealed class PacketTypeHandlerRegistryFactory
{
    /// <summary>
    /// Creates and configures the type-handler registry used by emitters.
    /// </summary>
    /// <returns>Configured type-handler registry.</returns>
    public TypeHandlerRegistry Create()
    {
        TypeHandlerRegistry registry = new();

        PrimitiveTypeHandler primitives = new();
        ArrayTypeHandler arrays = new(registry);
        ListTypeHandler lists = new(registry);
        DictionaryTypeHandler dictionaries = new(registry);
        ComplexTypeHandler complexTypes = new(registry);

        registry.SetHandlers(
        [
            primitives,
            arrays,
            lists,
            dictionaries,
            complexTypes
        ]);

        return registry;
    }
}
