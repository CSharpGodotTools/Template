using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Template.OptionsGen;

internal sealed class OptionsSettingsSourceEmitter : IOptionsSettingsSourceEmitter
{
    private static readonly DiagnosticDescriptor MissingOptionsSettingsDescriptor = new(
        id: "OG002",
        title: "OptionsSettings not found",
        messageFormat: "Could not find an OptionsSettings type. Strongly typed option properties were not generated.",
        category: OptionsGenConstants.GeneratorCategory,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private readonly IOptionSpecDeduplicator _specDeduplicator;
    private readonly IOptionsSettingsTypeLocator _typeLocator;
    private readonly IOptionsSettingsSourceBuilder _sourceBuilder;

    public OptionsSettingsSourceEmitter(
        IOptionSpecDeduplicator specDeduplicator,
        IOptionsSettingsTypeLocator typeLocator,
        IOptionsSettingsSourceBuilder sourceBuilder)
    {
        _specDeduplicator = specDeduplicator ?? throw new ArgumentNullException(nameof(specDeduplicator));
        _typeLocator = typeLocator ?? throw new ArgumentNullException(nameof(typeLocator));
        _sourceBuilder = sourceBuilder ?? throw new ArgumentNullException(nameof(sourceBuilder));
    }

    public IOptionSpecDeduplicator SpecDeduplicator => _specDeduplicator;
    public IOptionsSettingsTypeLocator TypeLocator => _typeLocator;
    public IOptionsSettingsSourceBuilder SourceBuilder => _sourceBuilder;

    public void Emit(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<OptionSettingSpec?> rawSpecs)
    {
        if (rawSpecs.IsDefaultOrEmpty)
            return;

        INamedTypeSymbol? optionsSettings = _typeLocator.FindTypeByName(
            compilation.Assembly.GlobalNamespace,
            OptionsGenConstants.OptionsSettingsTypeName);

        if (optionsSettings is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingOptionsSettingsDescriptor, Location.None));
            return;
        }

        IReadOnlyList<OptionSettingSpec> dedupedSpecs = _specDeduplicator.Deduplicate(context, rawSpecs);
        if (dedupedSpecs.Count == 0)
            return;

        List<OptionSettingSpec> orderedSpecs = new(dedupedSpecs);
        orderedSpecs.Sort(static (left, right) => string.Compare(left.SaveKey, right.SaveKey, StringComparison.Ordinal));

        string generatedSource = _sourceBuilder.BuildSource(optionsSettings, orderedSpecs);
        context.AddSource(OptionsGenConstants.GeneratedSourceHintName, SourceText.From(generatedSource, Encoding.UTF8));
    }
}
