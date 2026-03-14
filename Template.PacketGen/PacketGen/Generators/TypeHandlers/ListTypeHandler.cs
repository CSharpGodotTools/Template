using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Handles serialization of List&lt;T&gt; collections.
/// </summary>
internal sealed class ListTypeHandler(TypeHandlerRegistry registry) : ITypeHandler
{
    /// <inheritdoc/>
    public bool CanHandle(ITypeSymbol type) =>
        type is INamedTypeSymbol named && named.IsGenericType && TypeSymbolHelper.IsList(named);

    /// <inheritdoc/>
    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        ITypeSymbol elementType = namedType.TypeArguments[0];

        CollectionLoopEmitter.EmitWriteLoop(ctx, valueExpression, $"{valueExpression}.Count", indent, depth,
            (loopIndex, bodyIndent) =>
            {
                string elementAccess = $"{valueExpression}[{loopIndex}]";
                string? elementSuffix = ReadMethodSuffix.Get(elementType);

                if (elementSuffix != null)
                {
                    ctx.Shared.OutputLines.Add($"{bodyIndent}writer.Write({elementAccess});");
                    return;
                }

                GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, elementType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
                registry.TryEmitWrite(new WriteContext(nested), elementAccess, bodyIndent, depth + 1);
            });
    }

    /// <inheritdoc/>
    public void EmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        ctx.Shared.Namespaces.Add("System.Collections.Generic");

        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        ITypeSymbol elementType = namedType.TypeArguments[0];

        string elementTypeName = elementType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        TypeNamespaceHelper.AddNamespaceIfNeeded(elementType, ctx.Shared.Namespaces);

        string nameSeed = rootName ?? ctx.TargetExpression;
        (string countVar, string loopIndex, string elementVar) = CollectionLoopEmitter.BuildReadLoopNames(nameSeed, depth);

        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = new List<{elementTypeName}>();");
        ctx.Shared.OutputLines.Add($"{indent}int {countVar} = reader.ReadInt();");
        ctx.Shared.OutputLines.Add("");

        CollectionLoopEmitter.EmitReadLoop(ctx, indent, depth, loopIndex, countVar,
            (_, bodyIndent) =>
            {
                string? elementSuffix = ReadMethodSuffix.Get(elementType);

                if (elementSuffix != null)
                {
                    ctx.Shared.OutputLines.Add($"{bodyIndent}{ctx.TargetExpression}.Add(reader.Read{elementSuffix}());");
                    return;
                }

                ctx.Shared.OutputLines.Add($"{bodyIndent}{elementTypeName} {elementVar};");

                GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, elementType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
                registry.TryEmitRead(new ReadContext(nested, elementVar), bodyIndent, depth + 1, elementVar);

                ctx.Shared.OutputLines.Add("");
                ctx.Shared.OutputLines.Add($"{bodyIndent}{ctx.TargetExpression}.Add({elementVar});");
            });
    }
}
