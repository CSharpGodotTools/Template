using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Helper for managing namespace imports during code generation.
/// </summary>
internal static class TypeNamespaceHelper
{
    /// <summary>
    /// Adds the namespace of <paramref name="type"/> to <paramref name="namespaces"/> if it is non-empty
    /// and not the global namespace.
    /// </summary>
    /// <param name="type">Type whose namespace may be required by generated source.</param>
    /// <param name="namespaces">Namespace set to update.</param>
    public static void AddNamespaceIfNeeded(ITypeSymbol type, HashSet<string> namespaces)
    {
        string ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        // Skip empty or global namespaces because no using directive is required.
        if (string.IsNullOrWhiteSpace(ns) || ns == "<global namespace>")
            return;

        namespaces.Add(ns);
    }
}
