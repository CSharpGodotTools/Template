#if DEBUG
using System;

namespace GodotUtils.Debugging;

internal interface IVisualDictionaryAdapterFactory
{
    IVisualDictionaryAdapter Create(object dictionaryObject, Type dictionaryType);
}
#endif
