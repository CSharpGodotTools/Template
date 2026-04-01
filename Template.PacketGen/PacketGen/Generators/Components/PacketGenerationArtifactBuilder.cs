using Microsoft.CodeAnalysis;
using PacketGen.Generators.Emitters;
using PacketGen.Generators.PacketGeneration;
using PacketGen.Generators.TypeHandlers;
using System.Collections.Generic;
using System.Linq;

namespace PacketGen.Generators;

/// <summary>
/// Builds write/read/equality/hash generation artifacts for packet source composition.
/// </summary>
/// <param name="registryFactory">Factory for type-handler registries used by emitters.</param>
internal sealed class PacketGenerationArtifactBuilder(PacketTypeHandlerRegistryFactory registryFactory)
{
    private readonly PacketTypeHandlerRegistryFactory _registryFactory = registryFactory;

    /// <summary>
    /// Produces all generation artifacts for a packet model.
    /// </summary>
    /// <param name="compilation">Current Roslyn compilation.</param>
    /// <param name="model">Packet generation model.</param>
    /// <param name="packetFrameworkNamespace">Optional runtime packet namespace.</param>
    /// <returns>Complete artifact set for source composition.</returns>
    public PacketGenerationArtifacts Build(Compilation compilation, PacketGenerationModel model, string? packetFrameworkNamespace)
    {
        HashSet<string> namespaces = [];

        // Include framework namespace when packet runtime APIs are externally scoped.
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

        // Structural comparer APIs require generic collections namespace support.
        if (needsEqualityComparer)
            namespaces.Add("System.Collections.Generic");

        // Deep helpers require runtime and non-generic collection namespaces.
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

/// <summary>
/// Immutable container of generated packet source fragments and helper requirements.
/// </summary>
/// <param name="namespaces">Namespaces required by generated source.</param>
/// <param name="writeLines">Write-method body lines.</param>
/// <param name="readLines">Read-method body lines.</param>
/// <param name="equalsLines">Equality-expression lines and metadata.</param>
/// <param name="hashLines">Hash-code expression lines and metadata.</param>
/// <param name="needsDeepEquals">Whether deep-equality helper methods are required.</param>
/// <param name="needsDeepHash">Whether deep-hash helper methods are required.</param>
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
