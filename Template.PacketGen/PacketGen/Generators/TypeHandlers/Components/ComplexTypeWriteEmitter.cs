using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Immutable;

namespace PacketGen.Generators.TypeHandlers;

internal sealed class ComplexTypeWriteEmitter
{
    private readonly TypeHandlerRegistry _registry;

    public ComplexTypeWriteEmitter(TypeHandlerRegistry registry)
    {
        _registry = registry;
    }

    public void Emit(WriteContext ctx, INamedTypeSymbol namedType, string valueExpression, string indent, int depth)
    {
        if (ComplexTypeTypeClassifier.IsNullableValueType(namedType))
        {
            EmitNullableValueWrite(ctx, namedType, valueExpression, indent, depth);
            return;
        }

        if (namedType.TypeKind == TypeKind.Class)
        {
            EmitClassWrite(ctx, namedType, valueExpression, indent, depth);
            return;
        }

        EmitStructWrite(ctx, namedType, valueExpression, indent, depth);
    }

    private void EmitNullableValueWrite(WriteContext ctx, INamedTypeSymbol nullableType, string valueExpression, string indent, int depth)
    {
        ITypeSymbol innerType = nullableType.TypeArguments[0];
        string nullableValueExpression = $"{valueExpression}.Value";

        ctx.Shared.OutputLines.Add($"{indent}writer.Write({valueExpression}.HasValue);");
        ctx.Shared.OutputLines.Add($"{indent}if ({valueExpression}.HasValue)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, innerType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
        _registry.TryEmitWrite(new WriteContext(nested), nullableValueExpression, indent + "    ", depth + 1);

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

    private void EmitWritableMembers(WriteContext ctx, INamedTypeSymbol type, string valueExpression, string indent, int depth)
    {
        ImmutableArray<IPropertySymbol> properties = SerializablePropertySelector.Get(type);
        foreach (IPropertySymbol property in properties)
        {
            string memberValue = $"{valueExpression}.{property.Name}";
            GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, property.Type, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
            _registry.TryEmitWrite(new WriteContext(nested), memberValue, indent, depth);
        }
    }
}
