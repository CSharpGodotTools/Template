using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Template.OptionsGen;

internal sealed class InvocationArgumentResolver : IInvocationArgumentResolver
{
    public ExpressionSyntax? FindArgumentExpression(
        InvocationExpressionSyntax invocation,
        IMethodSymbol? method,
        string parameterName,
        int fallbackPosition)
    {
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            if (argument.NameColon?.Name.Identifier.Text == parameterName)
                return argument.Expression;
        }

        if (method is not null)
        {
            int index = FindParameterIndex(method, parameterName);

            if (index >= 0)
            {
                if (invocation.ArgumentList.Arguments.Count <= index)
                    return null;

                ArgumentSyntax indexedArgument = invocation.ArgumentList.Arguments[index];
                if (indexedArgument.NameColon is not null)
                    return null;

                return indexedArgument.Expression;
            }
        }

        if (fallbackPosition < 0 || invocation.ArgumentList.Arguments.Count <= fallbackPosition)
            return null;

        ArgumentSyntax fallbackArgument = invocation.ArgumentList.Arguments[fallbackPosition];
        if (fallbackArgument.NameColon is not null)
            return null;

        return fallbackArgument.Expression;
    }

    private static int FindParameterIndex(IMethodSymbol method, string parameterName)
    {
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            if (method.Parameters[i].Name == parameterName)
                return i;
        }

        return -1;
    }
}
