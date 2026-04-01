using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;

namespace Template.OptionsGen;

/// <summary>
/// Incremental source generator that emits strongly typed options settings accessors.
/// </summary>
[Generator]
public sealed class OptionsSettingsGenerator : IIncrementalGenerator
{
    private readonly IOptionSpecExtractor _specExtractor;
    private readonly IOptionsSettingsSourceEmitter _sourceEmitter;

    /// <summary>
    /// Extracts option specifications from syntax contexts.
    /// </summary>
    internal IOptionSpecExtractor SpecExtractor => _specExtractor;

    /// <summary>
    /// Emits generated source from extracted option specifications.
    /// </summary>
    internal IOptionsSettingsSourceEmitter SourceEmitter => _sourceEmitter;

    /// <summary>
    /// Creates the generator with default parser and emitter implementations.
    /// </summary>
    public OptionsSettingsGenerator()
        : this(CreateDefaultSpecExtractor(), CreateDefaultSourceEmitter())
    {
    }

    /// <summary>
    /// Creates the generator with explicit parser and emitter dependencies.
    /// </summary>
    /// <param name="specExtractor">Specification extractor implementation.</param>
    /// <param name="sourceEmitter">Source emitter implementation.</param>
    internal OptionsSettingsGenerator(IOptionSpecExtractor specExtractor, IOptionsSettingsSourceEmitter sourceEmitter)
    {
        _specExtractor = specExtractor ?? throw new ArgumentNullException(nameof(specExtractor));
        _sourceEmitter = sourceEmitter ?? throw new ArgumentNullException(nameof(sourceEmitter));
    }

    /// <summary>
    /// Registers incremental syntax and emission pipelines for options generation.
    /// </summary>
    /// <param name="context">Generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Build syntax pipeline that extracts option specs from invocation nodes.
        IncrementalValuesProvider<OptionSettingSpec?> specs = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InvocationExpressionSyntax,
                (syntaxContext, _) => _specExtractor.TryCreateSpec(syntaxContext, out OptionSettingSpec? spec) ? spec : null);

        // Combine extracted specs with compilation for final emission stage.
        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<OptionSettingSpec?> Specs)> combined =
            context.CompilationProvider.Combine(specs.Collect());

        // Emit generated source once compilation and collected specs are available.
        context.RegisterSourceOutput(combined, (productionContext, source) =>
        {
            _sourceEmitter.Emit(productionContext, source.Compilation, source.Specs);
        });
    }

    /// <summary>
    /// Creates the default option-spec extractor used by this generator.
    /// </summary>
    /// <returns>Default option spec extractor instance.</returns>
    private static IOptionSpecExtractor CreateDefaultSpecExtractor()
    {
        IInvocationArgumentResolver argumentResolver = new InvocationArgumentResolver();

        return new OptionSpecExtractor(
            new OptionFactoryMetadataResolver(),
            argumentResolver,
            new DefaultLiteralResolver(argumentResolver));
    }

    /// <summary>
    /// Creates the default source emitter used by this generator.
    /// </summary>
    /// <returns>Default source emitter instance.</returns>
    private static IOptionsSettingsSourceEmitter CreateDefaultSourceEmitter()
    {
        return new OptionsSettingsSourceEmitter(
            new OptionSpecDeduplicator(),
            new OptionsSettingsTypeLocator(),
            new OptionsSettingsSourceBuilder());
    }
}
