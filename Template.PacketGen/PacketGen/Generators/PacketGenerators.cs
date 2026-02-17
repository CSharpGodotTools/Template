using Microsoft.CodeAnalysis;
using PacketGen.Generators;

namespace PacketGen;

internal static class PacketGenerators
{
    private static readonly PacketGenerationOrchestrator _orchestrator = new();

    public static string? GetSource(Compilation compilation, INamedTypeSymbol symbol)
    {
        return _orchestrator.GenerateSource(compilation, symbol);
    }
}
