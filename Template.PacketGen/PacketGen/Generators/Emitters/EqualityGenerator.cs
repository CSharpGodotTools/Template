using Microsoft.CodeAnalysis;
using PacketGen.Utilities;
using System.Collections.Generic;

namespace PacketGen.Generators.Emitters;

/// <summary>
/// Generates Equals method expressions for packet properties.
/// </summary>
internal sealed class EqualityGenerator : IEqualityGenerator
{
    /// <inheritdoc/>
    public void Generate(List<EqualityLine> equalsLines, IPropertySymbol property)
    {
        string left = property.Name;
        string right = $"other.{property.Name}";
        ITypeSymbol type = property.Type;

        bool usesDeepEquality = TypeSymbolHelper.IsCollectionType(type);
        string expression = usesDeepEquality
            ? $"DeepEquals({left}, {right})"
            : $"EqualityComparer<{TypeSymbolHelper.ToTypeName(type)}>.Default.Equals({left}, {right})";

        string displayType = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string comment = $"{property.Name} ({displayType})";

        equalsLines.Add(new EqualityLine(comment, expression, usesDeepEquality));
    }
}
