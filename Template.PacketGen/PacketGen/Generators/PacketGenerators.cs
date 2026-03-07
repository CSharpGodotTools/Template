// Test Comment 2
using Microsoft.CodeAnalysis;
using PacketGen.Generators;

namespace PacketGen;

/// <summary>
/// Entry point for packet source generation.
/// </summary>
internal static class PacketGenerators
{
    private static readonly PacketGenerationOrchestrator _orchestrator = new();

    public static string? GetSource(Compilation compilation, INamedTypeSymbol symbol)
    {
        return _orchestrator.GenerateSource(compilation, symbol);
    }
}
