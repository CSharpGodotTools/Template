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

internal readonly struct EqualityLine
{
    public EqualityLine(string comment, string expression, bool usesDeepEquality)
    {
        Comment = comment;
        Expression = expression;
        UsesDeepEquality = usesDeepEquality;
    }

    public string Comment { get; }
    public string Expression { get; }
    public bool UsesDeepEquality { get; }
}

internal interface IEqualityGenerator
{
    void Generate(List<EqualityLine> equalsLines, IPropertySymbol property);
}

internal readonly struct HashLine
{
    public HashLine(string expression, bool usesDeepHash)
    {
        Expression = expression;
        UsesDeepHash = usesDeepHash;
    }

    public string Expression { get; }
    public bool UsesDeepHash { get; }
}

internal interface IHashGenerator
{
    void Generate(List<HashLine> hashLines, IPropertySymbol property, HashSet<string> namespaces);
}
