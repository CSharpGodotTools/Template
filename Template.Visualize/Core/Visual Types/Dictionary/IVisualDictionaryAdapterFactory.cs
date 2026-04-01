#if DEBUG
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Factory for creating dictionary adapters.
/// </summary>
internal interface IVisualDictionaryAdapterFactory
{
    /// <summary>
    /// Creates an adapter for the provided dictionary instance and type.
    /// </summary>
    /// <param name="dictionaryObject">Dictionary instance.</param>
    /// <param name="dictionaryType">Dictionary runtime type.</param>
    /// <returns>Adapter for the dictionary.</returns>
    IVisualDictionaryAdapter Create(object dictionaryObject, Type dictionaryType);
}
#endif
