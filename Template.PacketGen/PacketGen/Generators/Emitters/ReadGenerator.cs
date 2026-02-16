using PacketGen.Generators.PacketGeneration;
using PacketGen.Generators.TypeHandlers;

namespace PacketGen.Generators.Emitters;

internal sealed class ReadGenerator(TypeHandlerRegistry registry) : IReadGenerator
{
    public void Generate(GenerationContext ctx, string targetExpression, string indent)
    {
        registry.TryEmitRead(new ReadContext(ctx, targetExpression), indent, 0, null);
    }
}
