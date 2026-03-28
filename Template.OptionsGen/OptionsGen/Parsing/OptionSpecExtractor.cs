using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Template.OptionsGen;

internal sealed class OptionSpecExtractor : IOptionSpecExtractor
{
    private readonly IOptionFactoryMetadataResolver _metadataResolver;
    private readonly IInvocationArgumentResolver _argumentResolver;
    private readonly IDefaultLiteralResolver _defaultLiteralResolver;

    public OptionSpecExtractor(
        IOptionFactoryMetadataResolver metadataResolver,
        IInvocationArgumentResolver argumentResolver,
        IDefaultLiteralResolver defaultLiteralResolver)
    {
        _metadataResolver = metadataResolver ?? throw new ArgumentNullException(nameof(metadataResolver));
        _argumentResolver = argumentResolver ?? throw new ArgumentNullException(nameof(argumentResolver));
        _defaultLiteralResolver = defaultLiteralResolver ?? throw new ArgumentNullException(nameof(defaultLiteralResolver));
    }

    public IOptionFactoryMetadataResolver MetadataResolver => _metadataResolver;
    public IInvocationArgumentResolver ArgumentResolver => _argumentResolver;
    public IDefaultLiteralResolver DefaultLiteralResolver => _defaultLiteralResolver;

    public bool TryCreateSpec(GeneratorSyntaxContext context, out OptionSettingSpec? spec)
    {
        spec = null;

        if (context.Node is not InvocationExpressionSyntax invocation)
            return false;

        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        IMethodSymbol? method = symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        if (!_metadataResolver.TryResolve(invocation, method, out OptionFactoryMetadata? metadata) || metadata is null)
            return false;

        ExpressionSyntax? saveKeyExpression = _argumentResolver.FindArgumentExpression(
            invocation,
            method,
            OptionsGenConstants.SaveKeyParameterName,
            metadata.SaveKeyIndex);

        if (saveKeyExpression is null)
            return false;

        Optional<object?> saveKeyConstant = context.SemanticModel.GetConstantValue(saveKeyExpression);
        if (!saveKeyConstant.HasValue || saveKeyConstant.Value is not string saveKey || string.IsNullOrWhiteSpace(saveKey))
            return false;

        string defaultLiteral = _defaultLiteralResolver.ResolveDefaultLiteral(
            context.SemanticModel,
            invocation,
            method,
            metadata.ValueKind,
            metadata.DefaultValueIndex);

        spec = new OptionSettingSpec(saveKey.Trim(), metadata.ValueKind, defaultLiteral);
        return true;
    }
}
