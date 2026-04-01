using PacketGen.Generators.PacketGeneration;
using PacketGen.Generators.TypeHandlers;

namespace PacketGen.Generators.Emitters;

/// <summary>
/// Generates Read method code for packet deserialization.
/// </summary>
/// <param name="registry">Type-handler registry used for recursive dispatch.</param>
internal sealed class ReadGenerator(TypeHandlerRegistry registry) : IReadGenerator
{
    /// <summary>
    /// Emits read statements for a target expression using registered type handlers.
    /// </summary>
    /// <param name="ctx">Generation context for current property/type.</param>
    /// <param name="targetExpression">Expression receiving the deserialized value.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    public void Generate(GenerationContext ctx, string targetExpression, string indent)
    {
        registry.TryEmitRead(new ReadContext(ctx, targetExpression), indent, 0, null);
    }
}
