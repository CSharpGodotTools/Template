using Microsoft.CodeAnalysis;

namespace Template.OptionsGen;

internal sealed class OptionsSettingsTypeLocator : IOptionsSettingsTypeLocator
{
    public INamedTypeSymbol? FindTypeByName(INamespaceSymbol namespaceSymbol, string typeName)
    {
        foreach (INamedTypeSymbol type in namespaceSymbol.GetTypeMembers())
        {
            if (type.Name == typeName)
                return type;
        }

        foreach (INamespaceSymbol childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            INamedTypeSymbol? found = FindTypeByName(childNamespace, typeName);
            if (found is not null)
                return found;
        }

        return null;
    }
}
