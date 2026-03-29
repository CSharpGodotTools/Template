using Microsoft.CodeAnalysis;

namespace PacketGen.Generators.TypeHandlers;

internal static class ComplexTypeTypeClassifier
{
    public static bool IsNullableValueType(INamedTypeSymbol namedType)
    {
        return namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }
}
