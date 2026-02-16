using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;
using System.Collections.Immutable;
using System.Linq;

namespace PacketGen.Generators.TypeHandlers;

internal sealed class ComplexTypeHandler(TypeHandlerRegistry registry) : ITypeHandler
{
    private const int MaxTraversalDepth = 24;

    public bool CanHandle(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        if (IsNullableValueType(namedType))
        {
            if (namedType.TypeArguments[0] is not INamedTypeSymbol nested)
            {
                return false;
            }

            if (!nested.Locations.Any(static location => location.IsInSource))
            {
                return false;
            }

            return nested.SpecialType == SpecialType.None && nested.TypeKind == TypeKind.Struct;
        }

        if (!namedType.Locations.Any(static location => location.IsInSource))
        {
            return false;
        }

        if (namedType.SpecialType != SpecialType.None)
        {
            return false;
        }

        if (namedType.IsTupleType)
        {
            return false;
        }

        if (namedType.TypeKind == TypeKind.Class)
        {
            return !namedType.IsAbstract;
        }

        if (namedType.TypeKind == TypeKind.Struct)
        {
            return true;
        }

        return false;
    }

    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        if (depth > MaxTraversalDepth)
        {
            Logger.Err(ctx.Shared.Property, $"Exceeded max class/struct traversal depth for {ctx.Shared.Type.ToDisplayString()}.");
            ctx.Shared.OutputLines.Add($"{indent}// Exceeded max traversal depth for {ctx.Shared.Type.ToDisplayString()}.");
            return;
        }

        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        if (IsNullableValueType(namedType))
        {
            EmitNullableValueWrite(ctx, namedType, valueExpression, indent, depth);
        }
        else if (namedType.TypeKind == TypeKind.Class)
        {
            EmitClassWrite(ctx, namedType, valueExpression, indent, depth);
        }
        else
        {
            EmitStructWrite(ctx, namedType, valueExpression, indent, depth);
        }
    }

    public void EmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        if (depth > MaxTraversalDepth)
        {
            Logger.Err(ctx.Shared.Property, $"Exceeded max class/struct traversal depth for {ctx.Shared.Type.ToDisplayString()}.");
            ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = default!;");
            return;
        }

        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        string nameSeed = rootName ?? ctx.TargetExpression;
        if (IsNullableValueType(namedType))
        {
            EmitNullableValueRead(ctx, namedType, indent, depth, nameSeed);
        }
        else if (namedType.TypeKind == TypeKind.Class)
        {
            EmitClassRead(ctx, namedType, indent, depth, nameSeed);
        }
        else
        {
            EmitStructRead(ctx, namedType, indent, depth, nameSeed);
        }
    }

    private void EmitNullableValueWrite(WriteContext ctx, INamedTypeSymbol nullableType, string valueExpression, string indent, int depth)
    {
        ITypeSymbol innerType = nullableType.TypeArguments[0];
        string nullableValueExpression = $"{valueExpression}.Value";

        ctx.Shared.OutputLines.Add($"{indent}writer.Write({valueExpression}.HasValue);");
        ctx.Shared.OutputLines.Add($"{indent}if ({valueExpression}.HasValue)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, innerType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
        registry.TryEmitWrite(new WriteContext(nested), nullableValueExpression, indent + "    ", depth + 1);

        ctx.Shared.OutputLines.Add($"{indent}}}");
    }

    private void EmitClassWrite(WriteContext ctx, INamedTypeSymbol classType, string valueExpression, string indent, int depth)
    {
        ctx.Shared.OutputLines.Add($"{indent}writer.Write({valueExpression} is not null);");
        ctx.Shared.OutputLines.Add($"{indent}if ({valueExpression} is not null)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        EmitWritableMembers(ctx, classType, valueExpression, indent + "    ", depth + 1);

        ctx.Shared.OutputLines.Add($"{indent}}}");
    }

    private void EmitStructWrite(WriteContext ctx, INamedTypeSymbol structType, string valueExpression, string indent, int depth)
    {
        EmitWritableMembers(ctx, structType, valueExpression, indent, depth + 1);
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
        registry.TryEmitRead(new ReadContext(nested, valueName), indent + "    ", depth + 1, valueName);

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

    private void EmitWritableMembers(WriteContext ctx, INamedTypeSymbol type, string valueExpression, string indent, int depth)
    {
        ImmutableArray<IPropertySymbol> properties = GetSerializableProperties(type);
        foreach (IPropertySymbol property in properties)
        {
            string memberValue = $"{valueExpression}.{property.Name}";
            GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, property.Type, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
            registry.TryEmitWrite(new WriteContext(nested), memberValue, indent, depth);
        }
    }

    private void EmitReadableMembers(ReadContext ctx, INamedTypeSymbol type, string valueExpression, string indent, int depth, string nameSeed)
    {
        ImmutableArray<IPropertySymbol> properties = GetSerializableProperties(type);
        foreach (IPropertySymbol property in properties)
        {
            string memberTarget = $"{valueExpression}.{property.Name}";
            string memberSeed = $"{nameSeed}{property.Name}";
            GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, property.Type, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
            registry.TryEmitRead(new ReadContext(nested, memberTarget), indent, depth, memberSeed);
        }
    }

    private static ImmutableArray<IPropertySymbol> GetSerializableProperties(INamedTypeSymbol type)
    {
        ImmutableArray<ISymbol> members = type.GetMembers();
        ImmutableArray<IPropertySymbol>.Builder builder = ImmutableArray.CreateBuilder<IPropertySymbol>();

        foreach (ISymbol member in members)
        {
            if (member is not IPropertySymbol property)
            {
                continue;
            }

            if (!property.CanBeReferencedByName || property.IsStatic || property.Parameters.Length > 0)
            {
                continue;
            }

            if (property.GetMethod is null || property.SetMethod is null)
            {
                continue;
            }

            if (property.GetMethod.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            if (property.SetMethod.DeclaredAccessibility != Accessibility.Public || property.SetMethod.IsInitOnly)
            {
                continue;
            }

            if (property.GetAttributes().Any(static attr => attr.AttributeClass?.Name == "NetExcludeAttribute"))
            {
                continue;
            }

            builder.Add(property);
        }

        return builder.ToImmutable();
    }

    private static string GetNullAssignment(ITypeSymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return "null";
        }

        return "default!";
    }

    private static bool IsNullableValueType(INamedTypeSymbol namedType)
    {
        return namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }
}
