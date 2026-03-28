using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Template.OptionsGen;

internal interface IOptionFactoryMetadataResolver
{
    bool TryResolve(InvocationExpressionSyntax invocation, IMethodSymbol? method, out OptionFactoryMetadata? metadata);
}
