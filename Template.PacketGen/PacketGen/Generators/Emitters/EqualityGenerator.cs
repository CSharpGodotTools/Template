using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Generic;

namespace PacketGen.Generators.Emitters;

internal sealed class EqualityGenerator : IEqualityGenerator
{
    public void Generate(List<EqualityLine> equalsLines, IPropertySymbol property)
    {
        string left = property.Name;
        string right = $"other.{property.Name}";
        ITypeSymbol type = property.Type;

        bool usesDeepEquality = IsCollectionType(type);
        string expression = usesDeepEquality
            ? $"DeepEquals({left}, {right})"
            : $"EqualityComparer<{ToTypeName(type)}>.Default.Equals({left}, {right})";

        string displayType = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string comment = $"{property.Name} ({displayType})";

        equalsLines.Add(new EqualityLine(comment, expression, usesDeepEquality));
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
