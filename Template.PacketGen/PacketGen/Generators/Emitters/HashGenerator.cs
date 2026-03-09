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

    /// <inheritdoc/>
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

    private static string BuildDefaultHash(ITypeSymbol type, string typeName, IPropertySymbol property)
    {
        if (type.IsReferenceType || property.NullableAnnotation == NullableAnnotation.Annotated)
            return $"({property.Name} != null ? EqualityComparer<{typeName}>.Default.GetHashCode({property.Name}) : 0)";

        return $"EqualityComparer<{typeName}>.Default.GetHashCode({property.Name})";
    }
}
