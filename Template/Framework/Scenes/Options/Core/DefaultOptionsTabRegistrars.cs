using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

internal static class DefaultOptionsTabRegistrars
{
    public static IEnumerable<IOptionsTabRegistrar> Create()
    {
        yield return new OptionsGeneralTab();
        yield return new OptionsDisplayTab();
        yield return new OptionsGraphicsTab();
        yield return new OptionsAudioTab();
    }
}