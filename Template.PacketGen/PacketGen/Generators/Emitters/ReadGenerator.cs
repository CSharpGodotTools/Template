using PacketGen.Generators.PacketGeneration;
using PacketGen.Generators.TypeHandlers;

namespace PacketGen.Generators.Emitters;

/// <summary>
/// Generates Read method code for packet deserialization.
/// </summary>
internal sealed class ReadGenerator(TypeHandlerRegistry registry) : IReadGenerator
{
    public void Generate(GenerationContext ctx, string targetExpression, string indent)
    {
        registry.TryEmitRead(new ReadContext(ctx, targetExpression), indent, 0, null);
    }
}
