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
            // Capture serializable packet properties for generation.
            if (member is IPropertySymbol property)
            {
                ImmutableArray<AttributeData> attributes = property.GetAttributes();

                // Skip properties explicitly marked to be excluded.
                if (attributes.Any(attr => attr.AttributeClass?.Name == "NetExcludeAttribute"))
                    continue;

                properties.Add(property);
            }
            // Detect custom Write/Read implementations on packet types.
            else if (member is IMethodSymbol method)
            {
                // Mark packets that already define manual serialization methods.
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
