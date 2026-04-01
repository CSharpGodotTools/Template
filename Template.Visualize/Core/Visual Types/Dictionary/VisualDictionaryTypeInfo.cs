#if DEBUG
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Describes dictionary types and defaults for visualization.
/// </summary>
/// <param name="DictionaryType">Concrete dictionary runtime type.</param>
/// <param name="KeyType">Key type.</param>
/// <param name="ValueType">Value type.</param>
/// <param name="UseStableDisplayOrder">Whether to preserve entry order.</param>
/// <param name="DefaultKey">Default key value.</param>
/// <param name="DefaultValue">Default value.</param>
internal readonly record struct VisualDictionaryTypeInfo(
    Type DictionaryType,
    Type KeyType,
    Type ValueType,
    bool UseStableDisplayOrder,
    object DefaultKey,
    object DefaultValue);
#endif
