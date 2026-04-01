using PacketGen.Generators.PacketGeneration;
using PacketGen.Generators.TypeHandlers;

namespace PacketGen.Generators.Emitters;

/// <summary>
/// Generates Write method code for packet serialization.
/// </summary>
/// <param name="registry">Type-handler registry used for recursive dispatch.</param>
internal sealed class WriteGenerator(TypeHandlerRegistry registry) : IWriteGenerator
{
    /// <summary>
    /// Emits write statements for a value expression using registered type handlers.
    /// </summary>
    /// <param name="ctx">Generation context for current property/type.</param>
    /// <param name="valueExpression">Expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    public void Generate(GenerationContext ctx, string valueExpression, string indent)
    {
        registry.TryEmitWrite(new WriteContext(ctx), valueExpression, indent, 0);
    }
}
