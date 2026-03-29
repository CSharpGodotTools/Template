using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

internal static partial class DefaultOptionsTabRegistrars
{
    public static IEnumerable<IOptionsTabRegistrar> Create()
    {
        List<IOptionsTabRegistrar> registrars = [];
        AddDefaultTabRegistrars(registrars);
        return registrars;
    }

    static partial void AddDefaultTabRegistrars(List<IOptionsTabRegistrar> registrars);
}
