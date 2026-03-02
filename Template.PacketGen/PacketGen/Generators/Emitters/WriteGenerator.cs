using PacketGen.Generators.PacketGeneration;
using PacketGen.Generators.TypeHandlers;

namespace PacketGen.Generators.Emitters;

/// <summary>
/// Generates Write method code for packet serialization.
/// </summary>
internal sealed class WriteGenerator(TypeHandlerRegistry registry) : IWriteGenerator
{
    public void Generate(GenerationContext ctx, string valueExpression, string indent)
    {
        registry.TryEmitWrite(new WriteContext(ctx), valueExpression, indent, 0);
    }
}
