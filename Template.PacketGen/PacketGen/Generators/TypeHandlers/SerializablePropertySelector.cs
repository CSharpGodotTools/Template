using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Selects properties that are safe and valid for generated read/write serialization.
/// </summary>
internal static class SerializablePropertySelector
{
    /// <summary>
    /// Returns serializable public get/set properties that are eligible for generated packet IO.
    /// </summary>
    /// <param name="type">Type whose properties should be inspected.</param>
    /// <returns>Filtered property list for generation.</returns>
    public static ImmutableArray<IPropertySymbol> Get(INamedTypeSymbol type)
    {
        ImmutableArray<ISymbol> members = type.GetMembers();
        ImmutableArray<IPropertySymbol>.Builder builder = ImmutableArray.CreateBuilder<IPropertySymbol>();

        foreach (ISymbol member in members)
        {
            // Ignore non-property members during serializable property discovery.
            if (member is not IPropertySymbol property)
                continue;

            // Exclude indexers, static members, and compiler-only references.
            if (!property.CanBeReferencedByName || property.IsStatic || property.Parameters.Length > 0)
                continue;

            // Serialization requires both readable and writable accessors.
            if (property.GetMethod is null || property.SetMethod is null)
                continue;

            // Generated readers require a publicly accessible getter.
            if (property.GetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Generated writers require a public non-init setter.
            if (property.SetMethod.DeclaredAccessibility != Accessibility.Public || property.SetMethod.IsInitOnly)
                continue;

            // Respect explicit opt-out attributes on packet properties.
            if (property.GetAttributes().Any(static attr => attr.AttributeClass?.Name == PacketGenConstants.NetExcludeAttributeTypeName))
                continue;

            builder.Add(property);
        }

        return builder.ToImmutable();
    }
}
