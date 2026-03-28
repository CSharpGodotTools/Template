using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Template.OptionsGen;

internal sealed class OptionSpecDeduplicator : IOptionSpecDeduplicator
{
    private static readonly DiagnosticDescriptor ConflictingKeyTypeDescriptor = new(
        id: "OG001",
        title: "Conflicting option key type",
        messageFormat: "Option save key '{0}' is registered with conflicting types '{1}' and '{2}'.",
        category: OptionsGenConstants.GeneratorCategory,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public IReadOnlyList<OptionSettingSpec> Deduplicate(
        SourceProductionContext context,
        ImmutableArray<OptionSettingSpec?> rawSpecs)
    {
        Dictionary<string, OptionSettingSpec> deduped = new(StringComparer.Ordinal);

        foreach (OptionSettingSpec? raw in rawSpecs)
        {
            if (raw is null)
                continue;

            if (deduped.TryGetValue(raw.SaveKey, out OptionSettingSpec? existing))
            {
                if (existing.ValueKind != raw.ValueKind)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        ConflictingKeyTypeDescriptor,
                        Location.None,
                        raw.SaveKey,
                        OptionValueKindNaming.GetTypeKeyword(existing.ValueKind),
                        OptionValueKindNaming.GetTypeKeyword(raw.ValueKind)));
                }

                continue;
            }

            deduped.Add(raw.SaveKey, raw);
        }

        List<OptionSettingSpec> result = new(deduped.Count);

        foreach (KeyValuePair<string, OptionSettingSpec> entry in deduped)
            result.Add(entry.Value);

        return result;
    }
}
