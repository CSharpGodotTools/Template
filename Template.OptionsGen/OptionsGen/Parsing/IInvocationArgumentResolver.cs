using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Template.OptionsGen;

/// <summary>
/// Resolves invocation arguments to expressions based on parameter identity and positional rules.
/// </summary>
internal interface IInvocationArgumentResolver
{
    /// <summary>
    /// Finds the best matching argument expression for a logical parameter.
    /// </summary>
    /// <param name="invocation">Invocation expression to inspect.</param>
    /// <param name="method">Resolved method symbol used for parameter-index mapping, when available.</param>
    /// <param name="parameterName">Parameter name to resolve.</param>
    /// <param name="fallbackPosition">Positional fallback index when semantic mapping is unavailable.</param>
    /// <returns>Matching argument expression, or <see langword="null"/> if no valid argument is found.</returns>
    ExpressionSyntax? FindArgumentExpression(
        InvocationExpressionSyntax invocation,
        IMethodSymbol? method,
        string parameterName,
        int fallbackPosition);
}
