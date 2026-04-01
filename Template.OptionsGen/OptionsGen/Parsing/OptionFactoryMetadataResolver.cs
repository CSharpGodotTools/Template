using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Template.OptionsGen;

/// <summary>
/// Resolves option factory invocation metadata needed to extract save-key and default-value arguments.
/// </summary>
internal sealed class OptionFactoryMetadataResolver : IOptionFactoryMetadataResolver
{
    /// <summary>
    /// Attempts to resolve option metadata from an invocation using semantic symbols when available,
    /// with a syntax-based fallback for partially bound code.
    /// </summary>
    /// <param name="invocation">Invocation expression to inspect.</param>
    /// <param name="method">Resolved method symbol for the invocation, when available.</param>
    /// <param name="metadata">Resolved factory metadata when a supported option factory is identified.</param>
    /// <returns><see langword="true"/> when metadata was resolved; otherwise <see langword="false"/>.</returns>
    public bool TryResolve(InvocationExpressionSyntax invocation, IMethodSymbol? method, out OptionFactoryMetadata? metadata)
    {
        metadata = null;

        // Prefer semantic resolution when the invocation successfully binds to a method symbol.
        if (method is not null)
        {
            // Semantic symbol resolution is the most reliable path for identifying factory calls.
            if (!IsOptionDefinitionsFactory(method))
                return false;

            return TryResolveByMethodName(method.Name, out metadata);
        }

        // Syntax fallback handles cases where semantic binding cannot produce a concrete symbol.
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        // Require an identifier name so the method text can be matched to known factory names.
        if (memberAccess.Name is not IdentifierNameSyntax methodIdentifier)
            return false;

        // Abort if the method name is not one of the supported option factory entry points.
        if (!TryResolveByMethodName(methodIdentifier.Identifier.Text, out metadata))
            return false;

        return memberAccess.Expression.ToString().EndsWith(
            OptionsGenConstants.OptionDefinitionsTypeName,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that a method symbol belongs to the expected option factory type.
    /// </summary>
    /// <param name="method">Method symbol to validate.</param>
    /// <returns><see langword="true"/> when the method is on the supported option definitions type.</returns>
    private static bool IsOptionDefinitionsFactory(IMethodSymbol method)
    {
        // Ensure the method belongs to the expected factory type before accepting it.
        if (method.ContainingType is null || method.ContainingType.Name != OptionsGenConstants.OptionDefinitionsTypeName)
            return false;

        string fullTypeName = method.ContainingType.ToDisplayString();
        return fullTypeName.EndsWith(OptionsGenConstants.OptionDefinitionsQualifiedSuffix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Maps supported option factory method names to extraction metadata.
    /// </summary>
    /// <param name="methodName">Factory method name.</param>
    /// <param name="metadata">Resolved metadata for argument extraction when recognized.</param>
    /// <returns><see langword="true"/> when the method name is supported.</returns>
    private static bool TryResolveByMethodName(string methodName, out OptionFactoryMetadata? metadata)
    {
        metadata = null;

        // Map each supported factory overload to its value kind and positional argument indexes.
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
