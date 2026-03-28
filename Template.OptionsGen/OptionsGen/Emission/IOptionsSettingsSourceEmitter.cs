using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Template.OptionsGen;

internal interface IOptionsSettingsSourceEmitter
{
    void Emit(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<OptionSettingSpec?> rawSpecs);
}
