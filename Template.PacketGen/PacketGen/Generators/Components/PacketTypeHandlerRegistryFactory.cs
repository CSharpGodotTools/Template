using PacketGen.Generators.TypeHandlers;

namespace PacketGen.Generators;

internal sealed class PacketTypeHandlerRegistryFactory
{
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
