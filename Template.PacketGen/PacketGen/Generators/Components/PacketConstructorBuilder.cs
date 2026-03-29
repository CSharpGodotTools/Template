using PacketGen.Generators.PacketGeneration;
using System.Linq;

namespace PacketGen.Generators;

internal sealed class PacketConstructorBuilder
{
    public string Build(PacketGenerationModel model)
    {
        const string indent4 = "    ";
        const string indent8 = "        ";

        var refTypeProps = model.Properties.Where(p => !p.Type.IsValueType).ToList();
        string emptyConstructor;
        if (refTypeProps.Count == 0)
        {
            emptyConstructor = $"{indent4}public {model.ClassName}() {{ }}";
        }
        else
        {
            string nullInits = string.Join("\n", refTypeProps.Select(p => $"{indent8}{p.Name} = null!;"));
            emptyConstructor = $"{indent4}public {model.ClassName}()\n{indent4}{{\n{nullInits}\n{indent4}}}";
        }

        string paramList = string.Join(", ", model.Properties.Select(p =>
            $"{p.Type.ToDisplayString()} {ToCamelCase(p.Name)}"));

        string assignments = string.Join("\n", model.Properties.Select(p =>
            $"{indent8}{p.Name} = {ToCamelCase(p.Name)};"));

        string paramsConstructor =
            $"{indent4}public {model.ClassName}({paramList})\n" +
            $"{indent4}{{\n" +
            $"{assignments}\n" +
            $"{indent4}}}";

        return $"{emptyConstructor}\n\n{paramsConstructor}\n";
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}
