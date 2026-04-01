using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;
using System;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Handles serialization of arrays including jagged arrays.
/// </summary>
/// <param name="registry">Type-handler registry used for nested element dispatch.</param>
internal sealed class ArrayTypeHandler(TypeHandlerRegistry registry) : ITypeHandler
{
    /// <summary>
    /// Returns whether the type is an array handled by this serializer.
    /// </summary>
    /// <param name="type">Type symbol to check.</param>
    /// <returns>True when type is an array.</returns>
    public bool CanHandle(ITypeSymbol type) => type is IArrayTypeSymbol;

    /// <summary>
    /// Emits write statements for one-dimensional and jagged arrays.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="valueExpression">Array expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        IArrayTypeSymbol arrayType = (IArrayTypeSymbol)ctx.Shared.Type;

        // Reject rectangular arrays because only jagged and 1D arrays are supported.
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

                // Use direct writer methods for primitive/known element types.
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
    /// Emits read statements for one-dimensional and jagged arrays.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="rootName">Optional root variable name for nested contexts.</param>
    public void EmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        IArrayTypeSymbol arrayType = (IArrayTypeSymbol)ctx.Shared.Type;

        // Reject rectangular arrays because generated read logic expects 1D/jagged arrays.
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

                // Use direct reader methods when the element type has a known suffix.
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
    /// <param name="arrayType">Array type being allocated.</param>
    /// <param name="countVar">Variable name containing outer array length.</param>
    /// <returns>Allocation expression suffix for generated source.</returns>
    private static string BuildJaggedArrayAllocation(IArrayTypeSymbol arrayType, string countVar)
    {
        int jaggedDepth = 0;
        ITypeSymbol element = arrayType;

        // Walk down the element-type chain to count how many jagged dimensions exist
        while (element is IArrayTypeSymbol array)
        {
            // Abort if a rectangular level appears in a supposed jagged chain.
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
