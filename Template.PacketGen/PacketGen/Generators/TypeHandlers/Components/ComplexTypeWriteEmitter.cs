using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Immutable;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Emits write-side serialization statements for complex class/struct types.
/// </summary>
/// <param name="registry">Type-handler registry used for nested member dispatch.</param>
internal sealed class ComplexTypeWriteEmitter(TypeHandlerRegistry registry)
{
    private readonly TypeHandlerRegistry _registry = registry;

    /// <summary>
    /// Emits write statements for nullable, class, or struct complex types.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="namedType">Complex type symbol being emitted.</param>
    /// <param name="valueExpression">Expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    public void Emit(WriteContext ctx, INamedTypeSymbol namedType, string valueExpression, string indent, int depth)
    {
        // Nullable value types require HasValue guards and inner-value emission.
        if (ComplexTypeTypeClassifier.IsNullableValueType(namedType))
        {
            EmitNullableValueWrite(ctx, namedType, valueExpression, indent, depth);
            return;
        }

        // Reference types use null guards before member serialization.
        if (namedType.TypeKind == TypeKind.Class)
        {
            EmitClassWrite(ctx, namedType, valueExpression, indent, depth);
            return;
        }

        EmitStructWrite(ctx, namedType, valueExpression, indent, depth);
    }

    /// <summary>
    /// Emits write statements for <c>Nullable&lt;T&gt;</c> values.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="nullableType">Nullable type symbol.</param>
    /// <param name="valueExpression">Nullable expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    private void EmitNullableValueWrite(WriteContext ctx, INamedTypeSymbol nullableType, string valueExpression, string indent, int depth)
    {
        ITypeSymbol innerType = nullableType.TypeArguments[0];
        string nullableValueExpression = $"{valueExpression}.Value";

        ctx.Shared.OutputLines.Add($"{indent}writer.Write({valueExpression}.HasValue);");
        // Emit generated source that writes inner nullable values only when present.
        ctx.Shared.OutputLines.Add($"{indent}if ({valueExpression}.HasValue)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        GenerationContext nested = new(ctx.Shared.Compilation, ctx.Shared.Property, innerType, ctx.Shared.OutputLines, ctx.Shared.Namespaces);
        _registry.TryEmitWrite(new WriteContext(nested), nullableValueExpression, indent + "    ", depth + 1);

        ctx.Shared.OutputLines.Add($"{indent}}}");
    }

    /// <summary>
    /// Emits write statements for reference-type complex values.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="classType">Class type symbol.</param>
    /// <param name="valueExpression">Class expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    private void EmitClassWrite(WriteContext ctx, INamedTypeSymbol classType, string valueExpression, string indent, int depth)
    {
        ctx.Shared.OutputLines.Add($"{indent}writer.Write({valueExpression} is not null);");
        // Emit generated source that serializes class members only when non-null.
        ctx.Shared.OutputLines.Add($"{indent}if ({valueExpression} is not null)");
        ctx.Shared.OutputLines.Add($"{indent}{{");

        EmitWritableMembers(ctx, classType, valueExpression, indent + "    ", depth + 1);

        ctx.Shared.OutputLines.Add($"{indent}}}");
    }

    /// <summary>
    /// Emits write statements for value-type complex values.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="structType">Struct type symbol.</param>
    /// <param name="valueExpression">Struct expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    private void EmitStructWrite(WriteContext ctx, INamedTypeSymbol structType, string valueExpression, string indent, int depth)
    {
        EmitWritableMembers(ctx, structType, valueExpression, indent, depth + 1);
    }

    /// <summary>
    /// Emits write statements for serializable properties of a complex type.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="type">Complex type symbol.</param>
    /// <param name="valueExpression">Expression owning the properties.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
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
