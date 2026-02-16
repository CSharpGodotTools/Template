using Microsoft.CodeAnalysis;
using System;
using PacketGen;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;

namespace PacketGen.Generators.TypeHandlers;

internal sealed class ArrayTypeHandler(TypeHandlerRegistry registry) : ITypeHandler
{
    public bool CanHandle(ITypeSymbol type) => type is IArrayTypeSymbol;

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

        string countVar = $"{valueExpression}.Length";
        string loopIndex = $"i{depth}";
        string elementAccess = $"{valueExpression}[{loopIndex}]";

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#region {valueExpression}");

        ctx.Shared.OutputLines.Add($"{indent}writer.Write({countVar});");
        ctx.Shared.OutputLines.Add("");

        ctx.Shared.OutputLines.Add($"{indent}for (int {loopIndex} = 0; {loopIndex} < {countVar}; {loopIndex}++)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        string? elementSuffix = ReadMethodSuffix.Get(elementType);

        if (elementSuffix != null)
        {
            ctx.Shared.OutputLines.Add($"{indent}    writer.Write({elementAccess});");
        }
        else
        {
            GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, elementType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
            registry.TryEmitWrite(new WriteContext(nested), elementAccess, indent + "    ", depth + 1);
        }

        ctx.Shared.OutputLines.Add($"{indent}}}");

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }

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
        string countVar = TypeHandlerNameHelper.BuildName(nameSeed, "Count", depth);
        string loopIndex = TypeHandlerNameHelper.BuildName(nameSeed, "Index", depth);
        string elementVar = TypeHandlerNameHelper.BuildName(nameSeed, "Element", depth);

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#region {ctx.TargetExpression}");

        string allocation = BuildJaggedArrayAllocation(arrayType, countVar);

        ctx.Shared.OutputLines.Add($"{indent}int {countVar} = reader.ReadInt();");
        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = new {allocation};");
        ctx.Shared.OutputLines.Add("");

        ctx.Shared.OutputLines.Add($"{indent}for (int {loopIndex} = 0; {loopIndex} < {countVar}; {loopIndex}++)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        string? elementSuffix = ReadMethodSuffix.Get(elementType);

        if (elementSuffix != null)
        {
            ctx.Shared.OutputLines.Add($"{indent}    {ctx.TargetExpression}[{loopIndex}] = reader.Read{elementSuffix}();");
        }
        else
        {
            ctx.Shared.OutputLines.Add($"{indent}    {elementTypeName} {elementVar};");

            GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, elementType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
            registry.TryEmitRead(new ReadContext(nested, elementVar), indent + "    ", depth + 1, elementVar);

            ctx.Shared.OutputLines.Add("");
            ctx.Shared.OutputLines.Add($"{indent}    {ctx.TargetExpression}[{loopIndex}] = {elementVar};");
        }

        ctx.Shared.OutputLines.Add($"{indent}}}");

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }

    private static string BuildJaggedArrayAllocation(IArrayTypeSymbol arrayType, string countVar)
    {
        int jaggedDepth = 0;
        ITypeSymbol element = arrayType;

        while (element is IArrayTypeSymbol array)
        {
            if (array.Rank != 1)
                throw new InvalidOperationException($"Rectangular arrays are not supported: {array.ToDisplayString()}");

            jaggedDepth++;
            element = array.ElementType;
        }

        string elementTypeName = element.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        string suffix = string.Empty;
        for (int i = 0; i < jaggedDepth - 1; i++)
            suffix += "[]";

        return $"{elementTypeName}[{countVar}]{suffix}";
    }
}

