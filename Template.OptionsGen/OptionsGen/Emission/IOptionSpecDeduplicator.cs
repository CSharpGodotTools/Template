using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Template.OptionsGen;

/// <summary>
/// Normalizes raw option specifications into a deduplicated set suitable for source emission.
/// </summary>
internal interface IOptionSpecDeduplicator
{
    /// <summary>
    /// Deduplicates parsed option specs and reports diagnostics for invalid duplicate combinations.
    /// </summary>
    /// <param name="context">Generator context used for diagnostic reporting.</param>
    /// <param name="rawSpecs">Raw option specs from parsing stages.</param>
    /// <returns>Deduplicated option specs keyed by save key.</returns>
    IReadOnlyList<OptionSettingSpec> Deduplicate(
        SourceProductionContext context,
        ImmutableArray<OptionSettingSpec?> rawSpecs);
}
