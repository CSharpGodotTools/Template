using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Template.OptionsGen;

internal interface IOptionSpecDeduplicator
{
    IReadOnlyList<OptionSettingSpec> Deduplicate(
        SourceProductionContext context,
        ImmutableArray<OptionSettingSpec?> rawSpecs);
}
