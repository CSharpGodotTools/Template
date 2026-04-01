using Microsoft.CodeAnalysis;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Type-shape helpers used by complex type read/write emitters.
/// </summary>
internal static class ComplexTypeTypeClassifier
{
    /// <summary>
    /// Returns whether the provided type symbol is <c>Nullable&lt;T&gt;</c>.
    /// </summary>
    /// <param name="namedType">Type symbol to inspect.</param>
    /// <returns>True when symbol is <c>Nullable&lt;T&gt;</c>.</returns>
    public static bool IsNullableValueType(INamedTypeSymbol namedType)
    {
        return namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }
}
