#if DEBUG
using System;

namespace GodotUtils.Debugging;

internal interface IVisualDictionaryKeyResolver
{
    bool TryResolveRenamedKey(object currentKey, object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey);

    bool TryResolveAddedKey(object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey);
}
#endif
