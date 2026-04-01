using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Contract for a type-specific code emitter used by PacketGen to generate
/// <c>Write</c> and <c>Read</c> method bodies for packet properties.
/// </summary>
internal interface ITypeHandler
{
    /// <summary>
    /// Returns <c>true</c> if this handler can emit serialization code for the given type.
    /// </summary>
    /// <param name="type">Type symbol to evaluate.</param>
    /// <returns><see langword="true"/> when this handler supports the provided type.</returns>
    bool CanHandle(ITypeSymbol type);

    /// <summary>
    /// Emits code lines that write <paramref name="valueExpression"/> into the packet writer.
    /// </summary>
    /// <param name="ctx">Write context carrying shared state (output lines, namespaces, etc.).</param>
    /// <param name="valueExpression">C# expression that evaluates to the value to write.</param>
    /// <param name="indent">Indentation prefix for each emitted line.</param>
    /// <param name="depth">Nesting depth used to generate unique loop variable names.</param>
    void EmitWrite(WriteContext ctx, string valueExpression, string indent, int depth);

    /// <summary>
    /// Emits code lines that read a value from the packet reader and assign it via
    /// <see cref="ReadContext.TargetExpression"/>.
    /// </summary>
    /// <param name="ctx">Read context carrying the assignment target and shared state.</param>
    /// <param name="indent">Indentation prefix for each emitted line.</param>
    /// <param name="depth">Nesting depth used to generate unique loop variable names.</param>
    /// <param name="rootName">Optional root variable name for multi-element reads (e.g. array element).</param>
    void EmitRead(ReadContext ctx, string indent, int depth, string? rootName);
}
