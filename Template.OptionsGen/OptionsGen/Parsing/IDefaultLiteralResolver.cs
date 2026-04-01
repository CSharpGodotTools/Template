using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Template.OptionsGen;

/// <summary>
/// Resolves default-value arguments into literal text usable by generated source.
/// </summary>
internal interface IDefaultLiteralResolver
{
    /// <summary>
    /// Produces literal text for an option default value, using fallback behavior when extraction fails.
    /// </summary>
    /// <param name="model">Semantic model used for constant evaluation.</param>
    /// <param name="invocation">Invocation expression that may contain the default argument.</param>
    /// <param name="method">Resolved method symbol for argument binding, when available.</param>
    /// <param name="valueKind">Target option value kind for conversion.</param>
    /// <param name="fallbackPosition">Positional fallback index for argument lookup.</param>
    /// <returns>Literal text to embed in generated source.</returns>
    string ResolveDefaultLiteral(
        SemanticModel model,
        InvocationExpressionSyntax invocation,
        IMethodSymbol? method,
        OptionValueKind valueKind,
        int fallbackPosition);
}
