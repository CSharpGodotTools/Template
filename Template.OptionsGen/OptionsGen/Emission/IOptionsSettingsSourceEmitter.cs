using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Template.OptionsGen;

/// <summary>
/// Emits generated source for strongly typed option settings during source-generation execution.
/// </summary>
internal interface IOptionsSettingsSourceEmitter
{
    /// <summary>
    /// Executes the emission pipeline for collected option specifications.
    /// </summary>
    /// <param name="context">Source generator context for diagnostics and generated output.</param>
    /// <param name="compilation">Current compilation being analyzed.</param>
    /// <param name="rawSpecs">Collected option specs from parsing stage.</param>
    void Emit(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<OptionSettingSpec?> rawSpecs);
}
