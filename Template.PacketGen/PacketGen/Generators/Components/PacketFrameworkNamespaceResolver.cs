using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace PacketGen.Generators;

internal sealed class PacketFrameworkNamespaceResolver
{
    public string? Resolve(INamedTypeSymbol symbol)
    {
        string? semanticNamespace = GetPacketFrameworkNamespaceFromSymbols(symbol);
        if (semanticNamespace is not null)
            return semanticNamespace;

        return GetPacketFrameworkNamespaceFromSyntax(symbol);
    }

    private static string? GetPacketFrameworkNamespaceFromSymbols(INamedTypeSymbol symbol)
    {
        INamedTypeSymbol? current = symbol;

        while (current is not null)
        {
            if (current.Name is PacketGenConstants.ClientPacketTypeName or PacketGenConstants.ServerPacketTypeName)
            {
                string ns = current.ContainingNamespace?.ToDisplayString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(ns) || ns == "<global namespace>")
                    return null;

                return ns;
            }

            current = current.BaseType;
        }

        return null;
    }

    private static string? GetPacketFrameworkNamespaceFromSyntax(INamedTypeSymbol symbol)
    {
        foreach (SyntaxReference syntaxReference in symbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not ClassDeclarationSyntax classDeclaration)
                continue;

            if (classDeclaration.BaseList is null)
                continue;

            string? namespaceFromBase = TryGetNamespaceFromBaseType(classDeclaration);
            if (namespaceFromBase is not null)
                return namespaceFromBase;

            string? namespaceFromUsing = TryGetNamespaceFromUsings(classDeclaration);
            if (namespaceFromUsing is not null)
                return namespaceFromUsing;
        }

        return null;
    }

    private static string? TryGetNamespaceFromBaseType(ClassDeclarationSyntax classDeclaration)
    {
        foreach (BaseTypeSyntax baseType in classDeclaration.BaseList!.Types)
        {
            string baseTypeText = baseType.Type.ToString();

            if (baseTypeText.EndsWith("." + PacketGenConstants.ClientPacketTypeName)
                || baseTypeText.EndsWith("." + PacketGenConstants.ServerPacketTypeName))
                return baseTypeText.Substring(0, baseTypeText.LastIndexOf('.'));
        }

        return null;
    }

    private static string? TryGetNamespaceFromUsings(ClassDeclarationSyntax classDeclaration)
    {
        bool usesPacketBaseName = classDeclaration.BaseList!.Types.Any(baseType =>
        {
            string baseTypeText = baseType.Type.ToString();
            return baseTypeText is PacketGenConstants.ClientPacketTypeName or PacketGenConstants.ServerPacketTypeName;
        });

        if (!usesPacketBaseName)
            return null;

        if (classDeclaration.SyntaxTree.GetRoot() is not CompilationUnitSyntax compilationUnit)
            return null;

        List<string> candidateUsings = compilationUnit.Usings
            .Where(usingDirective =>
                usingDirective.Alias is null
                && usingDirective.StaticKeyword.RawKind != (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword
                && usingDirective.Name is not null)
            .Select(usingDirective => usingDirective.Name!.ToString())
            .Where(namespaceName => !namespaceName.StartsWith("System"))
            .ToList();

        if (candidateUsings.Count == 1)
            return candidateUsings[0];

        return candidateUsings.FirstOrDefault(namespaceName => namespaceName.EndsWith(PacketGenConstants.NetcodeNamespaceSuffix));
    }
}
