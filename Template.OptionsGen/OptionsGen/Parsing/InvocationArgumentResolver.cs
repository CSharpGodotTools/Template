using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Template.OptionsGen;

/// <summary>
/// Resolves invocation argument expressions by parameter name with symbol-aware and positional fallbacks.
/// </summary>
internal sealed class InvocationArgumentResolver : IInvocationArgumentResolver
{
    /// <summary>
    /// Finds the argument expression for a logical parameter by checking named arguments first,
    /// then mapped positional arguments, and finally a caller-supplied positional fallback.
    /// </summary>
    /// <param name="invocation">Invocation expression containing candidate arguments.</param>
    /// <param name="method">Resolved method symbol for parameter mapping, when available.</param>
    /// <param name="parameterName">Parameter name to resolve.</param>
    /// <param name="fallbackPosition">Positional fallback index when symbol mapping is unavailable.</param>
    /// <returns>Resolved argument expression, or <see langword="null"/> if no valid match exists.</returns>
    public ExpressionSyntax? FindArgumentExpression(
        InvocationExpressionSyntax invocation,
        IMethodSymbol? method,
        string parameterName,
        int fallbackPosition)
    {
        // Named arguments are authoritative regardless of argument order.
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            // Prefer an exact named-argument match for the requested parameter.
            if (argument.NameColon?.Name.Identifier.Text == parameterName)
                return argument.Expression;
        }

        // Use semantic mapping only when method symbol information is available.
        if (method is not null)
        {
            // Use semantic parameter order to resolve positional calls safely.
            int index = FindParameterIndex(method, parameterName);

            // Continue only when the parameter exists in the resolved method.
            if (index >= 0)
            {
                // Reject out-of-range indices in shortened argument lists.
                if (invocation.ArgumentList.Arguments.Count <= index)
                    return null;

                ArgumentSyntax indexedArgument = invocation.ArgumentList.Arguments[index];
                // A named argument at this slot indicates positional mapping is unreliable here.
                if (indexedArgument.NameColon is not null)
                    return null;

                return indexedArgument.Expression;
            }
        }

        // Final fallback supports known factory signatures when symbol binding is unavailable.
        if (fallbackPosition < 0 || invocation.ArgumentList.Arguments.Count <= fallbackPosition)
            return null;

        ArgumentSyntax fallbackArgument = invocation.ArgumentList.Arguments[fallbackPosition];

        // Reject named fallback entries because positional fallback expects position-only.
        if (fallbackArgument.NameColon is not null)
            return null;

        return fallbackArgument.Expression;
    }

    /// <summary>
    /// Returns the zero-based index of a method parameter by name.
    /// </summary>
    /// <param name="method">Method symbol containing parameters to inspect.</param>
    /// <param name="parameterName">Parameter name to locate.</param>
    /// <returns>Parameter index, or <c>-1</c> when no matching parameter exists.</returns>
    private static int FindParameterIndex(IMethodSymbol method, string parameterName)
    {
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            // Return the first parameter whose symbol name matches the lookup key.
            if (method.Parameters[i].Name == parameterName)
                return i;
        }

        return -1;
    }
}
