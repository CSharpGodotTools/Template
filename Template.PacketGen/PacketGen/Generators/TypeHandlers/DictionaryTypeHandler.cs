using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Handles serialization of Dictionary&lt;TKey, TValue&gt; collections.
/// </summary>
/// <param name="registry">Type-handler registry used for nested key/value dispatch.</param>
internal sealed class DictionaryTypeHandler(TypeHandlerRegistry registry) : ITypeHandler
{
    /// <summary>
    /// Returns whether the type is a generic <c>Dictionary&lt;TKey, TValue&gt;</c>.
    /// </summary>
    /// <param name="type">Type symbol to check.</param>
    /// <returns>True when type is <c>Dictionary&lt;TKey, TValue&gt;</c>.</returns>
    public bool CanHandle(ITypeSymbol type) =>
        type is INamedTypeSymbol named && named.IsGenericType && TypeSymbolHelper.IsDictionary(named);

    /// <summary>
    /// Emits write statements for dictionary values.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="valueExpression">Dictionary expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        // Unique loop variable name per nesting depth prevents shadowing in nested collections
        string kvVar = $"kv{depth}";
        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        ITypeSymbol keyType = namedType.TypeArguments[0];
        ITypeSymbol valueType = namedType.TypeArguments[1];
        string keyTypeName = TypeSymbolHelper.ToTypeName(keyType);
        string valueTypeName = TypeSymbolHelper.ToTypeName(valueType);

        ctx.Shared.Namespaces.Add("System.Collections.Generic");

        // Wrap only top-level dictionary writes with a region marker.
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

        // Close the region only for top-level dictionary writes.
        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }

    /// <summary>
    /// Emits read statements for dictionary values.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="rootName">Optional root variable name for nested contexts.</param>
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

        // Wrap only top-level dictionary reads with a region marker.
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

        // Close the region only for top-level dictionary reads.
        if (depth == 0)
            ctx.Shared.OutputLines.Add($"{indent}#endregion");
    }
}
