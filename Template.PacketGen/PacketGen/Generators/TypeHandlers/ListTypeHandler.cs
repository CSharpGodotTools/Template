using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Handles serialization of List&lt;T&gt; collections.
/// </summary>
/// <param name="registry">Type-handler registry used for nested element dispatch.</param>
internal sealed class ListTypeHandler(TypeHandlerRegistry registry) : ITypeHandler
{
    /// <summary>
    /// Returns whether the type is a generic <c>List&lt;T&gt;</c>.
    /// </summary>
    /// <param name="type">Type symbol to check.</param>
    /// <returns>True when type is <c>List&lt;T&gt;</c>.</returns>
    public bool CanHandle(ITypeSymbol type) =>
        type is INamedTypeSymbol named && named.IsGenericType && TypeSymbolHelper.IsList(named);

    /// <summary>
    /// Emits write statements for list values.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="valueExpression">List expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        ITypeSymbol elementType = namedType.TypeArguments[0];

        CollectionLoopEmitter.EmitWriteLoop(ctx, valueExpression, $"{valueExpression}.Count", indent, depth,
            (loopIndex, bodyIndent) =>
            {
                string elementAccess = $"{valueExpression}[{loopIndex}]";
                string? elementSuffix = ReadMethodSuffix.Get(elementType);

                // Use direct writer calls for primitive or known element types.
                if (elementSuffix != null)
                {
                    ctx.Shared.OutputLines.Add($"{bodyIndent}writer.Write({elementAccess});");
                    return;
                }

                GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, elementType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
                registry.TryEmitWrite(new WriteContext(nested), elementAccess, bodyIndent, depth + 1);
            });
    }

    /// <summary>
    /// Emits read statements for list values.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="rootName">Optional root variable name for nested contexts.</param>
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

                // Use direct reader calls for primitive or known element types.
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
