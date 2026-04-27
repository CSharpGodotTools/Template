using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Template.OptionsGen;

/// <summary>
/// Coordinates the final source-generation pass for option settings by locating the target type,
/// normalizing discovered specs, and emitting a single deterministic source file.
/// </summary>
/// <param name="specDeduplicator">Deduplicates extracted option specifications prior to emission.</param>
/// <param name="typeLocator">Resolves the target options settings symbol in the compilation.</param>
/// <param name="sourceBuilder">Builds generated source text from normalized option metadata.</param>
internal sealed class OptionsSettingsSourceEmitter(
    IOptionSpecDeduplicator specDeduplicator,
    IOptionsSettingsTypeLocator typeLocator,
    IOptionsSettingsSourceBuilder sourceBuilder) : IOptionsSettingsSourceEmitter
{
    private static readonly DiagnosticDescriptor _missingOptionsSettingsDescriptor = new(
        id: "OG002",
        title: "OptionsSettings not found",
        messageFormat: "Could not find an OptionsSettings type. Strongly typed option properties were not generated.",
        category: OptionsGenConstants.GeneratorCategory,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// Deduplicates parsed option specifications before source emission.
    /// </summary>
    public IOptionSpecDeduplicator SpecDeduplicator { get; } = specDeduplicator ?? throw new ArgumentNullException(nameof(specDeduplicator));

    /// <summary>
    /// Locates the runtime options settings type that receives generated properties.
    /// </summary>
    public IOptionsSettingsTypeLocator TypeLocator { get; } = typeLocator ?? throw new ArgumentNullException(nameof(typeLocator));

    /// <summary>
    /// Builds the final generated source text from resolved symbols and normalized specs.
    /// </summary>
    public IOptionsSettingsSourceBuilder SourceBuilder { get; } = sourceBuilder ?? throw new ArgumentNullException(nameof(sourceBuilder));

    /// <summary>
    /// Emits strongly typed option accessors when option specs are present and an
    /// <c>OptionsSettings</c> type is available in the current compilation.
    /// </summary>
    /// <param name="context">Source generator context used for diagnostics and source output.</param>
    /// <param name="compilation">Compilation to search for the target options settings type.</param>
    /// <param name="rawSpecs">Option specifications collected during earlier generator stages.</param>
    public void Emit(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<OptionSettingSpec?> rawSpecs)
    {
        // No discovered option metadata means there is nothing to generate for this compilation.
        if (rawSpecs.IsDefaultOrEmpty)
            return;

        // Generated properties target OptionsSettings; skip generation if that type is absent.
        INamedTypeSymbol? optionsSettings = TypeLocator.FindTypeByName(
            compilation.Assembly.GlobalNamespace,
            OptionsGenConstants.OptionsSettingsTypeName);

        // Report diagnostic and skip generation when OptionsSettings cannot be located.
        if (optionsSettings is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(_missingOptionsSettingsDescriptor, Location.None));
            return;
        }

        // Normalize duplicates before emission so each save key maps to a single generated property.
        IReadOnlyList<OptionSettingSpec> dedupedSpecs = SpecDeduplicator.Deduplicate(context, rawSpecs);

        // Stop when deduplication yields no remaining option specs.
        if (dedupedSpecs.Count == 0)
            return;

        // Keep output ordering stable across runs to avoid noisy generated-source diffs.
        List<OptionSettingSpec> orderedSpecs = [.. dedupedSpecs];
        orderedSpecs.Sort(static (left, right) => string.CompareOrdinal(left.SaveKey, right.SaveKey));

        // Emit one generated source file containing all normalized option accessors.
        string generatedSource = SourceBuilder.BuildSource(optionsSettings, orderedSpecs);
        context.AddSource(OptionsGenConstants.GeneratedSourceHintName, SourceText.From(generatedSource, Encoding.UTF8));
    }
}
