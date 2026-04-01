using System.Collections.Immutable;

namespace PacketGen.Tests;

/// <summary>
/// Immutable options payload for configuring generator test runs.
/// </summary>
/// <param name="GeneratedFile">Expected generated file name.</param>
/// <param name="Sources">Source texts used to build test compilation.</param>
/// <param name="References">Metadata reference file paths.</param>
/// <param name="TrustedPlatformAssemblyNames">Trusted platform assembly names to include.</param>
internal sealed record GeneratorTestOptions(
    string GeneratedFile,
    ImmutableArray<string> Sources,
    ImmutableArray<string> References,
    ImmutableArray<string> TrustedPlatformAssemblyNames
);
