using PacketGen.Generators.PacketGeneration;
using System.Linq;

namespace PacketGen.Generators;

internal sealed class PacketSourceComposer
{
    private const int HashSeedValue = 17;
    private const int HashMultiplierSecondary = 31;

    private readonly PacketConstructorBuilder _constructorBuilder;
    private readonly PacketDeepObjectHelperBuilder _deepObjectHelperBuilder;

    public PacketSourceComposer(PacketConstructorBuilder constructorBuilder, PacketDeepObjectHelperBuilder deepObjectHelperBuilder)
    {
        _constructorBuilder = constructorBuilder;
        _deepObjectHelperBuilder = deepObjectHelperBuilder;
    }

    public string Compose(PacketGenerationModel model, PacketGenerationArtifacts artifacts)
    {
        string usings = string.Join("\n", artifacts.Namespaces.OrderBy(static ns => ns).Select(ns => $"using {ns};"));
        const string indent8 = "        ";
        const string indent12 = "            ";

        string equalsChecks = artifacts.EqualsLines.Count == 0
            ? $"{indent8}return true;"
            : string.Join("\n\n", artifacts.EqualsLines.Select(line =>
                $"{indent8}// {line.Comment}\n{indent8}if (!{line.Expression})\n{indent12}return false;"));

        string constructors = _constructorBuilder.Build(model);
        string deepEqualsHelper = _deepObjectHelperBuilder.BuildDeepEqualsHelper(artifacts.NeedsDeepEquals);
        string deepHashHelper = _deepObjectHelperBuilder.BuildDeepHashHelper(
            artifacts.NeedsDeepHash,
            HashSeedValue,
            HashMultiplierSecondary);

        return $$"""
#nullable enable
{{usings}}

namespace {{model.NamespaceName}};

public partial class {{model.ClassName}}
{
{{constructors}}
    public override void Write(PacketWriter writer)
    {
{{string.Join("\n", artifacts.WriteLines.Select(line => indent8 + line))}}
    }

    public override void Read(PacketReader reader)
    {
{{string.Join("\n", artifacts.ReadLines.Select(line => indent8 + line))}}
    }

    public override bool Equals(object? obj)
    {
        if (object.ReferenceEquals(this, obj))
            return true;

        if (obj is not {{model.ClassName}} other)
            return false;

{{equalsChecks}}

        return true;
    }

    public override int GetHashCode()
    {
        int hash = {{HashSeedValue}};

{{string.Join("\n", artifacts.HashLines.Select(line => indent8 + line.Expression))}}

        return hash;
    }
{{deepEqualsHelper}}
{{deepHashHelper}}
}

""";
    }
}
