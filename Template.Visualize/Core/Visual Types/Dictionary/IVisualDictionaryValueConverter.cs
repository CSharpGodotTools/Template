#if DEBUG
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Converts dictionary values to the expected runtime type.
/// </summary>
internal interface IVisualDictionaryValueConverter
{
    /// <summary>
    /// Converts a value to the expected runtime type when needed.
    /// </summary>
    /// <param name="value">Incoming value.</param>
    /// <param name="expectedType">Expected runtime type.</param>
    /// <returns>Converted value.</returns>
    object? ConvertToExpectedType(object? value, Type expectedType);
}
#endif
