using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Generic;

namespace PacketGen.Generators.Emitters;

/// <summary>
/// Contract for generating write statements for a packet property/type.
/// </summary>
internal interface IWriteGenerator
{
    /// <summary>
    /// Emits write statements for a value expression.
    /// </summary>
    /// <param name="ctx">Generation context for current property/type.</param>
    /// <param name="valueExpression">Expression to serialize.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    void Generate(GenerationContext ctx, string valueExpression, string indent);
}

/// <summary>
/// Contract for generating read statements for a packet property/type.
/// </summary>
internal interface IReadGenerator
{
    /// <summary>
    /// Emits read statements for a target expression.
    /// </summary>
    /// <param name="ctx">Generation context for current property/type.</param>
    /// <param name="targetExpression">Expression receiving deserialized value.</param>
    /// <param name="indent">Indentation prefix for generated lines.</param>
    void Generate(GenerationContext ctx, string targetExpression, string indent);
}

/// <summary>
/// Generated equality check line metadata.
/// </summary>
/// <param name="comment">Human-readable property/type label.</param>
/// <param name="expression">Equality expression to emit.</param>
/// <param name="usesDeepEquality">Whether expression relies on deep-equality helpers.</param>
internal readonly struct EqualityLine(string comment, string expression, bool usesDeepEquality)
{
    /// <summary>
    /// Human-readable property/type label.
    /// </summary>
    public string Comment { get; } = comment;

    /// <summary>
    /// Equality expression to emit.
    /// </summary>
    public string Expression { get; } = expression;

    /// <summary>
    /// Indicates whether deep-equality helpers are required.
    /// </summary>
    public bool UsesDeepEquality { get; } = usesDeepEquality;
}

/// <summary>
/// Contract for generating equality lines for packet properties.
/// </summary>
internal interface IEqualityGenerator
{
    /// <summary>
    /// Appends equality lines for a packet property.
    /// </summary>
    /// <param name="equalsLines">Destination collection of equality lines.</param>
    /// <param name="property">Property being emitted.</param>
    void Generate(List<EqualityLine> equalsLines, IPropertySymbol property);
}

/// <summary>
/// Generated hash-code line metadata.
/// </summary>
/// <param name="expression">Hash expression to emit.</param>
/// <param name="usesDeepHash">Whether expression relies on deep-hash helpers.</param>
internal readonly struct HashLine(string expression, bool usesDeepHash)
{
    /// <summary>
    /// Hash expression to emit.
    /// </summary>
    public string Expression { get; } = expression;

    /// <summary>
    /// Indicates whether deep-hash helpers are required.
    /// </summary>
    public bool UsesDeepHash { get; } = usesDeepHash;
}

/// <summary>
/// Contract for generating hash-code lines for packet properties.
/// </summary>
internal interface IHashGenerator
{
    /// <summary>
    /// Appends hash-code lines for a packet property.
    /// </summary>
    /// <param name="hashLines">Destination collection of hash lines.</param>
    /// <param name="property">Property being emitted.</param>
    /// <param name="namespaces">Namespace set for generated source.</param>
    void Generate(List<HashLine> hashLines, IPropertySymbol property, HashSet<string> namespaces);
}
