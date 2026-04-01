using Microsoft.CodeAnalysis;
using PacketGen.Utilities;
using System.Collections.Generic;

namespace PacketGen.Generators.Emitters;

/// <summary>
/// Generates GetHashCode expressions for packet properties.
/// </summary>
internal sealed class HashGenerator : IHashGenerator
{
    private const int HashMultiplier = 397;

    /// <summary>
    /// Appends hash-code generation lines for a packet property.
    /// </summary>
    /// <param name="hashLines">Destination collection of hash lines.</param>
    /// <param name="property">Property being emitted.</param>
    /// <param name="namespaces">Namespace set for generated source (currently unused).</param>
    public void Generate(List<HashLine> hashLines, IPropertySymbol property, HashSet<string> namespaces)
    {
        ITypeSymbol type = property.Type;
        bool usesDeepHash = TypeSymbolHelper.IsCollectionType(type);
        string typeName = TypeSymbolHelper.ToTypeName(type);

        string propHash = usesDeepHash
            ? $"DeepHash({property.Name})"
            : BuildDefaultHash(type, typeName, property);

        hashLines.Add(new HashLine($"hash = (hash * {HashMultiplier}) ^ {propHash};", usesDeepHash));
    }

    /// <summary>
    /// Builds default hash expression for non-collection property types.
    /// </summary>
    /// <param name="type">Property type symbol.</param>
    /// <param name="typeName">Generated type-name text for equality comparer.</param>
    /// <param name="property">Property being emitted.</param>
    /// <returns>Hash expression string.</returns>
    private static string BuildDefaultHash(ITypeSymbol type, string typeName, IPropertySymbol property)
    {
        // Null-protect hash generation for reference or nullable property types.
        if (type.IsReferenceType || property.NullableAnnotation == NullableAnnotation.Annotated)
            return $"({property.Name} != null ? EqualityComparer<{typeName}>.Default.GetHashCode({property.Name}) : 0)";

        return $"EqualityComparer<{typeName}>.Default.GetHashCode({property.Name})";
    }
}
