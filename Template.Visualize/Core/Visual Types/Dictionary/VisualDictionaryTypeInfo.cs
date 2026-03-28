#if DEBUG
using System;

namespace GodotUtils.Debugging;

internal readonly record struct VisualDictionaryTypeInfo(
    Type DictionaryType,
    Type KeyType,
    Type ValueType,
    bool UseStableDisplayOrder,
    object DefaultKey,
    object DefaultValue);
#endif
