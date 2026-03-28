#if DEBUG
using System;

namespace GodotUtils.Debugging;

internal interface IVisualDictionaryValueConverter
{
    object? ConvertToExpectedType(object? value, Type expectedType);
}
#endif
