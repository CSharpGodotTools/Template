using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PacketGen.Generators.PacketGeneration;

internal sealed class PacketGenerationModel(
    INamedTypeSymbol packetSymbol,
    ImmutableArray<IPropertySymbol> properties,
    bool hasWriteReadMethods,
    string namespaceName,
    string className)
{
    public INamedTypeSymbol PacketSymbol { get; } = packetSymbol;
    public ImmutableArray<IPropertySymbol> Properties { get; } = properties;
    public bool HasWriteReadMethods { get; } = hasWriteReadMethods;
    public string NamespaceName { get; } = namespaceName;
    public string ClassName { get; } = className;
}
