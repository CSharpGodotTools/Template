using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;

namespace PacketGen.Generators.TypeHandlers;

internal sealed class DictionaryTypeHandler(TypeHandlerRegistry registry) : ITypeHandler
{
    public bool CanHandle(ITypeSymbol type)
    {
        return type is INamedTypeSymbol named && named.IsGenericType && named.Name == "Dictionary";
    }

    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        string kvVar = $"kv{depth}";
        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        ITypeSymbol keyType = namedType.TypeArguments[0];
        ITypeSymbol valueType = namedType.TypeArguments[1];
        string keyTypeName = keyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty);
        string valueTypeName = valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty);

        ctx.Shared.Namespaces.Add("System.Collections.Generic");

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#region {valueExpression}");

        ctx.Shared.OutputLines.Add($"{indent}// {valueExpression}");
        ctx.Shared.OutputLines.Add($"{indent}writer.Write({valueExpression}.Count);");
        ctx.Shared.OutputLines.Add("");

        ctx.Shared.OutputLines.Add($"{indent}foreach (KeyValuePair<{keyTypeName}, {valueTypeName}> {kvVar} in {valueExpression})");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        GenerationContext keyCtx = new(ctx.Shared.Compilation, ctx.Shared.Property, keyType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
        registry.TryEmitWrite(new WriteContext(keyCtx), $"{kvVar}.Key", indent + "    ", depth + 1);

        ctx.Shared.OutputLines.Add("");

        GenerationContext valueCtx = new(ctx.Shared.Compilation, ctx.Shared.Property, valueType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
        registry.TryEmitWrite(new WriteContext(valueCtx), $"{kvVar}.Value", indent + "    ", depth + 1);

        ctx.Shared.OutputLines.Add($"{indent}}}");

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }

    public void EmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        ctx.Shared.Namespaces.Add("System.Collections.Generic");

        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        ITypeSymbol keyType = namedType.TypeArguments[0];
        ITypeSymbol valueType = namedType.TypeArguments[1];

        string keyTypeName = keyType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string valueTypeName = valueType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        TypeNamespaceHelper.AddNamespaceIfNeeded(keyType, ctx.Shared.Namespaces);
        TypeNamespaceHelper.AddNamespaceIfNeeded(valueType, ctx.Shared.Namespaces);

        string nameSeed = rootName ?? ctx.TargetExpression;
        string countVar = TypeHandlerNameHelper.BuildName(nameSeed, "Count", depth);
        string loopIndex = TypeHandlerNameHelper.BuildName(nameSeed, "Index", depth);
        string keyVar = TypeHandlerNameHelper.BuildName(nameSeed, "Key", depth);
        string valueVar = TypeHandlerNameHelper.BuildName(nameSeed, "Value", depth);

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#region {ctx.TargetExpression}");

        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = new Dictionary<{keyTypeName}, {valueTypeName}>();");
        ctx.Shared.OutputLines.Add($"{indent}int {countVar} = reader.ReadInt();");
        ctx.Shared.OutputLines.Add("");

        ctx.Shared.OutputLines.Add($"{indent}for (int {loopIndex} = 0; {loopIndex} < {countVar}; {loopIndex}++)");
        ctx.Shared.OutputLines.Add($"{indent}{{");
        ctx.Shared.OutputLines.Add($"{indent}    {keyTypeName} {keyVar};");
        ctx.Shared.OutputLines.Add($"{indent}    {valueTypeName} {valueVar};");
        ctx.Shared.OutputLines.Add("");

        GenerationContext keyCtx = new(ctx.Shared.Compilation, ctx.Shared.Property, keyType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
        registry.TryEmitRead(new ReadContext(keyCtx, keyVar), indent + "    ", depth + 1, keyVar);

        ctx.Shared.OutputLines.Add("");

        GenerationContext valueCtx = new(ctx.Shared.Compilation, ctx.Shared.Property, valueType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
        registry.TryEmitRead(new ReadContext(valueCtx, valueVar), indent + "    ", depth + 1, valueVar);

        ctx.Shared.OutputLines.Add("");
        ctx.Shared.OutputLines.Add($"{indent}    {ctx.TargetExpression}.Add({keyVar}, {valueVar});");
        ctx.Shared.OutputLines.Add($"{indent}}}");

        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }
}
