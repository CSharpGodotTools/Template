using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Template.OptionsGen;

internal interface IInvocationArgumentResolver
{
    ExpressionSyntax? FindArgumentExpression(
        InvocationExpressionSyntax invocation,
        IMethodSymbol? method,
        string parameterName,
        int fallbackPosition);
}
