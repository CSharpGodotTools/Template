using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Globalization;

namespace Template.OptionsGen;

/// <summary>
/// Resolves option default-value arguments into C# literal text suitable for generated source.
/// </summary>
/// <param name="argumentResolver">Resolves invocation arguments by parameter name or position.</param>
internal sealed class DefaultLiteralResolver(IInvocationArgumentResolver argumentResolver) : IDefaultLiteralResolver
{
    private readonly IInvocationArgumentResolver _argumentResolver = argumentResolver ?? throw new ArgumentNullException(nameof(argumentResolver));

    /// <summary>
    /// Finds candidate default-value argument expressions for option factory invocations.
    /// </summary>
    public IInvocationArgumentResolver ArgumentResolver => _argumentResolver;

    /// <summary>
    /// Resolves the default-value argument for an option invocation to emitted literal text,
    /// falling back to type-based defaults when the argument is missing or not a valid constant.
    /// </summary>
    /// <param name="model">Semantic model used for constant evaluation.</param>
    /// <param name="invocation">Invocation expression containing option arguments.</param>
    /// <param name="method">Resolved method symbol for the invocation, when available.</param>
    /// <param name="valueKind">Option value kind the literal must target.</param>
    /// <param name="fallbackPosition">Positional index to use when named lookup is unavailable.</param>
    /// <returns>Literal text to embed in generated source.</returns>
    public string ResolveDefaultLiteral(
        SemanticModel model,
        InvocationExpressionSyntax invocation,
        IMethodSymbol? method,
        OptionValueKind valueKind,
        int fallbackPosition)
    {
        ExpressionSyntax? defaultExpression = _argumentResolver.FindArgumentExpression(
            invocation,
            method,
            OptionsGenConstants.DefaultValueParameterName,
            fallbackPosition);

        // Missing default argument uses the kind-specific safe fallback.
        if (defaultExpression is null)
            return GetFallbackDefaultLiteral(valueKind);

        // Only compile-time constants can be emitted directly as source literals.
        Optional<object?> constant = model.GetConstantValue(defaultExpression);
        // Use fallback defaults when a constant value cannot be resolved.
        if (!constant.HasValue || constant.Value is null)
            return GetFallbackDefaultLiteral(valueKind);

        return ConvertDefaultLiteral(constant.Value, valueKind);
    }

    /// <summary>
    /// Converts a constant runtime value into emitted literal text for the requested option kind.
    /// </summary>
    /// <param name="value">Constant value extracted from semantic analysis.</param>
    /// <param name="valueKind">Target option value kind.</param>
    /// <returns>Literal text that can be embedded in generated code.</returns>
    private static string ConvertDefaultLiteral(object value, OptionValueKind valueKind)
    {
        try
        {
            switch (valueKind)
            {
                case OptionValueKind.Int:
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

                case OptionValueKind.Float:
                    float floatValue = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                    // Non-finite floats cannot be represented as standard literal tokens.
                    if (float.IsNaN(floatValue) || float.IsInfinity(floatValue))
                        return GetFallbackDefaultLiteral(valueKind);

                    return floatValue.ToString("R", CultureInfo.InvariantCulture) + "f";

                case OptionValueKind.String:
                    return SymbolDisplay.FormatLiteral(
                        Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
                        quote: true);

                case OptionValueKind.Bool:
                    return Convert.ToBoolean(value, CultureInfo.InvariantCulture) ? "true" : "false";

                default:
                    return GetFallbackDefaultLiteral(valueKind);
            }
        }
        catch
        {
            // Invalid conversions degrade to fallback literals to keep generated code valid.
            return GetFallbackDefaultLiteral(valueKind);
        }
    }

    /// <summary>
    /// Provides conservative default literals when conversion or extraction cannot produce a concrete value.
    /// </summary>
    /// <param name="valueKind">Target option value kind.</param>
    /// <returns>Fallback literal text for generated source.</returns>
    private static string GetFallbackDefaultLiteral(OptionValueKind valueKind)
    {
        return valueKind switch
        {
            OptionValueKind.Int => "0",
            OptionValueKind.Float => "0f",
            OptionValueKind.String => "string.Empty",
            OptionValueKind.Bool => "false",
            _ => "default",
        };
    }
}
