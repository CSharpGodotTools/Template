#if DEBUG
using System;

namespace GodotUtils.Debugging;

/// <summary>
/// Resolves duplicate dictionary keys for visualization edits.
/// </summary>
internal interface IVisualDictionaryKeyResolver
{
    /// <summary>
    /// Attempts to resolve a key rename when the target key is already used.
    /// </summary>
    /// <param name="currentKey">Current key value.</param>
    /// <param name="duplicateKey">Incoming key that collides.</param>
    /// <param name="keyType">Expected key type.</param>
    /// <param name="containsKey">Key existence predicate.</param>
    /// <param name="resolvedKey">Resolved key when successful.</param>
    /// <returns><see langword="true"/> when a replacement key is found.</returns>
    bool TryResolveRenamedKey(object currentKey, object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey);

    /// <summary>
    /// Attempts to resolve a new key when a duplicate is detected.
    /// </summary>
    /// <param name="duplicateKey">Incoming key that collides.</param>
    /// <param name="keyType">Expected key type.</param>
    /// <param name="containsKey">Key existence predicate.</param>
    /// <param name="resolvedKey">Resolved key when successful.</param>
    /// <returns><see langword="true"/> when a replacement key is found.</returns>
    bool TryResolveAddedKey(object duplicateKey, Type keyType, Func<object, bool> containsKey, out object resolvedKey);
}
#endif
