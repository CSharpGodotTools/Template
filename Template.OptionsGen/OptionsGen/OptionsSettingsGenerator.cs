using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;

namespace Template.OptionsGen;

[Generator]
public sealed class OptionsSettingsGenerator : IIncrementalGenerator
{
    private readonly IOptionSpecExtractor _specExtractor;
    private readonly IOptionsSettingsSourceEmitter _sourceEmitter;

    internal IOptionSpecExtractor SpecExtractor => _specExtractor;
    internal IOptionsSettingsSourceEmitter SourceEmitter => _sourceEmitter;

    public OptionsSettingsGenerator()
        : this(CreateDefaultSpecExtractor(), CreateDefaultSourceEmitter())
    {
    }

    internal OptionsSettingsGenerator(IOptionSpecExtractor specExtractor, IOptionsSettingsSourceEmitter sourceEmitter)
    {
        _specExtractor = specExtractor ?? throw new ArgumentNullException(nameof(specExtractor));
        _sourceEmitter = sourceEmitter ?? throw new ArgumentNullException(nameof(sourceEmitter));
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<OptionSettingSpec?> specs = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InvocationExpressionSyntax,
                (syntaxContext, _) => _specExtractor.TryCreateSpec(syntaxContext, out OptionSettingSpec? spec) ? spec : null);

        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<OptionSettingSpec?> Specs)> combined =
            context.CompilationProvider.Combine(specs.Collect());

        context.RegisterSourceOutput(combined, (productionContext, source) =>
        {
            _sourceEmitter.Emit(productionContext, source.Compilation, source.Specs);
        });
    }

    private static IOptionSpecExtractor CreateDefaultSpecExtractor()
    {
        IInvocationArgumentResolver argumentResolver = new InvocationArgumentResolver();

        return new OptionSpecExtractor(
            new OptionFactoryMetadataResolver(),
            argumentResolver,
            new DefaultLiteralResolver(argumentResolver));
    }

    private static IOptionsSettingsSourceEmitter CreateDefaultSourceEmitter()
    {
        return new OptionsSettingsSourceEmitter(
            new OptionSpecDeduplicator(),
            new OptionsSettingsTypeLocator(),
            new OptionsSettingsSourceBuilder());
    }
}
