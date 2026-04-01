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

    /// <summary>
    /// Creates a complex-type handler with default read/write emitters.
    /// </summary>
    /// <param name="registry">Type-handler registry used for nested member dispatch.</param>
    public ComplexTypeHandler(TypeHandlerRegistry registry)
        : this(new ComplexTypeWriteEmitter(registry), new ComplexTypeReadEmitter(registry))
    {
    }

    /// <summary>
    /// Creates a complex-type handler with explicit read/write emitters.
    /// </summary>
    /// <param name="writeEmitter">Emitter for write-side generation.</param>
    /// <param name="readEmitter">Emitter for read-side generation.</param>
    internal ComplexTypeHandler(ComplexTypeWriteEmitter writeEmitter, ComplexTypeReadEmitter readEmitter)
    {
        _writeEmitter = writeEmitter;
        _readEmitter = readEmitter;
    }

    /// <summary>
    /// Returns whether the type is a supported user-defined class or struct.
    /// </summary>
    /// <param name="type">Type symbol to check.</param>
    /// <returns>True when complex-type serialization is supported.</returns>
    public bool CanHandle(ITypeSymbol type)
    {
        // Complex handling applies only to named type symbols.
        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        // Handle nullable value-types by validating their wrapped struct type.
        if (ComplexTypeTypeClassifier.IsNullableValueType(namedType))
        {
            // Reject malformed nullable symbols without a named wrapped type.
            if (namedType.TypeArguments[0] is not INamedTypeSymbol nested)
            {
                return false;
            }

            // Restrict support to user-authored source types.
            if (!nested.Locations.Any(static location => location.IsInSource))
            {
                return false;
            }

            return nested.SpecialType == SpecialType.None && nested.TypeKind == TypeKind.Struct;
        }

        // Restrict support to user-authored source types.
        if (!namedType.Locations.Any(static location => location.IsInSource))
        {
            return false;
        }

        // Exclude intrinsic special types from complex-type handling.
        if (namedType.SpecialType != SpecialType.None)
        {
            return false;
        }

        // Exclude tuple syntax because it has dedicated handling paths.
        if (namedType.IsTupleType)
        {
            return false;
        }

        // Allow non-abstract classes.
        if (namedType.TypeKind == TypeKind.Class)
        {
            return !namedType.IsAbstract;
        }

        // Allow structs.
        if (namedType.TypeKind == TypeKind.Struct)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Emits write statements for user-defined class/struct values.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="valueExpression">Expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        // Abort write emission when traversal depth protection triggers.
        if (!TryGuardTraversalDepthForWrite(ctx, indent, depth))
            return;

        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        _writeEmitter.Emit(ctx, namedType, valueExpression, indent, depth);
    }

    /// <summary>
    /// Emits read statements for user-defined class/struct values.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="rootName">Optional root variable name for nested contexts.</param>
    public void EmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        // Abort read emission when traversal depth protection triggers.
        if (!TryGuardTraversalDepthForRead(ctx, indent, depth))
            return;

        INamedTypeSymbol namedType = (INamedTypeSymbol)ctx.Shared.Type;
        string nameSeed = rootName ?? ctx.TargetExpression;
        _readEmitter.Emit(ctx, namedType, indent, depth, nameSeed);
    }

    /// <summary>
    /// Guards write-side traversal depth and emits diagnostics when depth is exceeded.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <returns>True when generation may continue; otherwise false.</returns>
    private static bool TryGuardTraversalDepthForWrite(WriteContext ctx, string indent, int depth)
    {
        // Continue while recursion depth remains within configured safety limit.
        if (depth <= MaxTraversalDepth)
            return true;

        Logger.Err(ctx.Shared.Property, $"Exceeded max class/struct traversal depth for {ctx.Shared.Type.ToDisplayString()}.");
        ctx.Shared.OutputLines.Add($"{indent}// Exceeded max traversal depth for {ctx.Shared.Type.ToDisplayString()}.");
        return false;
    }

    /// <summary>
    /// Guards read-side traversal depth and emits diagnostics when depth is exceeded.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <returns>True when generation may continue; otherwise false.</returns>
    private static bool TryGuardTraversalDepthForRead(ReadContext ctx, string indent, int depth)
    {
        // Continue while recursion depth remains within configured safety limit.
        if (depth <= MaxTraversalDepth)
            return true;

        Logger.Err(ctx.Shared.Property, $"Exceeded max class/struct traversal depth for {ctx.Shared.Type.ToDisplayString()}.");
        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = default!;");
        return false;
    }

}
