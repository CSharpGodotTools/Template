using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Template.OptionsGen;

internal interface IOptionsSettingsSourceBuilder
{
    string BuildSource(INamedTypeSymbol optionsSettings, IReadOnlyList<OptionSettingSpec> specs);
}
