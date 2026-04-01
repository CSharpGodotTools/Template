using Microsoft.CodeAnalysis;
using PacketGen.Generators;

namespace PacketGen;

/// <summary>
/// Entry point for packet source generation.
/// </summary>
internal static class PacketGenerators
{
    /// <summary>Shared orchestrator instance reused across generation calls.</summary>
    private static readonly PacketGenerationOrchestrator _orchestrator = new();

    /// <summary>
    /// Generates the partial class source for a single packet symbol, or <c>null</c> if generation
    /// is not applicable (e.g. the packet already defines its own Write/Read methods).
    /// </summary>
    /// <param name="compilation">Current Roslyn compilation.</param>
    /// <param name="symbol">Packet type symbol to generate.</param>
    /// <returns>Generated source, or null when generation is skipped.</returns>
    public static string? GetSource(Compilation compilation, INamedTypeSymbol symbol)
    {
        return _orchestrator.GenerateSource(compilation, symbol);
    }
}
