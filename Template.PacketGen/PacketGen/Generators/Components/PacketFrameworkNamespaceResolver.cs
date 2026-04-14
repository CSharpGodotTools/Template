using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace PacketGen.Generators;

/// <summary>
/// Resolves the packet framework namespace from semantic symbols or syntax fallbacks.
/// </summary>
internal sealed class PacketFrameworkNamespaceResolver
{
    /// <summary>
    /// Resolves the namespace where packet base classes live for a packet symbol.
    /// </summary>
    /// <param name="symbol">Packet type symbol.</param>
    /// <returns>Resolved packet framework namespace, or null when unknown.</returns>
    public string? Resolve(INamedTypeSymbol symbol)
    {
        string? semanticNamespace = GetPacketFrameworkNamespaceFromSymbols(symbol);

        // Prefer semantic resolution when it yields a namespace.
        return semanticNamespace ?? GetPacketFrameworkNamespaceFromSyntax(symbol);
    }

    /// <summary>
    /// Resolves packet namespace by traversing semantic base-type symbols.
    /// </summary>
    /// <param name="symbol">Packet type symbol.</param>
    /// <returns>Resolved namespace, or null when unavailable.</returns>
    private static string? GetPacketFrameworkNamespaceFromSymbols(INamedTypeSymbol symbol)
    {
        INamedTypeSymbol? current = symbol;

        while (current is not null)
        {
            // Stop when packet base class is found in the inheritance chain.
            if (current.Name is PacketGenConstants.ClientPacketTypeName or PacketGenConstants.ServerPacketTypeName)
            {
                string ns = current.ContainingNamespace?.ToDisplayString() ?? string.Empty;

                // Treat global namespace as unresolved for generation purposes.
                if (string.IsNullOrWhiteSpace(ns) || ns == "<global namespace>")
                    return null;

                return ns;
            }

            current = current.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Resolves packet namespace using syntax-only heuristics when semantic lookup fails.
    /// </summary>
    /// <param name="symbol">Packet type symbol.</param>
    /// <returns>Resolved namespace, or null when unavailable.</returns>
    private static string? GetPacketFrameworkNamespaceFromSyntax(INamedTypeSymbol symbol)
    {
        foreach (SyntaxReference syntaxReference in symbol.DeclaringSyntaxReferences)
        {
            // Process only class declarations.
            if (syntaxReference.GetSyntax() is not ClassDeclarationSyntax classDeclaration)
                continue;

            // Skip classes without base-type information.
            if (classDeclaration.BaseList is null)
                continue;

            string? namespaceFromBase = TryGetNamespaceFromBaseType(classDeclaration);

            // Return namespace when explicitly qualified base type is found.
            if (namespaceFromBase is not null)
                return namespaceFromBase;

            string? namespaceFromUsing = TryGetNamespaceFromUsings(classDeclaration);

            // Return namespace when using-directive heuristic finds a candidate.
            if (namespaceFromUsing is not null)
                return namespaceFromUsing;
        }

        return null;
    }

    /// <summary>
    /// Extracts namespace from explicitly qualified base-type syntax.
    /// </summary>
    /// <param name="classDeclaration">Class declaration to inspect.</param>
    /// <returns>Resolved namespace, or null when no qualified base type is found.</returns>
    private static string? TryGetNamespaceFromBaseType(ClassDeclarationSyntax classDeclaration)
    {
        foreach (BaseTypeSyntax baseType in classDeclaration.BaseList!.Types)
        {
            string baseTypeText = baseType.Type.ToString();

            // Match fully-qualified packet base types and strip trailing type name.
            if (baseTypeText.EndsWith("." + PacketGenConstants.ClientPacketTypeName)
                || baseTypeText.EndsWith("." + PacketGenConstants.ServerPacketTypeName))
            {
                return baseTypeText.Substring(0, baseTypeText.LastIndexOf('.'));
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts candidate namespace from using directives for unqualified base types.
    /// </summary>
    /// <param name="classDeclaration">Class declaration to inspect.</param>
    /// <returns>Resolved namespace, or null when no confident candidate exists.</returns>
    private static string? TryGetNamespaceFromUsings(ClassDeclarationSyntax classDeclaration)
    {
        bool usesPacketBaseName = classDeclaration.BaseList!.Types.Any(baseType =>
        {
            string baseTypeText = baseType.Type.ToString();
            return baseTypeText is PacketGenConstants.ClientPacketTypeName or PacketGenConstants.ServerPacketTypeName;
        });

        // Ignore using-based heuristics when packet base type names are absent.
        if (!usesPacketBaseName)
            return null;

        // Require compilation-unit root to inspect using directives.
        if (classDeclaration.SyntaxTree.GetRoot() is not CompilationUnitSyntax compilationUnit)
            return null;

        List<string> candidateUsings = [.. compilationUnit.Usings
            .Where(usingDirective =>
                usingDirective.Alias is null
                && usingDirective.StaticKeyword.RawKind != (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword
                && usingDirective.Name is not null)
            .Select(usingDirective => usingDirective.Name!.ToString())
            .Where(namespaceName => !namespaceName.StartsWith("System"))];

        // Use sole non-System using when there is exactly one candidate.
        if (candidateUsings.Count == 1)
            return candidateUsings[0];

        return candidateUsings.FirstOrDefault(namespaceName => namespaceName.EndsWith(PacketGenConstants.NetcodeNamespaceSuffix));
    }
}
