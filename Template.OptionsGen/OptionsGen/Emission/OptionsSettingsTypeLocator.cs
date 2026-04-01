using Microsoft.CodeAnalysis;

namespace Template.OptionsGen;

/// <summary>
/// Searches Roslyn namespace symbols to locate the first type with a specific simple name.
/// </summary>
internal sealed class OptionsSettingsTypeLocator : IOptionsSettingsTypeLocator
{
    /// <summary>
    /// Performs a depth-first search across the provided namespace and its descendants for a type name match.
    /// </summary>
    /// <param name="namespaceSymbol">Namespace symbol to begin searching from.</param>
    /// <param name="typeName">Simple type name to locate.</param>
    /// <returns>The first matching named type symbol, or <see langword="null"/> if not found.</returns>
    public INamedTypeSymbol? FindTypeByName(INamespaceSymbol namespaceSymbol, string typeName)
    {
        // Check immediate types first to avoid deeper traversal when the match is local.
        foreach (INamedTypeSymbol type in namespaceSymbol.GetTypeMembers())
        {
            // Return immediately when a type name matches the lookup target.
            if (type.Name == typeName)
                return type;
        }

        // Recurse through child namespaces until a match is found.
        foreach (INamespaceSymbol childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            INamedTypeSymbol? found = FindTypeByName(childNamespace, typeName);

            // Bubble up the first match discovered in nested namespaces.
            if (found is not null)
                return found;
        }

        return null;
    }
}
