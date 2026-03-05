using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PacketGen.Generators.PacketGeneration;

/// <summary>
/// Analyzes packet type symbols to extract properties and metadata for code generation.
/// </summary>
internal static class PacketAnalysis
{
    /// <summary>
    /// Analyzes a packet type symbol and builds a generation model.
    /// </summary>
    /// <param name="symbol">The packet type symbol to analyze.</param>
    /// <returns>A model containing all properties and metadata needed for generation.</returns>
    public static PacketGenerationModel Analyze(INamedTypeSymbol symbol)
    {
        ImmutableArray<ISymbol> members = symbol.GetMembers();
        List<IPropertySymbol> properties = [];
        bool hasWriteReadMethods = false;

        foreach (ISymbol member in members)
        {
            if (member is IPropertySymbol property)
            {
                ImmutableArray<AttributeData> attributes = property.GetAttributes();

                if (attributes.Any(attr => attr.AttributeClass?.Name == "NetExcludeAttribute"))
                    continue;

                properties.Add(property);
            }
            else if (member is IMethodSymbol method)
            {
                if (method.Name == "Write" || method.Name == "Read")
                    hasWriteReadMethods = true;
            }
        }

        return new PacketGenerationModel(
            symbol,
            properties.ToImmutableArray(),
            hasWriteReadMethods,
            symbol.ContainingNamespace.ToDisplayString(),
            symbol.Name
        );
    }
}
