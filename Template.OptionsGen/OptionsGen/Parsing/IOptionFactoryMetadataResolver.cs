using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Template.OptionsGen;

/// <summary>
/// Resolves extraction metadata for recognized option factory invocations.
/// </summary>
internal interface IOptionFactoryMetadataResolver
{
    /// <summary>
    /// Attempts to map an invocation to option value-kind metadata and argument indexes.
    /// </summary>
    /// <param name="invocation">Invocation expression under analysis.</param>
    /// <param name="method">Resolved method symbol, when available.</param>
    /// <param name="metadata">Resolved metadata for save-key/default-value extraction.</param>
    /// <returns><see langword="true"/> when the invocation matches a supported option factory.</returns>
    bool TryResolve(InvocationExpressionSyntax invocation, IMethodSymbol? method, out OptionFactoryMetadata? metadata);
}
