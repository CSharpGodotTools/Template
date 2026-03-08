using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace PacketGen.Generators.PacketGeneration;

/// <summary>
/// Shared state threaded through all <see cref="ITypeHandler"/> emit calls for a single property.
/// </summary>
internal sealed class GenerationContext(
    Compilation compilation,
    IPropertySymbol property,
    ITypeSymbol type,
    List<string> outputLines,
    HashSet<string> namespaces)
{
    /// <summary>The Roslyn compilation, used for type lookups.</summary>
    public Compilation Compilation { get; } = compilation;
    /// <summary>The packet property being serialized.</summary>
    public IPropertySymbol Property { get; } = property;
    /// <summary>The resolved type of the property (may differ from <c>Property.Type</c> for nested elements).</summary>
    public ITypeSymbol Type { get; } = type;
    /// <summary>Accumulates the emitted source lines for the current Write/Read method body.</summary>
    public List<string> OutputLines { get; } = outputLines;
    /// <summary>Collects additional <c>using</c> namespaces required by the emitted code.</summary>
    public HashSet<string> Namespaces { get; } = namespaces;
}

/// <summary>Context passed to <see cref="ITypeHandler.EmitWrite"/>.</summary>
internal sealed class WriteContext(GenerationContext shared)
{
    /// <summary>Shared generation state.</summary>
    public GenerationContext Shared { get; } = shared;
}

/// <summary>Context passed to <see cref="ITypeHandler.EmitRead"/>.</summary>
internal sealed class ReadContext(GenerationContext shared, string targetExpression)
{
    /// <summary>Shared generation state.</summary>
    public GenerationContext Shared { get; } = shared;
    /// <summary>Left-hand-side expression to assign the deserialized value to.</summary>
    public string TargetExpression { get; } = targetExpression;
}
