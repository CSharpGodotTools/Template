using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Immutable;
using System.Linq;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Handles serialization of user-defined classes and structs by recursively serializing their properties.
/// </summary>
internal sealed class ComplexTypeHandler(TypeHandlerRegistry registry) : ITypeHandler
{
    /// <summary>Safety ceiling on recursive type traversal to prevent infinite loops on circular references.</summary>
    private const int MaxTraversalDepth = 24;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        if (!TryGuardTraversalDepthForWrite(ctx, indent, depth))
            return;

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

    /// <inheritdoc/>
    public void EmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        if (!TryGuardTraversalDepthForRead(ctx, indent, depth))
            return;

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

    /// <summary>Emits write code for a <c>Nullable&lt;T&gt;</c> struct, writing a bool flag followed by the inner value if present.</summary>
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

    /// <summary>Emits write code for a class type, writing a not-null bool flag followed by all properties if non-null.</summary>
    private void EmitClassWrite(WriteContext ctx, INamedTypeSymbol classType, string valueExpression, string indent, int depth)
    {
        ctx.Shared.OutputLines.Add($"{indent}writer.Write({valueExpression} is not null);");
        ctx.Shared.OutputLines.Add($"{indent}if ({valueExpression} is not null)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        EmitWritableMembers(ctx, classType, valueExpression, indent + "    ", depth + 1);

        ctx.Shared.OutputLines.Add($"{indent}}}");
    }

    /// <summary>Emits write code for a struct type — structs are never null so no presence flag is needed.</summary>
    private void EmitStructWrite(WriteContext ctx, INamedTypeSymbol structType, string valueExpression, string indent, int depth)
    {
        EmitWritableMembers(ctx, structType, valueExpression, indent, depth + 1);
    }

    /// <summary>Emits read code for a <c>Nullable&lt;T&gt;</c> struct: reads a bool flag, then the inner value if true.</summary>
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

    /// <summary>Emits read code for a class type: reads a presence bool, then instantiates and populates the class if true.</summary>
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

    /// <summary>Emits read code for a struct type — instantiates a new struct and reads all properties into it.</summary>
    private void EmitStructRead(ReadContext ctx, INamedTypeSymbol structType, string indent, int depth, string nameSeed)
    {
        string typeName = structType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string valueName = TypeHandlerNameHelper.BuildName(nameSeed, "Value", depth);

        TypeNamespaceHelper.AddNamespaceIfNeeded(structType, ctx.Shared.Namespaces);

        ctx.Shared.OutputLines.Add($"{indent}{typeName} {valueName} = new {typeName}();");

        EmitReadableMembers(ctx, structType, valueName, indent, depth + 1, nameSeed);

        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = {valueName};");
    }

    /// <summary>Emits write calls for all serializable properties of the type.</summary>
    private void EmitWritableMembers(WriteContext ctx, INamedTypeSymbol type, string valueExpression, string indent, int depth)
    {
        ImmutableArray<IPropertySymbol> properties = SerializablePropertySelector.Get(type);
        foreach (IPropertySymbol property in properties)
        {
            string memberValue = $"{valueExpression}.{property.Name}";
            GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, property.Type, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
            registry.TryEmitWrite(new WriteContext(nested), memberValue, indent, depth);
        }
    }

    private void EmitReadableMembers(ReadContext ctx, INamedTypeSymbol type, string valueExpression, string indent, int depth, string nameSeed)
    {
        ImmutableArray<IPropertySymbol> properties = SerializablePropertySelector.Get(type);
        foreach (IPropertySymbol property in properties)
        {
            string memberTarget = $"{valueExpression}.{property.Name}";
            string memberSeed = $"{nameSeed}{property.Name}";
            GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, property.Type, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
            registry.TryEmitRead(new ReadContext(nested, memberTarget), indent, depth, memberSeed);
        }
    }

    private static bool TryGuardTraversalDepthForWrite(WriteContext ctx, string indent, int depth)
    {
        if (depth <= MaxTraversalDepth)
            return true;

        Logger.Err(ctx.Shared.Property, $"Exceeded max class/struct traversal depth for {ctx.Shared.Type.ToDisplayString()}.");
        ctx.Shared.OutputLines.Add($"{indent}// Exceeded max traversal depth for {ctx.Shared.Type.ToDisplayString()}.");
        return false;
    }

    private static bool TryGuardTraversalDepthForRead(ReadContext ctx, string indent, int depth)
    {
        if (depth <= MaxTraversalDepth)
            return true;

        Logger.Err(ctx.Shared.Property, $"Exceeded max class/struct traversal depth for {ctx.Shared.Type.ToDisplayString()}.");
        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = default!;");
        return false;
    }

    /// <summary>Returns <c>null</c> or <c>default!</c> depending on whether the type is already annotated nullable.</summary>
    private static string GetNullAssignment(ITypeSymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return "null";
        }

        return "default!";
    }

    /// <summary>Returns <c>true</c> if the type is <c>Nullable&lt;T&gt;</c>.</summary>
    private static bool IsNullableValueType(INamedTypeSymbol namedType)
    {
        return namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }
}
