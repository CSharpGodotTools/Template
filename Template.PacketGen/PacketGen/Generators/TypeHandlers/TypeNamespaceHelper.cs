using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace PacketGen.Generators.TypeHandlers;

/// <summary>
/// Helper for managing namespace imports during code generation.
/// </summary>
internal static class TypeNamespaceHelper
{
    public static void AddNamespaceIfNeeded(ITypeSymbol type, HashSet<string> namespaces)
    {
        string ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(ns) || ns == "<global namespace>")
            return;

        namespaces.Add(ns);
    }
}
