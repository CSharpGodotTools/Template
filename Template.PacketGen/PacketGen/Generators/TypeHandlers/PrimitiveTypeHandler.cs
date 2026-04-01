using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Utilities;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Handles serialization of primitive types (int, string, bool, etc.).
/// </summary>
internal sealed class PrimitiveTypeHandler : ITypeHandler
{
    /// <summary>
    /// Returns whether the type maps to a known PacketReader primitive read method.
    /// </summary>
    /// <param name="type">Type symbol to check.</param>
    /// <returns>True when primitive handler can serialize/deserialize the type.</returns>
    public bool CanHandle(ITypeSymbol type) => ReadMethodSuffix.Get(type) != null;

    /// <summary>
    /// Emits primitive write statement for a value expression.
    /// </summary>
    /// <param name="ctx">Write generation context.</param>
    /// <param name="valueExpression">Expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    public void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth)
    {
        ctx.Shared.OutputLines.Add($"{indent}writer.Write({valueExpression});");
    }

    /// <summary>
    /// Emits primitive read statement for the target expression.
    /// </summary>
    /// <param name="ctx">Read generation context.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    /// <param name="depth">Current recursion depth.</param>
    /// <param name="rootName">Optional root variable name for nested contexts.</param>
    public void EmitRead(ReadContext ctx, string indent, int depth, string? rootName)
    {
        string? suffix = ReadMethodSuffix.Get(ctx.Shared.Type);
        ctx.Shared.OutputLines.Add($"{indent}{ctx.TargetExpression} = reader.Read{suffix}();");
    }
}
