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

    /// <summary>
    /// Creates an orchestrator with default generation components.
    /// </summary>
    public PacketGenerationOrchestrator()
        : this(
            new PacketFrameworkNamespaceResolver(),
            new PacketGenerationArtifactBuilder(new PacketTypeHandlerRegistryFactory()),
            new PacketSourceComposer(new PacketConstructorBuilder(), new PacketDeepObjectHelperBuilder()))
    {
    }

    /// <summary>
    /// Creates an orchestrator with explicit component dependencies.
    /// </summary>
    /// <param name="namespaceResolver">Resolver for packet framework namespace.</param>
    /// <param name="artifactBuilder">Builder for generated write/read/equality artifacts.</param>
    /// <param name="sourceComposer">Composer for final packet source text.</param>
    internal PacketGenerationOrchestrator(
        PacketFrameworkNamespaceResolver namespaceResolver,
        PacketGenerationArtifactBuilder artifactBuilder,
        PacketSourceComposer sourceComposer)
    {
        _namespaceResolver = namespaceResolver;
        _artifactBuilder = artifactBuilder;
        _sourceComposer = sourceComposer;
    }

    /// <summary>
    /// Generates partial packet source for a packet type when generation prerequisites are met.
    /// </summary>
    /// <param name="compilation">Current Roslyn compilation.</param>
    /// <param name="symbol">Packet type symbol being generated.</param>
    /// <returns>Generated source text, or null when generation is skipped.</returns>
    public string? GenerateSource(Compilation compilation, INamedTypeSymbol symbol)
    {
        PacketGenerationModel model = PacketAnalysis.Analyze(symbol);

        // Skip packets that already implement write/read or expose no serializable properties.
        if (model.HasWriteReadMethods || model.Properties.Length == 0)
            return null;

        string? packetFrameworkNamespace = _namespaceResolver.Resolve(symbol);
        PacketGenerationArtifacts artifacts = _artifactBuilder.Build(compilation, model, packetFrameworkNamespace);
        return _sourceComposer.Compose(model, artifacts);
    }
}
