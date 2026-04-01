using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PacketGen.Generators.PacketGeneration;

/// <summary>
/// Immutable model describing a single packet class, built by <see cref="PacketAnalysis"/>
/// and consumed by <see cref="PacketGenerationOrchestrator"/> to emit source code.
/// </summary>
/// <param name="packetSymbol">Roslyn symbol for the packet class.</param>
/// <param name="properties">Serializable packet properties included in generation.</param>
/// <param name="hasWriteReadMethods">Whether user code already defines Write/Read methods.</param>
/// <param name="namespaceName">Packet class namespace.</param>
/// <param name="className">Packet class simple name.</param>
internal sealed class PacketGenerationModel(
    INamedTypeSymbol packetSymbol,
    ImmutableArray<IPropertySymbol> properties,
    bool hasWriteReadMethods,
    string namespaceName,
    string className)
{
    /// <summary>The Roslyn symbol for the packet class itself.</summary>
    public INamedTypeSymbol PacketSymbol { get; } = packetSymbol;
    /// <summary>All serializable properties on the packet (excludes <c>[NetExclude]</c> properties).</summary>
    public ImmutableArray<IPropertySymbol> Properties { get; } = properties;
    /// <summary><c>true</c> if the user's partial class already defines <c>Write</c> or <c>Read</c> — generation is skipped.</summary>
    public bool HasWriteReadMethods { get; } = hasWriteReadMethods;
    /// <summary>Fully-qualified namespace of the packet class.</summary>
    public string NamespaceName { get; } = namespaceName;
    /// <summary>Simple class name of the packet.</summary>
    public string ClassName { get; } = className;
}
