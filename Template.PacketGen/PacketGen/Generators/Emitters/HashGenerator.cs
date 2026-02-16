using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Generic;

namespace PacketGen.Generators.Emitters;

internal sealed class HashGenerator : IHashGenerator
{
    public void Generate(List<HashLine> hashLines, IPropertySymbol property, HashSet<string> namespaces)
    {
        ITypeSymbol type = property.Type;
        bool usesDeepHash = IsCollectionType(type);
        string typeName = ToTypeName(type);

        string propHash = usesDeepHash
            ? $"DeepHash({property.Name})"
            : BuildDefaultHash(type, typeName, property);

        hashLines.Add(new HashLine($"hash = (hash * 397) ^ {propHash};", usesDeepHash));
    }

    private static string BuildDefaultHash(ITypeSymbol type, string typeName, IPropertySymbol property)
    {
        if (type.IsReferenceType || property.NullableAnnotation == NullableAnnotation.Annotated)
            return $"({property.Name} != null ? EqualityComparer<{typeName}>.Default.GetHashCode({property.Name}) : 0)";

        return $"EqualityComparer<{typeName}>.Default.GetHashCode({property.Name})";
    }

    private static bool IsCollectionType(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol)
            return true;

        if (type is INamedTypeSymbol named && named.IsGenericType)
            return IsList(named) || IsDictionary(named);

        return false;
    }

    private static bool IsList(INamedTypeSymbol type)
    {
        string definition = type.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return definition == "global::System.Collections.Generic.List<T>";
    }

    private static bool IsDictionary(INamedTypeSymbol type)
    {
        string definition = type.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return definition == "global::System.Collections.Generic.Dictionary<TKey, TValue>";
    }

    private static string ToTypeName(ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty);
    }
}
