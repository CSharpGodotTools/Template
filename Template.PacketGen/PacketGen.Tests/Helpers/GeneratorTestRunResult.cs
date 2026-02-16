using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PacketGen.Tests;

internal sealed record GeneratorTestRunResult(
    string GeneratedSource,
    string GeneratedFile,
    ImmutableArray<string> References,
    string TestSource,
    ImmutableArray<Diagnostic> GeneratorDiagnostics
);
