using PacketGen.Generators.PacketGeneration;
using System.Linq;

namespace PacketGen.Generators;

/// <summary>
/// Composes final generated packet source text from analyzed model artifacts.
/// </summary>
/// <param name="constructorBuilder">Builder for generated constructors.</param>
/// <param name="deepObjectHelperBuilder">Builder for deep equality/hash helper methods.</param>
internal sealed class PacketSourceComposer(PacketConstructorBuilder constructorBuilder, PacketDeepObjectHelperBuilder deepObjectHelperBuilder)
{
    private const int HashSeedValue = 17;
    private const int HashMultiplierSecondary = 31;

    private readonly PacketConstructorBuilder _constructorBuilder = constructorBuilder;
    private readonly PacketDeepObjectHelperBuilder _deepObjectHelperBuilder = deepObjectHelperBuilder;

    /// <summary>
    /// Builds the full partial class source for a packet type.
    /// </summary>
    /// <param name="model">Packet generation model.</param>
    /// <param name="artifacts">Generated write/read/equality/hash artifacts.</param>
    /// <returns>Complete generated source text.</returns>
    public string Compose(PacketGenerationModel model, PacketGenerationArtifacts artifacts)
    {
        string usings = string.Join("\n", artifacts.Namespaces.OrderBy(static ns => ns).Select(ns => $"using {ns};"));
        const string indent8 = "        ";
        const string indent12 = "            ";

        // Preserve per-property comments while generating early-return equality checks.
        string equalsChecks = artifacts.EqualsLines.Count == 0
            ? $"{indent8}return true;"
            // Emit generated source with per-property early-return equality if checks.
            // Each generated equality clause includes an if guard for early return.
            : string.Join("\n\n", artifacts.EqualsLines.Select(line =>
                // Build generated source that emits one if guard per equality expression.
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
        // Fast path for reference-equal packet instances.
        if (object.ReferenceEquals(this, obj))
            return true;

        // Equality requires matching generated packet type.
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
