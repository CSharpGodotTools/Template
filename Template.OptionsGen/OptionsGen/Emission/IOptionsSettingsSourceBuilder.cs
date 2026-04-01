using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Template.OptionsGen;

/// <summary>
/// Builds the generated source text for typed option settings properties.
/// </summary>
internal interface IOptionsSettingsSourceBuilder
{
    /// <summary>
    /// Creates the source file content that augments the runtime options settings type.
    /// </summary>
    /// <param name="optionsSettings">Resolved options settings type symbol.</param>
    /// <param name="specs">Normalized option specifications to emit.</param>
    /// <returns>Generated C# source text.</returns>
    string BuildSource(INamedTypeSymbol optionsSettings, IReadOnlyList<OptionSettingSpec> specs);
}
