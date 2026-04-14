using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Template.OptionsGen;

/// <summary>
/// Extracts <see cref="OptionSettingSpec"/> instances from option factory invocation syntax nodes.
/// </summary>
/// <param name="metadataResolver">Resolves factory metadata for candidate invocations.</param>
/// <param name="argumentResolver">Resolves invocation arguments by parameter name or position.</param>
/// <param name="defaultLiteralResolver">Converts default argument constants into source literals.</param>
internal sealed class OptionSpecExtractor(
    IOptionFactoryMetadataResolver metadataResolver,
    IInvocationArgumentResolver argumentResolver,
    IDefaultLiteralResolver defaultLiteralResolver) : IOptionSpecExtractor
{

    /// <summary>
    /// Resolves option factory method metadata from invocation and symbol information.
    /// </summary>
    public IOptionFactoryMetadataResolver MetadataResolver { get; } = metadataResolver ?? throw new ArgumentNullException(nameof(metadataResolver));

    /// <summary>
    /// Locates invocation argument expressions by parameter name or position.
    /// </summary>
    public IInvocationArgumentResolver ArgumentResolver { get; } = argumentResolver ?? throw new ArgumentNullException(nameof(argumentResolver));

    /// <summary>
    /// Produces normalized literal text for default option values.
    /// </summary>
    public IDefaultLiteralResolver DefaultLiteralResolver { get; } = defaultLiteralResolver ?? throw new ArgumentNullException(nameof(defaultLiteralResolver));

    /// <summary>
    /// Attempts to build an option specification from an invocation syntax context.
    /// Returns <see langword="false"/> when required metadata or constant save-key information is unavailable.
    /// </summary>
    /// <param name="context">Syntax context for the candidate invocation node.</param>
    /// <param name="spec">Extracted option specification when parsing succeeds.</param>
    /// <returns><see langword="true"/> when a valid specification is created; otherwise <see langword="false"/>.</returns>
    public bool TryCreateSpec(GeneratorSyntaxContext context, out OptionSettingSpec? spec)
    {
        spec = null;

        // This extractor only handles invocation nodes discovered by the syntax provider.
        if (context.Node is not InvocationExpressionSyntax invocation)
            return false;

        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        // Fall back to candidate symbols for cases where overload resolution is incomplete during analysis.
        IMethodSymbol? method = symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        // Only continue when the invocation matches one of the recognized option factory shapes.
        if (!MetadataResolver.TryResolve(invocation, method, out OptionFactoryMetadata? metadata) || metadata is null)
            return false;

        // Resolve the save-key argument using metadata-provided name/index rules.
        ExpressionSyntax? saveKeyExpression = ArgumentResolver.FindArgumentExpression(
            invocation,
            method,
            OptionsGenConstants.SaveKeyParameterName,
            metadata.SaveKeyIndex);

        // Missing save-key argument means the invocation cannot be represented as an option spec.
        if (saveKeyExpression is null)
            return false;

        // Save keys must be compile-time constant, non-empty strings to guarantee deterministic generation.
        Optional<object?> saveKeyConstant = context.SemanticModel.GetConstantValue(saveKeyExpression);

        // Reject missing, non-string, or empty save keys during spec extraction.
        if (!saveKeyConstant.HasValue || saveKeyConstant.Value is not string saveKey || string.IsNullOrWhiteSpace(saveKey))
            return false;

        // Normalize the default value to emitted literal syntax for the discovered option kind.
        string defaultLiteral = DefaultLiteralResolver.ResolveDefaultLiteral(
            context.SemanticModel,
            invocation,
            method,
            metadata.ValueKind,
            metadata.DefaultValueIndex);

        spec = new OptionSettingSpec(saveKey.Trim(), metadata.ValueKind, defaultLiteral);
        return true;
    }
}
