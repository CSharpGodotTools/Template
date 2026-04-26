using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Immutable;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Emits read-side deserialization statements for complex class/struct types.
/// </summary>
/// <param name="registry">Type-handler registry used for nested member dispatch.</param>
internal sealed class ComplexTypeReadEmitter(TypeHandlerRegistry registry)
{
    private readonly TypeHandlerRegistry _registry = registry;

    /// <summary>
    /// Emits read statements for nullable, class, or struct complex types.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="namedType">Complex type symbol being emitted.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="nameSeed">Name seed for generated locals.</param>
    public void Emit(ReadContext ctx, INamedTypeSymbol namedType, string indent, int depth, string nameSeed)
    {
        // Nullable value types require generated HasValue branch handling.
        if (ComplexTypeTypeClassifier.IsNullableValueType(namedType))
        {
            EmitNullableValueRead(ctx, namedType, indent, depth, nameSeed);
            return;
        }

        // Reference types require generated null-presence branch handling.
        if (namedType.TypeKind == TypeKind.Class)
        {
            EmitClassRead(ctx, namedType, indent, depth, nameSeed);
            return;
        }

        EmitStructRead(ctx, namedType, indent, depth, nameSeed);
    }

    /// <summary>
    /// Emits read statements for <c>Nullable&lt;T&gt;</c> values.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="nullableType">Nullable type symbol.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="nameSeed">Name seed for generated locals.</param>
    private void EmitNullableValueRead(ReadContext ctx, INamedTypeSymbol nullableType, string indent, int depth, string nameSeed)
    {
        ITypeSymbol innerType = nullableType.TypeArguments[0];
        string hasValueName = TypeHandlerNameHelper.BuildName(nameSeed, "HasValue", depth);
        string valueName = TypeHandlerNameHelper.BuildName(nameSeed, "Value", depth);
        string typeName = innerType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        TypeNamespaceHelper.AddNamespaceIfNeeded(innerType, ctx.Shared.Namespaces);

        ctx.Shared.OutputLines.Add($"{indent}bool {hasValueName} = reader.ReadBool();");
        // Emit generated source for nullable value presence checks.
        ctx.Shared.OutputLines.Add($"{indent}if ({hasValueName})");
        ctx.Shared.OutputLines.Add($"{indent}{{");
        ctx.Shared.OutputLines.Add($"{indent}    {typeName} {valueName};");

        GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, innerType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
        _registry.TryEmitRead(new ReadContext(nested, valueName), indent + "    ", depth + 1, valueName);

        ctx.Shared.OutputLines.Add($"{indent}    {ctx.TargetExpression} = {valueName};");
        ctx.Shared.OutputLines.Add($"{indent}}}");
        ctx.Shared.OutputLines.Add($"{indent}else");
        ctx.Shared.OutputLines.Add($"{indent}{{");
        ctx.Shared.OutputLines.Add($"{indent}    {ctx.TargetExpression} = null;");
        ctx.Shared.OutputLines.Add($"{indent}}}");
    }

    /// <summary>
    /// Emits read statements for reference-type complex values.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="classType">Class type symbol.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="nameSeed">Name seed for generated locals.</param>
    private void EmitClassRead(ReadContext ctx, INamedTypeSymbol classType, string indent, int depth, string nameSeed)
    {
        string typeName = classType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string hasValueName = TypeHandlerNameHelper.BuildName(nameSeed, "HasValue", depth);
        string valueName = TypeHandlerNameHelper.BuildName(nameSeed, "Value", depth);

        TypeNamespaceHelper.AddNamespaceIfNeeded(classType, ctx.Shared.Namespaces);

        ctx.Shared.OutputLines.Add($"{indent}bool {hasValueName} = reader.ReadBool();");
        // Emit generated source for class null-presence checks.
        ctx.Shared.OutputLines.Add($"{indent}if ({hasValueName})");
        ctx.Shared.OutputLines.Add($"{indent}{{");
        ctx.Shared.OutputLines.Add($"{indent}    {typeName} {valueName} = new {typeName}();");

        EmitReadableMembers(ctx, classType, valueName, indent + "    ", depth + 1, nameSeed);

        ctx.Shared.OutputLines.Add($"{indent}    {ctx.TargetExpression} = {valueName};");
        ctx.Shared.OutputLines.Add($"{indent}}}");
        ctx.Shared.OutputLines.Add($"{indent}else");
        ctx.Shared.OutputLines.Add($"{indent}{{");
        ctx.Shared.OutputLines.Add($"{indent}    {ctx.TargetExpression} = {GetNullAssignment(ctx.Shared.Type)};");
        ctx.Shared.OutputLines.Add($"{indent}}}");
    }

    /// <summary>
    /// Emits read statements for value-type complex values.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="structType">Struct type symbol.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="nameSeed">Name seed for generated locals.</param>
    private void EmitStructRead(ReadContext ctx, INamedTypeSymbol structType, string indent, int depth, string nameSeed)
    {
        string typeName = structType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string valueName = TypeHandlerNameHelper.BuildName(nameSeed, "Value", depth);

        TypeNamespaceHelper.AddNamespaceIfNeeded(structType, ctx.Shared.Namespaces);

        ctx.Shared.OutputLines.Add($"{indent}{typeName} {valueName} = new {typeName}();");

        EmitReadableMembers(ctx, structType, valueName, indent, depth + 1, nameSeed);

        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = {valueName};");
    }

    /// <summary>
    /// Emits read statements for serializable properties of a complex type.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="type">Complex type symbol.</param>
    /// <param name="valueExpression">Expression owning the properties.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="nameSeed">Name seed for generated locals.</param>
    private void EmitReadableMembers(ReadContext ctx, INamedTypeSymbol type, string valueExpression, string indent, int depth, string nameSeed)
    {
        foreach (IPropertySymbol property in SerializablePropertySelector.Get(type))
        {
            string memberTarget = $"{valueExpression}.{property.Name}";
            string memberSeed = nameSeed + property.Name;
            GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, property.Type, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
            _registry.TryEmitRead(new ReadContext(nested, memberTarget), indent, depth, memberSeed);
        }
    }

    /// <summary>
    /// Returns null/default assignment expression based on target nullable annotation.
    /// </summary>
    /// <param name="type">Target type symbol.</param>
    /// <returns>Assignment expression text.</returns>
    private static string GetNullAssignment(ITypeSymbol type)
    {
        return type.NullableAnnotation == NullableAnnotation.Annotated ? "null" : "default!";
    }
}
