using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;

namespace PacketGen.Generators;

/// <summary>
/// Coordinates packet source generation using composable, single-purpose components.
/// </summary>
internal sealed class PacketGenerationOrchestrator
{
    private readonly PacketFrameworkNamespaceResolver _namespaceResolver;
    private readonly PacketGenerationArtifactBuilder _artifactBuilder;
    private readonly PacketSourceComposer _sourceComposer;

    public PacketGenerationOrchestrator()
        : this(
            new PacketFrameworkNamespaceResolver(),
            new PacketGenerationArtifactBuilder(new PacketTypeHandlerRegistryFactory()),
            new PacketSourceComposer(new PacketConstructorBuilder(), new PacketDeepObjectHelperBuilder()))
    {
    }

    internal PacketGenerationOrchestrator(
        PacketFrameworkNamespaceResolver namespaceResolver,
        PacketGenerationArtifactBuilder artifactBuilder,
        PacketSourceComposer sourceComposer)
    {
        _namespaceResolver = namespaceResolver;
        _artifactBuilder = artifactBuilder;
        _sourceComposer = sourceComposer;
    }

    public string? GenerateSource(Compilation compilation, INamedTypeSymbol symbol)
    {
        PacketGenerationModel model = PacketAnalysis.Analyze(symbol);

        if (model.HasWriteReadMethods || model.Properties.Length == 0)
            return null;

        string? packetFrameworkNamespace = _namespaceResolver.Resolve(symbol);
        PacketGenerationArtifacts artifacts = _artifactBuilder.Build(compilation, model, packetFrameworkNamespace);
        return _sourceComposer.Compose(model, artifacts);
    }
}
