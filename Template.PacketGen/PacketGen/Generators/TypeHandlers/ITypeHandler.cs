using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;

namespace PacketGen.Generators.TypeHandlers;

internal interface ITypeHandler
{
    bool CanHandle(ITypeSymbol type);

    void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth);

    void EmitRead(ReadContext ctx, string indent, int depth, string? rootName);
}
