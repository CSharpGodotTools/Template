namespace PacketGen.Generators;

internal sealed class PacketDeepObjectHelperBuilder
{
    public string BuildDeepEqualsHelper(bool include)
    {
        if (!include)
            return string.Empty;

        return """

    private static bool DeepEquals(object? left, object? right)
    {
        if (object.ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        if (left is string && right is string)
            return string.Equals((string)left, (string)right);

        if (left is string || right is string)
            return false;

        if (left is IDictionary leftDict && right is IDictionary rightDict)
            return DictionaryEquals(leftDict, rightDict);

        if (left is Array leftArray && right is Array rightArray)
            return ArrayEquals(leftArray, rightArray);

        if (left is IEnumerable leftEnumerable && right is IEnumerable rightEnumerable)
            return SequenceEquals(leftEnumerable, rightEnumerable);

        return left.Equals(right);
    }

    private static bool ArrayEquals(Array left, Array right)
    {
        if (left.Length != right.Length)
            return false;

        Type? elementType = left.GetType().GetElementType();
        if (elementType != null && elementType != typeof(object) && !typeof(IEnumerable).IsAssignableFrom(elementType))
            return StructuralEqualityComparer.Equals(left, right);

        return SequenceEquals(left, right);
    }

    private static bool DictionaryEquals(IDictionary left, IDictionary right)
    {
        if (left.Count != right.Count)
            return false;

        foreach (DictionaryEntry entry in left)
        {
            if (!right.Contains(entry.Key))
                return false;

            if (!DeepEquals(entry.Value, right[entry.Key]))
                return false;
        }

        return true;
    }

    private static bool SequenceEquals(IEnumerable left, IEnumerable right)
    {
        if (left is ICollection leftCollection && right is ICollection rightCollection && leftCollection.Count != rightCollection.Count)
            return false;

        IEnumerator leftEnumerator = left.GetEnumerator();
        IEnumerator rightEnumerator = right.GetEnumerator();

        while (true)
        {
            bool leftNext = leftEnumerator.MoveNext();
            bool rightNext = rightEnumerator.MoveNext();

            if (leftNext != rightNext)
                return false;

            if (!leftNext)
                return true;

            if (!DeepEquals(leftEnumerator.Current, rightEnumerator.Current))
                return false;
        }
    }
""";
    }

    public string BuildDeepHashHelper(bool include, int hashSeedValue, int hashMultiplierSecondary)
    {
        if (!include)
            return string.Empty;

        return $$"""

    private static int DeepHash(object? value)
    {
        if (value is null)
            return 0;

        if (value is string)
            return value.GetHashCode();

        if (value is IDictionary dict)
            return DictionaryHash(dict);

        if (value is Array array)
            return ArrayHash(array);

        if (value is IEnumerable enumerable)
            return SequenceHash(enumerable);

        return value.GetHashCode();
    }

    private static int ArrayHash(Array array)
    {
        Type? elementType = array.GetType().GetElementType();
        if (elementType != null && elementType != typeof(object) && !typeof(IEnumerable).IsAssignableFrom(elementType))
            return StructuralEqualityComparer.GetHashCode(array);

        return SequenceHash(array);
    }

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
