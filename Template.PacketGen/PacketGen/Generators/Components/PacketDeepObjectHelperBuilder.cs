namespace PacketGen.Generators;

/// <summary>
/// Builds optional deep equality and deep hash helper methods for generated packets.
/// </summary>
internal sealed class PacketDeepObjectHelperBuilder
{
    /// <summary>
    /// Builds deep-equality helper methods when requested.
    /// </summary>
    /// <param name="include">Whether helpers should be emitted.</param>
    /// <returns>Deep-equality helper source, or empty string.</returns>
    public string BuildDeepEqualsHelper(bool include)
    {
        // Skip helper emission when deep equality support is not requested.
        if (!include)
            return string.Empty;

        return """

    /// <summary>
    /// Compares two values recursively, including arrays, dictionaries, and sequences.
    /// </summary>
    /// <param name="left">First value.</param>
    /// <param name="right">Second value.</param>
    /// <returns><see langword="true"/> when both values are structurally equal.</returns>

    private static bool DeepEquals(object? left, object? right)
    {
        // Fast path when both references already point to the same object.
        if (object.ReferenceEquals(left, right))
            return true;

        // Null mismatch means values are not equal.
        if (left is null || right is null)
            return false;

        // Compare strings directly to avoid treating them as IEnumerable<char>.
        if (left is string && right is string)
            return string.Equals((string)left, (string)right);

        // One string and one non-string cannot be equal.
        if (left is string || right is string)
            return false;

        // Compare dictionaries key-by-key and value-by-value.
        if (left is IDictionary leftDict && right is IDictionary rightDict)
            return DictionaryEquals(leftDict, rightDict);

        // Compare arrays with dedicated array equality logic.
        if (left is Array leftArray && right is Array rightArray)
            return ArrayEquals(leftArray, rightArray);

        // Compare other enumerable sequences in order.
        if (left is IEnumerable leftEnumerable && right is IEnumerable rightEnumerable)
            return SequenceEquals(leftEnumerable, rightEnumerable);

        return left.Equals(right);
    }

    /// <summary>
    /// Compares two arrays using structural equality for simple element types and recursive sequence logic otherwise.
    /// </summary>
    /// <param name="left">First array.</param>
    /// <param name="right">Second array.</param>
    /// <returns><see langword="true"/> when the arrays are structurally equal.</returns>
    private static bool ArrayEquals(Array left, Array right)
    {
        // Arrays with different lengths are never equal.
        if (left.Length != right.Length)
            return false;

        Type? elementType = left.GetType().GetElementType();

        // Use structural comparer for primitive-like arrays to avoid per-item recursion.
        if (elementType != null && elementType != typeof(object) && !typeof(IEnumerable).IsAssignableFrom(elementType))
            return StructuralEqualityComparer.Equals(left, right);

        return SequenceEquals(left, right);
    }

    /// <summary>
    /// Compares two dictionaries by key set and recursively compared values.
    /// </summary>
    /// <param name="left">First dictionary.</param>
    /// <param name="right">Second dictionary.</param>
    /// <returns><see langword="true"/> when both dictionaries contain equal entries.</returns>
    private static bool DictionaryEquals(IDictionary left, IDictionary right)
    {
        // Dictionaries with different sizes cannot be equal.
        if (left.Count != right.Count)
            return false;

        foreach (DictionaryEntry entry in left)
        {
            // Missing key on the right means dictionaries differ.
            if (!right.Contains(entry.Key))
                return false;

            // Compare values recursively for matching keys.
            if (!DeepEquals(entry.Value, right[entry.Key]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Compares two enumerables item-by-item using recursive deep equality.
    /// </summary>
    /// <param name="left">First sequence.</param>
    /// <param name="right">Second sequence.</param>
    /// <returns><see langword="true"/> when both sequences are equal in length and content.</returns>
    private static bool SequenceEquals(IEnumerable left, IEnumerable right)
    {
        // Short-circuit when both collections expose different counts.
        if (left is ICollection leftCollection && right is ICollection rightCollection && leftCollection.Count != rightCollection.Count)
            return false;

        IEnumerator leftEnumerator = left.GetEnumerator();
        IEnumerator rightEnumerator = right.GetEnumerator();

        while (true)
        {
            bool leftNext = leftEnumerator.MoveNext();
            bool rightNext = rightEnumerator.MoveNext();

            // Sequence length mismatch detected.
            if (leftNext != rightNext)
                return false;

            // Both enumerators reached end at the same time.
            if (!leftNext)
                return true;

            // Compare each current element recursively.
            if (!DeepEquals(leftEnumerator.Current, rightEnumerator.Current))
                return false;
        }
    }
""";
    }

    /// <summary>
    /// Builds deep-hash helper methods when requested.
    /// </summary>
    /// <param name="include">Whether helpers should be emitted.</param>
    /// <param name="hashSeedValue">Seed value for hash accumulation.</param>
    /// <param name="hashMultiplierSecondary">Multiplier used for hash mixing.</param>
    /// <returns>Deep-hash helper source, or empty string.</returns>
    public string BuildDeepHashHelper(bool include, int hashSeedValue, int hashMultiplierSecondary)
    {
        // Skip helper emission when deep hash support is not requested.
        if (!include)
            return string.Empty;

        return $$"""

    /// <summary>
    /// Computes a deep hash code for a value, including collection contents.
    /// </summary>
    /// <param name="value">Value to hash.</param>
    /// <returns>Deep hash code for the provided value.</returns>

    private static int DeepHash(object? value)
    {
        // Null contributes zero to composite hashes.
        if (value is null)
            return 0;

        // Hash strings directly to avoid sequence hashing behavior.
        if (value is string)
            return value.GetHashCode();

        // Hash dictionaries in a key/value aware manner.
        if (value is IDictionary dict)
            return DictionaryHash(dict);

        // Hash arrays with array-aware logic.
        if (value is Array array)
            return ArrayHash(array);

        // Hash other enumerables item-by-item.
        if (value is IEnumerable enumerable)
            return SequenceHash(enumerable);

        return value.GetHashCode();
    }

    /// <summary>
    /// Computes a hash code for an array using structural or recursive sequence hashing.
    /// </summary>
    /// <param name="array">Array to hash.</param>
    /// <returns>Hash code for the array contents.</returns>
    private static int ArrayHash(Array array)
    {
        Type? elementType = array.GetType().GetElementType();

        // Use structural hash for primitive-like arrays when possible.
        if (elementType != null && elementType != typeof(object) && !typeof(IEnumerable).IsAssignableFrom(elementType))
            return StructuralEqualityComparer.GetHashCode(array);

        return SequenceHash(array);
    }

    /// <summary>
    /// Computes an order-insensitive hash for dictionary entries by combining key and value hashes.
    /// </summary>
    /// <param name="dict">Dictionary to hash.</param>
    /// <returns>Hash code for dictionary contents.</returns>
    private static int DictionaryHash(IDictionary dict)
    {
        int hash = dict.Count;

        foreach (DictionaryEntry entry in dict)
        {
            int entryHash = {{hashSeedValue}};
            entryHash = (entryHash * {{hashMultiplierSecondary}}) ^ DeepHash(entry.Key);
            entryHash = (entryHash * {{hashMultiplierSecondary}}) ^ DeepHash(entry.Value);
            hash ^= entryHash;
        }

        return hash;
    }

    /// <summary>
    /// Computes a rolling hash across sequence items using deep item hashing.
    /// </summary>
    /// <param name="sequence">Sequence to hash.</param>
    /// <returns>Hash code for sequence contents.</returns>
    private static int SequenceHash(IEnumerable sequence)
    {
        int hash = {{hashSeedValue}};

        foreach (object item in sequence)
        {
            hash = (hash * {{hashMultiplierSecondary}}) ^ DeepHash(item);
        }

        return hash;
    }
""";
    }
}
