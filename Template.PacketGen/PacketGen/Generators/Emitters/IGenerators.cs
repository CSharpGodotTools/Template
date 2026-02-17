using Microsoft.CodeAnalysis;
using PacketGen.Generators.PacketGeneration;
using System.Collections.Generic;

namespace PacketGen.Generators.Emitters;

internal interface IWriteGenerator
{
    void Generate(GenerationContext ctx, string valueExpression, string indent);
}

internal interface IReadGenerator
{
    void Generate(GenerationContext ctx, string targetExpression, string indent);
}

internal readonly struct EqualityLine(string comment, string expression, bool usesDeepEquality)
{
    public string Comment { get; } = comment;
    public string Expression { get; } = expression;
    public bool UsesDeepEquality { get; } = usesDeepEquality;
}

internal interface IEqualityGenerator
{
    void Generate(List<EqualityLine> equalsLines, IPropertySymbol property);
}

internal readonly struct HashLine(string expression, bool usesDeepHash)
{
    public string Expression { get; } = expression;
    public bool UsesDeepHash { get; } = usesDeepHash;
}

internal interface IHashGenerator
{
    void Generate(List<HashLine> hashLines, IPropertySymbol property, HashSet<string> namespaces);
}
