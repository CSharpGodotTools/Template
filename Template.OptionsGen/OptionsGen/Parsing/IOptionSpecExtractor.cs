using Microsoft.CodeAnalysis;

namespace Template.OptionsGen;

/// <summary>
/// Extracts normalized option specifications from syntax contexts produced by the generator pipeline.
/// </summary>
internal interface IOptionSpecExtractor
{
    /// <summary>
    /// Attempts to create an option specification from a generator syntax context.
    /// </summary>
    /// <param name="context">Syntax context containing node and semantic information.</param>
    /// <param name="spec">Extracted option specification when successful.</param>
    /// <returns><see langword="true"/> when extraction succeeds; otherwise <see langword="false"/>.</returns>
    bool TryCreateSpec(GeneratorSyntaxContext context, out OptionSettingSpec? spec);
}
