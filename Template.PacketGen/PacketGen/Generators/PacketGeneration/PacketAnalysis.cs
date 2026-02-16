using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;

namespace PacketGen.Generators.PacketGeneration;

internal static class PacketAnalysis
{
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
