using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;
using System;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Handles serialization of arrays including jagged arrays.
/// </summary>
internal sealed class ArrayTypeHandler(TypeHandlerRegistry registry) : ITypeHandler
{
    /// <inheritdoc/>
    public bool CanHandle(ITypeSymbol type) => type is IArrayTypeSymbol;

    /// <inheritdoc/>
    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        IArrayTypeSymbol arrayType = (IArrayTypeSymbol)ctx.Shared.Type;
        if (arrayType.Rank != 1)
        {
            Logger.Err(ctx.Shared.Property, $"Rectangular arrays are not supported: {arrayType.ToDisplayString()}");
            ctx.Shared.OutputLines.Add($"{indent}// Unsupported rectangular array type: {arrayType.ToDisplayString()}");
            return;
        }

        ITypeSymbol elementType = arrayType.ElementType;

        CollectionLoopEmitter.EmitWriteLoop(ctx, valueExpression, $"{valueExpression}.Length", indent, depth,
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
        IArrayTypeSymbol arrayType = (IArrayTypeSymbol)ctx.Shared.Type;
        if (arrayType.Rank != 1)
        {
            Logger.Err(ctx.Shared.Property, $"Rectangular arrays are not supported: {arrayType.ToDisplayString()}");
            ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = default!;");
            return;
        }

        ITypeSymbol elementType = arrayType.ElementType;

        string elementTypeName = elementType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        TypeNamespaceHelper.AddNamespaceIfNeeded(elementType, ctx.Shared.Namespaces);

        string nameSeed = rootName ?? ctx.TargetExpression;
        (string countVar, string loopIndex, string elementVar) = CollectionLoopEmitter.BuildReadLoopNames(nameSeed, depth);

        string allocation = BuildJaggedArrayAllocation(arrayType, countVar);

        ctx.Shared.OutputLines.Add($"{indent}int {countVar} = reader.ReadInt();");
        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = new {allocation};");
        ctx.Shared.OutputLines.Add("");

        CollectionLoopEmitter.EmitReadLoop(ctx, indent, depth, loopIndex, countVar,
            (indexName, bodyIndent) =>
            {
                string? elementSuffix = ReadMethodSuffix.Get(elementType);

                if (elementSuffix != null)
                {
                    ctx.Shared.OutputLines.Add($"{bodyIndent}{ctx.TargetExpression}[{indexName}] = reader.Read{elementSuffix}();");
                    return;
                }

                ctx.Shared.OutputLines.Add($"{bodyIndent}{elementTypeName} {elementVar};");

                GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, elementType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
                registry.TryEmitRead(new ReadContext(nested, elementVar), bodyIndent, depth + 1, elementVar);

                ctx.Shared.OutputLines.Add("");
                ctx.Shared.OutputLines.Add($"{bodyIndent}{ctx.TargetExpression}[{indexName}] = {elementVar};");
            });
    }

    /// <summary>
    /// Builds a jagged-array allocation expression such as <c>int[count][]</c> or
    /// <c>int[count][][]</c> by walking the element-type chain to determine nesting depth.
    /// </summary>
    private static string BuildJaggedArrayAllocation(IArrayTypeSymbol arrayType, string countVar)
    {
        int jaggedDepth = 0;
        ITypeSymbol element = arrayType;

        // Walk down the element-type chain to count how many jagged dimensions exist
        while (element is IArrayTypeSymbol array)
        {
            if (array.Rank != 1)
                throw new InvalidOperationException($"Rectangular arrays are not supported: {array.ToDisplayString()}");

            jaggedDepth++;
            element = array.ElementType;
        }

        string elementTypeName = element.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        // Append empty bracket pairs for each additional jagged dimension beyond the outermost
        string suffix = string.Empty;
        for (int i = 0; i < jaggedDepth - 1; i++)
            suffix += "[]";

        return $"{elementTypeName}[{countVar}]{suffix}";
    }
}

