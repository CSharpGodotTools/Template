using Microsoft.CodeAnalysis;
using PacketGen.Generators.Emitters;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Generators.TypeHandlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PacketGen.Generators;

internal sealed class PacketGenerationOrchestrator
{
    public string? GenerateSource(Compilation compilation, INamedTypeSymbol symbol)
    {
        PacketGenerationModel model = PacketAnalysis.Analyze(symbol);

        if (model.HasWriteReadMethods || model.Properties.Length == 0)
            return null;

        HashSet<string> namespaces = ["Framework.Netcode"];
        List<string> writeLines = [];
        List<string> readLines = [];
        List<EqualityLine> equalsLines = [];
        List<HashLine> hashLines = [];

        TypeHandlerRegistry registry = BuildRegistry();

        IWriteGenerator writeGenerator = new WriteGenerator(registry);
        IReadGenerator readGenerator = new ReadGenerator(registry);
        IEqualityGenerator equalityGenerator = new EqualityGenerator();
        IHashGenerator hashGenerator = new HashGenerator();

        foreach (IPropertySymbol property in model.Properties)
        {
            GenerationContext shared = new(compilation, property, property.Type, writeLines, namespaces);
            writeGenerator.Generate(shared, property.Name, "");

            GenerationContext readShared = new(compilation, property, property.Type, readLines, namespaces);
            readGenerator.Generate(readShared, property.Name, "");

            equalityGenerator.Generate(equalsLines, property);
            hashGenerator.Generate(hashLines, property, namespaces);
        }

        bool needsDeepEquals = equalsLines.Any(line => line.UsesDeepEquality);
        bool needsDeepHash = hashLines.Any(line => line.UsesDeepHash);
        bool needsEqualityComparer = equalsLines.Any(line => !line.UsesDeepEquality)
            || hashLines.Any(line => !line.UsesDeepHash);

        if (needsEqualityComparer)
            namespaces.Add("System.Collections.Generic");

        if (needsDeepEquals || needsDeepHash)
        {
            namespaces.Add("System");
            namespaces.Add("System.Collections");
            namespaces.Add("static System.Collections.StructuralComparisons");
        }

        string usings = string.Join("\n", namespaces.Select(ns => $"using {ns};"));
        string indent8 = "        ";
        string indent12 = "            ";

        string equalsChecks = equalsLines.Count == 0
            ? $"{indent8}return true;"
            : string.Join("\n\n", equalsLines.Select(line =>
                $"{indent8}// {line.Comment}\n{indent8}if (!{line.Expression})\n{indent12}return false;"));

        string deepEqualsHelper = needsDeepEquals
            ? $$"""

    private static bool DeepEquals(object left, object right)
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

        var elementType = left.GetType().GetElementType();
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
"""
            : "";

        string deepHashHelper = needsDeepHash
            ? $$"""

    private static int DeepHash(object value)
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
        var elementType = array.GetType().GetElementType();
        if (elementType != null && elementType != typeof(object) && !typeof(IEnumerable).IsAssignableFrom(elementType))
            return StructuralEqualityComparer.GetHashCode(array);

        return SequenceHash(array);
    }

    private static int DictionaryHash(IDictionary dict)
    {
        int hash = dict.Count;

        foreach (DictionaryEntry entry in dict)
        {
            int entryHash = 17;
            entryHash = (entryHash * 31) ^ DeepHash(entry.Key);
            entryHash = (entryHash * 31) ^ DeepHash(entry.Value);
            hash ^= entryHash;
        }

        return hash;
    }

    private static int SequenceHash(IEnumerable sequence)
    {
        int hash = 17;

        foreach (object item in sequence)
        {
            hash = (hash * 31) ^ DeepHash(item);
        }

        return hash;
    }
"""
            : "";

        string sourceCode = $$"""
{{usings}}

namespace {{model.NamespaceName}};

public partial class {{model.ClassName}}
{
    public override void Write(PacketWriter writer)
    {
{{string.Join("\n", writeLines.Select(line => indent8 + line))}}
    }

    public override void Read(PacketReader reader)
    {
{{string.Join("\n", readLines.Select(line => indent8 + line))}}
    }

    public override bool Equals(object obj)
    {
        if (object.ReferenceEquals(this, obj))
            return true;

        if (obj is not {{model.ClassName}} other)
            return false;

{{equalsChecks}}

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 17;

{{string.Join("\n", hashLines.Select(line => indent8 + line.Expression))}}

        return hash;
    }
{{deepEqualsHelper}}
{{deepHashHelper}}
}

""";

        return sourceCode;
    }

    private static TypeHandlerRegistry BuildRegistry()
    {
        TypeHandlerRegistry registry = new();

        PrimitiveTypeHandler primitives = new();
        ArrayTypeHandler arrays = new(registry);
        ListTypeHandler lists = new(registry);
        DictionaryTypeHandler dictionaries = new(registry);
        ComplexTypeHandler complexTypes = new(registry);

#pragma warning disable IDE0300 // Simplify collection initialization
        registry.SetHandlers(new ITypeHandler[]
        {
            primitives,
            arrays,
            lists,
            dictionaries,
            complexTypes
        });
#pragma warning restore IDE0300 // Simplify collection initialization

        return registry;
    }
}
