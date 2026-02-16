using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;

namespace PacketGen.Generators.TypeHandlers;

internal sealed class PrimitiveTypeHandler : ITypeHandler
{
    public bool CanHandle(ITypeSymbol type) => ReadMethodSuffix.Get(type) != null;

    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        ctx.Shared.OutputLines.Add($"{indent}writer.Write({valueExpression});");
    }

    public void EmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        string? suffix = ReadMethodSuffix.Get(ctx.Shared.Type);
        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = reader.Read{suffix}();");
    }
}
