using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Template.OptionsGen;

internal interface IDefaultLiteralResolver
{
    string ResolveDefaultLiteral(
        SemanticModel model,
        InvocationExpressionSyntax invocation,
        IMethodSymbol? method,
        OptionValueKind valueKind,
        int fallbackPosition);
}
