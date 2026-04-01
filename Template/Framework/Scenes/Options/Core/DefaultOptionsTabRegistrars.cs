using System.Collections.Generic;

namespace __TEMPLATE__.Ui;

/// <summary>
/// Creates default tab registrar instances for the options UI.
/// </summary>
internal static partial class DefaultOptionsTabRegistrars
{
    /// <summary>
    /// Builds the default registrar collection.
    /// </summary>
    /// <returns>Registrar sequence used to register default option tabs.</returns>
    public static IEnumerable<IOptionsTabRegistrar> Create()
    {
        List<IOptionsTabRegistrar> registrars = [];
        AddDefaultTabRegistrars(registrars);
        return registrars;
    }

    /// <summary>
    /// Adds framework-specific default tab registrars to the provided list.
    /// </summary>
    /// <param name="registrars">Destination list for registrar instances.</param>
    static partial void AddDefaultTabRegistrars(List<IOptionsTabRegistrar> registrars);
}
