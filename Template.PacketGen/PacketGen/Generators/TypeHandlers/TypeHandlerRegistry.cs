using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Generic;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Registry that dispatches type serialization/deserialization to appropriate handlers.
/// </summary>
internal sealed class TypeHandlerRegistry
{
    private readonly List<ITypeHandler> _handlers = [];

    /// <summary>Creates an empty registry; call <see cref="SetHandlers"/> before use.</summary>
    public TypeHandlerRegistry()
    {
    }

    /// <summary>Creates a registry pre-loaded with the given handlers.</summary>
    /// <param name="handlers">Initial handler set in dispatch order.</param>
    public TypeHandlerRegistry(IEnumerable<ITypeHandler> handlers)
    {
        _handlers.AddRange(handlers);
    }

    /// <summary>Replaces all registered handlers with the provided set (first match wins during dispatch).</summary>
    /// <param name="handlers">Handler set in dispatch order.</param>
    public void SetHandlers(IEnumerable<ITypeHandler> handlers)
    {
        _handlers.Clear();
        _handlers.AddRange(handlers);
    }

    /// <summary>
    /// Finds the first handler that can handle <c>ctx.Shared.Type</c> and calls
    /// <see cref="ITypeHandler.EmitWrite"/>. Returns <c>false</c> and emits a diagnostic if no handler matches.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="valueExpression">Expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <returns>True when a handler emitted output; otherwise false.</returns>
    public bool TryEmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        ITypeSymbol type = ctx.Shared.Type;

        foreach (ITypeHandler handler in _handlers)
        {
            // Skip handlers that do not support the current type symbol.
            if (!handler.CanHandle(type))
                continue;

            handler.EmitWrite(ctx, valueExpression, indent, depth);
            return true;
        }

        Logger.Info(ctx.Shared.Property, $"Type {type.ToDisplayString()} is not supported. Implement Write and Read manually.");
        return false;
    }

    /// <summary>
    /// Finds the first handler that can handle <c>ctx.Shared.Type</c> and calls
    /// <see cref="ITypeHandler.EmitRead"/>. Returns <c>false</c> and emits a diagnostic if no handler matches.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="rootName">Optional root variable name for nested contexts.</param>
    /// <returns>True when a handler emitted output; otherwise false.</returns>
    public bool TryEmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        ITypeSymbol type = ctx.Shared.Type;

        foreach (ITypeHandler handler in _handlers)
        {
            // Skip handlers that do not support the current type symbol.
            if (!handler.CanHandle(type))
                continue;

            handler.EmitRead(ctx, indent, depth, rootName);
            return true;
        }

        Logger.Info(ctx.Shared.Property, $"Type {type.ToDisplayString()} is not supported. Implement Write and Read manually.");
        return false;
    }
}
