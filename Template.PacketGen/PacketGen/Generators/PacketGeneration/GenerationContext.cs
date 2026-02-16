using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace PacketGen.Generators.PacketGeneration;

internal sealed class GenerationContext(
    Compilation compilation,
    IPropertySymbol property,
    ITypeSymbol type,
    List<string> outputLines,
    HashSet<string> namespaces)
{
    public Compilation Compilation { get; } = compilation;
    public IPropertySymbol Property { get; } = property;
    public ITypeSymbol Type { get; } = type;
    public List<string> OutputLines { get; } = outputLines;
    public HashSet<string> Namespaces { get; } = namespaces;
}

internal sealed class WriteContext(GenerationContext shared)
{
    public GenerationContext Shared { get; } = shared;
}

internal sealed class ReadContext(GenerationContext shared, string targetExpression)
{
    public GenerationContext Shared { get; } = shared;
    public string TargetExpression { get; } = targetExpression;
}
