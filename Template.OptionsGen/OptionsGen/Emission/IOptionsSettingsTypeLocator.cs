using Microsoft.CodeAnalysis;

namespace Template.OptionsGen;

/// <summary>
/// Resolves target type symbols by name within a Roslyn namespace tree.
/// </summary>
internal interface IOptionsSettingsTypeLocator
{
    /// <summary>
    /// Finds a named type by simple name, starting from a namespace symbol and its descendants.
    /// </summary>
    /// <param name="namespaceSymbol">Namespace root to search.</param>
    /// <param name="typeName">Simple type name to locate.</param>
    /// <returns>The matching type symbol, or <see langword="null"/> when no match exists.</returns>
    INamedTypeSymbol? FindTypeByName(INamespaceSymbol namespaceSymbol, string typeName);
}
