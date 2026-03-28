using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Template.OptionsGen;

internal sealed class OptionFactoryMetadataResolver : IOptionFactoryMetadataResolver
{
    public bool TryResolve(InvocationExpressionSyntax invocation, IMethodSymbol? method, out OptionFactoryMetadata? metadata)
    {
        metadata = null;

        if (method is not null)
        {
            if (!IsOptionDefinitionsFactory(method))
                return false;

            return TryResolveByMethodName(method.Name, out metadata);
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        if (memberAccess.Name is not IdentifierNameSyntax methodIdentifier)
            return false;

        if (!TryResolveByMethodName(methodIdentifier.Identifier.Text, out metadata))
            return false;

        return memberAccess.Expression.ToString().EndsWith(
            OptionsGenConstants.OptionDefinitionsTypeName,
            StringComparison.Ordinal);
    }

    private static bool IsOptionDefinitionsFactory(IMethodSymbol method)
    {
        if (method.ContainingType is null || method.ContainingType.Name != OptionsGenConstants.OptionDefinitionsTypeName)
            return false;

        string fullTypeName = method.ContainingType.ToDisplayString();
        return fullTypeName.EndsWith(OptionsGenConstants.OptionDefinitionsQualifiedSuffix, StringComparison.Ordinal);
    }

    private static bool TryResolveByMethodName(string methodName, out OptionFactoryMetadata? metadata)
    {
        metadata = null;

        switch (methodName)
        {
            case "Dropdown":
                metadata = new OptionFactoryMetadata(OptionValueKind.Int, saveKeyIndex: 6, defaultValueIndex: 7);
                return true;
            case "Slider":
                metadata = new OptionFactoryMetadata(OptionValueKind.Float, saveKeyIndex: 7, defaultValueIndex: 8);
                return true;
            case "LineEdit":
                metadata = new OptionFactoryMetadata(OptionValueKind.String, saveKeyIndex: 5, defaultValueIndex: 6);
                return true;
            case "Toggle":
                metadata = new OptionFactoryMetadata(OptionValueKind.Bool, saveKeyIndex: 4, defaultValueIndex: 5);
                return true;
            default:
                return false;
        }
    }
}
