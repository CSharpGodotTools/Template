using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Globalization;

namespace Template.OptionsGen;

internal sealed class DefaultLiteralResolver : IDefaultLiteralResolver
{
    private readonly IInvocationArgumentResolver _argumentResolver;

    public DefaultLiteralResolver(IInvocationArgumentResolver argumentResolver)
    {
        _argumentResolver = argumentResolver ?? throw new ArgumentNullException(nameof(argumentResolver));
    }

    public IInvocationArgumentResolver ArgumentResolver => _argumentResolver;

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

        if (defaultExpression is null)
            return GetFallbackDefaultLiteral(valueKind);

        Optional<object?> constant = model.GetConstantValue(defaultExpression);
        if (!constant.HasValue || constant.Value is null)
            return GetFallbackDefaultLiteral(valueKind);

        return ConvertDefaultLiteral(constant.Value, valueKind);
    }

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
            return GetFallbackDefaultLiteral(valueKind);
        }
    }

    private static string GetFallbackDefaultLiteral(OptionValueKind valueKind)
    {
        switch (valueKind)
        {
            case OptionValueKind.Int:
                return "0";
            case OptionValueKind.Float:
                return "0f";
            case OptionValueKind.String:
                return "string.Empty";
            case OptionValueKind.Bool:
                return "false";
            default:
                return "default";
        }
    }
}
