using Microsoft.CodeAnalysis;

namespace Template.OptionsGen;

internal interface IOptionSpecExtractor
{
    bool TryCreateSpec(GeneratorSyntaxContext context, out OptionSettingSpec? spec);
}
