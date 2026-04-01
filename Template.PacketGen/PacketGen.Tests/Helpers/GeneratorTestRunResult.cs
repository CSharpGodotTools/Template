using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PacketGen.Tests;

/// <summary>
/// Captures source-generator outputs and diagnostics for a test run.
/// </summary>
/// <param name="GeneratedSource">Generated source text.</param>
/// <param name="GeneratedFile">Expected generated file name.</param>
/// <param name="References">Metadata references used during compilation.</param>
/// <param name="TestSource">Primary test input source.</param>
/// <param name="GeneratorDiagnostics">Diagnostics emitted by the generator.</param>
internal sealed record GeneratorTestRunResult(
    string GeneratedSource,
    string GeneratedFile,
    ImmutableArray<string> References,
    string TestSource,
    ImmutableArray<Diagnostic> GeneratorDiagnostics
);
