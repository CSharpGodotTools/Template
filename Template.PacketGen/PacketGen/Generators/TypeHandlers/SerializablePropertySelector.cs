using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Selects properties that are safe and valid for generated read/write serialization.
/// </summary>
internal static class SerializablePropertySelector
{
    public static ImmutableArray<IPropertySymbol> Get(INamedTypeSymbol type)
    {
        ImmutableArray<ISymbol> members = type.GetMembers();
        ImmutableArray<IPropertySymbol>.Builder builder = ImmutableArray.CreateBuilder<IPropertySymbol>();

        foreach (ISymbol member in members)
        {
            if (member is not IPropertySymbol property)
                continue;

            if (!property.CanBeReferencedByName || property.IsStatic || property.Parameters.Length > 0)
                continue;

            if (property.GetMethod is null || property.SetMethod is null)
                continue;

            if (property.GetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (property.SetMethod.DeclaredAccessibility != Accessibility.Public || property.SetMethod.IsInitOnly)
                continue;

            if (property.GetAttributes().Any(static attr => attr.AttributeClass?.Name == PacketGenConstants.NetExcludeAttributeTypeName))
                continue;

            builder.Add(property);
        }

        return builder.ToImmutable();
    }
}
