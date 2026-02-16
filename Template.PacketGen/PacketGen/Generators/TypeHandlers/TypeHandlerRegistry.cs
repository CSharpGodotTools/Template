using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;
using System.Collections.Generic;

namespace PacketGen.Generators.TypeHandlers;

internal sealed class TypeHandlerRegistry
{
    private readonly List<ITypeHandler> _handlers = [];

    public TypeHandlerRegistry()
    {
    }

    public TypeHandlerRegistry(IEnumerable<ITypeHandler> handlers)
    {
        _handlers.AddRange(handlers);
    }

    public void SetHandlers(IEnumerable<ITypeHandler> handlers)
    {
        _handlers.Clear();
        _handlers.AddRange(handlers);
    }

    public bool TryEmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        ITypeSymbol type = ctx.Shared.Type;

        foreach (ITypeHandler handler in _handlers)
        {
            if (!handler.CanHandle(type))
                continue;

            handler.EmitWrite(ctx, valueExpression, indent, depth);
            return true;
        }

        Logger.Info(ctx.Shared.Property, $"Type {type.ToDisplayString()} is not supported. Implement Write and Read manually.");
        return false;
    }

    public bool TryEmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        ITypeSymbol type = ctx.Shared.Type;

        foreach (ITypeHandler handler in _handlers)
        {
            if (!handler.CanHandle(type))
                continue;

            handler.EmitRead(ctx, indent, depth, rootName);
            return true;
        }

        Logger.Info(ctx.Shared.Property, $"Type {type.ToDisplayString()} is not supported. Implement Write and Read manually.");
        return false;
    }
}
