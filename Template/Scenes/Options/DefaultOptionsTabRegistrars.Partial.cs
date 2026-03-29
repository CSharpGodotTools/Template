using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

internal static partial class DefaultOptionsTabRegistrars
{
    static partial void AddDefaultTabRegistrars(List<IOptionsTabRegistrar> registrars)
    {
        registrars.Add(new OptionsGeneralTab());
        registrars.Add(new OptionsDisplayTab());
        registrars.Add(new OptionsGraphicsTab());
        registrars.Add(new OptionsAudioTab());
    }
}
