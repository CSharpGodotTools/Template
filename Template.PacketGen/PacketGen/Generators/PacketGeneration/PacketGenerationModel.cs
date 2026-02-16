using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PacketGen.Generators.PacketGeneration;

internal sealed class PacketGenerationModel
{
    public PacketGenerationModel(
        INamedTypeSymbol packetSymbol,
        ImmutableArray<IPropertySymbol> properties,
        bool hasWriteReadMethods,
        string namespaceName,
        string className)
    {
        PacketSymbol = packetSymbol;
        Properties = properties;
        HasWriteReadMethods = hasWriteReadMethods;
        NamespaceName = namespaceName;
        ClassName = className;
    }

    public INamedTypeSymbol PacketSymbol { get; }
    public ImmutableArray<IPropertySymbol> Properties { get; }
    public bool HasWriteReadMethods { get; }
    public string NamespaceName { get; }
    public string ClassName { get; }
}
