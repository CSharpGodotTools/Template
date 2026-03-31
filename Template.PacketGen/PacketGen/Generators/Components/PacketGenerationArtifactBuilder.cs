using Microsoft.CodeAnalysis;
using PacketGen.Generators.Emitters;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Generators.TypeHandlers;
using System.Collections.Generic;
using System.Linq;

namespace PacketGen.Generators;

internal sealed class PacketGenerationArtifactBuilder(PacketTypeHandlerRegistryFactory registryFactory)
{
    private readonly PacketTypeHandlerRegistryFactory _registryFactory = registryFactory;

    public PacketGenerationArtifacts Build(Compilation compilation, PacketGenerationModel model, string? packetFrameworkNamespace)
    {
        HashSet<string> namespaces = [];
        if (packetFrameworkNamespace is not null)
            namespaces.Add(packetFrameworkNamespace);

        List<string> writeLines = [];
        List<string> readLines = [];
        List<EqualityLine> equalsLines = [];
        List<HashLine> hashLines = [];

        TypeHandlerRegistry registry = _registryFactory.Create();
        IWriteGenerator writeGenerator = new WriteGenerator(registry);
        IReadGenerator readGenerator = new ReadGenerator(registry);
        IEqualityGenerator equalityGenerator = new EqualityGenerator();
        IHashGenerator hashGenerator = new HashGenerator();

        foreach (IPropertySymbol property in model.Properties)
        {
            GenerationContext writeContext = new(compilation, property, property.Type, writeLines, namespaces);
            writeGenerator.Generate(writeContext, property.Name, "");

            GenerationContext readContext = new(compilation, property, property.Type, readLines, namespaces);
            readGenerator.Generate(readContext, property.Name, "");

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

        return new PacketGenerationArtifacts(
            namespaces,
            writeLines,
            readLines,
            equalsLines,
            hashLines,
            needsDeepEquals,
            needsDeepHash);
    }
}

internal sealed class PacketGenerationArtifacts(
    HashSet<string> namespaces,
    List<string> writeLines,
    List<string> readLines,
    List<EqualityLine> equalsLines,
    List<HashLine> hashLines,
    bool needsDeepEquals,
    bool needsDeepHash)
{
    public HashSet<string> Namespaces { get; } = namespaces;
    public List<string> WriteLines { get; } = writeLines;
    public List<string> ReadLines { get; } = readLines;
    public List<EqualityLine> EqualsLines { get; } = equalsLines;
    public List<HashLine> HashLines { get; } = hashLines;
    public bool NeedsDeepEquals { get; } = needsDeepEquals;
    public bool NeedsDeepHash { get; } = needsDeepHash;
}
