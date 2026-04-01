using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Template.OptionsGen;

/// <summary>
/// Collapses duplicate option specifications by save key and reports diagnostics for conflicting value kinds.
/// </summary>
internal sealed class OptionSpecDeduplicator : IOptionSpecDeduplicator
{
    private static readonly DiagnosticDescriptor _conflictingKeyTypeDescriptor = new(
        id: "OG001",
        title: "Conflicting option key type",
        messageFormat: "Option save key '{0}' is registered with conflicting types '{1}' and '{2}'.",
        category: OptionsGenConstants.GeneratorCategory,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Produces a unique option specification set keyed by save key.
    /// When the same key appears with different value kinds, a warning is reported and the first entry is kept.
    /// </summary>
    /// <param name="context">Generator context used for reporting diagnostics.</param>
    /// <param name="rawSpecs">Raw option specs discovered from source analysis.</param>
    /// <returns>Deduplicated option specs preserving first-seen entries for each save key.</returns>
    public IReadOnlyList<OptionSettingSpec> Deduplicate(
        SourceProductionContext context,
        ImmutableArray<OptionSettingSpec?> rawSpecs)
    {
        Dictionary<string, OptionSettingSpec> deduped = new(StringComparer.Ordinal);

        foreach (OptionSettingSpec? raw in rawSpecs)
        {
            // Null entries represent extraction misses and are ignored.
            if (raw is null)
                continue;

            // Handle duplicates by save key and optionally report conflicting value kinds.
            if (deduped.TryGetValue(raw.SaveKey, out OptionSettingSpec? existing))
            {
                // Report value-kind conflicts for keys already encountered.
                if (existing.ValueKind != raw.ValueKind)
                {
                    // Surface type conflicts so consumers can correct ambiguous option registrations.
                    context.ReportDiagnostic(Diagnostic.Create(
                        _conflictingKeyTypeDescriptor,
                        Location.None,
                        raw.SaveKey,
                        OptionValueKindNaming.GetTypeKeyword(existing.ValueKind),
                        OptionValueKindNaming.GetTypeKeyword(raw.ValueKind)));
                }

                // Keep the first entry for a key to make conflict resolution deterministic.
                continue;
            }

            deduped.Add(raw.SaveKey, raw);
        }

        // Materialize dictionary values into a list for the emitter contract.
        List<OptionSettingSpec> result = new(deduped.Count);

        foreach (KeyValuePair<string, OptionSettingSpec> entry in deduped)
            result.Add(entry.Value);

        return result;
    }
}
