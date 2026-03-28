using Microsoft.CodeAnalysis;

namespace Template.OptionsGen;

internal interface IOptionsSettingsTypeLocator
{
    INamedTypeSymbol? FindTypeByName(INamespaceSymbol namespaceSymbol, string typeName);
}
