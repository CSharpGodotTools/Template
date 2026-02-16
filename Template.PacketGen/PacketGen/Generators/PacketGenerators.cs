using Microsoft.CodeAnalysis;
using PacketGen.Generators;

namespace PacketGen;

internal static class PacketGenerators
{
    private static readonly PacketGenerationOrchestrator Orchestrator = new();

    public static string? GetSource(Compilation compilation, INamedTypeSymbol symbol)
    {
        return Orchestrator.GenerateSource(compilation, symbol);
    }
}
