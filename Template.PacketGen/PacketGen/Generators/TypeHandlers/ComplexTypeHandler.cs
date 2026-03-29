using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Linq;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Handles serialization of user-defined classes and structs by recursively serializing their properties.
/// </summary>
internal sealed class ComplexTypeHandler : ITypeHandler
{
    /// <summary>Safety ceiling on recursive type traversal to prevent infinite loops on circular references.</summary>
    private const int MaxTraversalDepth = 24;
    private readonly ComplexTypeWriteEmitter _writeEmitter;
    private readonly ComplexTypeReadEmitter _readEmitter;

    public ComplexTypeHandler(TypeHandlerRegistry registry)
        : this(new ComplexTypeWriteEmitter(registry), new ComplexTypeReadEmitter(registry))
    {
    }

    internal ComplexTypeHandler(ComplexTypeWriteEmitter writeEmitter, ComplexTypeReadEmitter readEmitter)
    {
        _writeEmitter = writeEmitter;
        _readEmitter = readEmitter;
    }

    /// <inheritdoc/>
    public bool CanHandle(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        if (ComplexTypeTypeClassifier.IsNullableValueType(namedType))
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
        _writeEmitter.Emit(ctx, namedType, valueExpression, indent, depth);
    }

    /// <inheritdoc/>
    public void EmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        if (!TryGuardTraversalDepthForRead(ctx, indent, depth))
            return;

        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        string nameSeed = rootName ?? ctx.TargetExpression;
        _readEmitter.Emit(ctx, namedType, indent, depth, nameSeed);
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

}
