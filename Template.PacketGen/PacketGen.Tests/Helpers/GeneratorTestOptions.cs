using System.Collections.Immutable;

namespace PacketGen.Tests;

internal sealed record GeneratorTestOptions(
    string GeneratedFile,
    ImmutableArray<string> Sources,
    ImmutableArray<string> References,
    ImmutableArray<string> TrustedPlatformAssemblyNames
);
