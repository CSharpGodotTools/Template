using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Immutable;

namespace PacketGen.Generators.TypeHandlers;

internal sealed class ComplexTypeReadEmitter(TypeHandlerRegistry registry)
{
    private readonly TypeHandlerRegistry _registry = registry;

    public void Emit(ReadContext ctx, INamedTypeSymbol namedType, string indent, int depth, string nameSeed)
    {
        if (ComplexTypeTypeClassifier.IsNullableValueType(namedType))
        {
            EmitNullableValueRead(ctx, namedType, indent, depth, nameSeed);
            return;
        }

        if (namedType.TypeKind == TypeKind.Class)
        {
            EmitClassRead(ctx, namedType, indent, depth, nameSeed);
            return;
        }

        EmitStructRead(ctx, namedType, indent, depth, nameSeed);
    }

    private void EmitNullableValueRead(ReadContext ctx, INamedTypeSymbol nullableType, string indent, int depth, string nameSeed)
    {
        ITypeSymbol innerType = nullableType.TypeArguments[0];
        string hasValueName = TypeHandlerNameHelper.BuildName(nameSeed, "HasValue", depth);
        string valueName = TypeHandlerNameHelper.BuildName(nameSeed, "Value", depth);
        string typeName = innerType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        TypeNamespaceHelper.AddNamespaceIfNeeded(innerType, ctx.Shared.Namespaces);

        ctx.Shared.OutputLines.Add($"{indent}bool {hasValueName} = reader.ReadBool();");
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

    private void EmitClassRead(ReadContext ctx, INamedTypeSymbol classType, string indent, int depth, string nameSeed)
    {
        string typeName = classType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string hasValueName = TypeHandlerNameHelper.BuildName(nameSeed, "HasValue", depth);
        string valueName = TypeHandlerNameHelper.BuildName(nameSeed, "Value", depth);

        TypeNamespaceHelper.AddNamespaceIfNeeded(classType, ctx.Shared.Namespaces);

        ctx.Shared.OutputLines.Add($"{indent}bool {hasValueName} = reader.ReadBool();");
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

    private void EmitStructRead(ReadContext ctx, INamedTypeSymbol structType, string indent, int depth, string nameSeed)
    {
        string typeName = structType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string valueName = TypeHandlerNameHelper.BuildName(nameSeed, "Value", depth);

        TypeNamespaceHelper.AddNamespaceIfNeeded(structType, ctx.Shared.Namespaces);

        ctx.Shared.OutputLines.Add($"{indent}{typeName} {valueName} = new {typeName}();");

        EmitReadableMembers(ctx, structType, valueName, indent, depth + 1, nameSeed);

        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = {valueName};");
    }

    private void EmitReadableMembers(ReadContext ctx, INamedTypeSymbol type, string valueExpression, string indent, int depth, string nameSeed)
    {
        ImmutableArray<IPropertySymbol> properties = SerializablePropertySelector.Get(type);
        foreach (IPropertySymbol property in properties)
        {
            string memberTarget = $"{valueExpression}.{property.Name}";
            string memberSeed = $"{nameSeed}{property.Name}";
            GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, property.Type, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
            _registry.TryEmitRead(new ReadContext(nested, memberTarget), indent, depth, memberSeed);
        }
    }

    private static string GetNullAssignment(ITypeSymbol type)
    {
        return type.NullableAnnotation == NullableAnnotation.Annotated ? "null" : "default!";
    }
}
