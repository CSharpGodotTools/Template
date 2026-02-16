using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace PacketGen;

[Generator(LanguageNames.CSharp)]
public sealed class PacketGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<INamedTypeSymbol> packetSymbols = GetPacketSymbols(context);
        IncrementalValuesProvider<INamedTypeSymbol> clientPackets = packetSymbols.Where(static s => InheritsFrom(s, "ClientPacket"));
        IncrementalValuesProvider<INamedTypeSymbol> serverPackets = packetSymbols.Where(static s => InheritsFrom(s, "ServerPacket"));

        IncrementalValueProvider<Compilation> compilation = context.CompilationProvider;

        // Per-packet script source generation
        GenerateSourceForPacketScripts(context, packetSymbols, compilation);

        // Discover [PacketRegistry] attributes
        IncrementalValuesProvider<INamedTypeSymbol> registryClass = FindPacketRegistryAttributes(context);

        // Registry generation (gated by attribute)
        IncrementalValuesProvider<((INamedTypeSymbol Left, ImmutableArray<INamedTypeSymbol> Right) Left, ImmutableArray<INamedTypeSymbol> Right)> registryInput = registryClass
            .Combine(clientPackets.Collect())
            .Combine(serverPackets.Collect());

        GeneratePacketRegistryClass(context, registryInput);
    }

    private static IncrementalValuesProvider<INamedTypeSymbol> GetPacketSymbols(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax cds && cds.BaseList is not null,
                transform: static (ctx, _) =>
                {
                    ClassDeclarationSyntax syntax = (ClassDeclarationSyntax)ctx.Node;
                    return ctx.SemanticModel.GetDeclaredSymbol(syntax) as INamedTypeSymbol;
                })
            .Where(static symbol => symbol is not null && IsPacketType(symbol))
            .Select(static (symbol, _) => symbol!);
    }

    private static void GenerateSourceForPacketScripts(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<INamedTypeSymbol> packetSymbols, IncrementalValueProvider<Compilation> compilation)
    {
        context.RegisterSourceOutput(
            packetSymbols.Combine(compilation),
            static (spc, pair) =>
            {
                Logger.Init(spc);

                INamedTypeSymbol symbol = pair.Left;
                Compilation compilationValue = pair.Right;

                string? source =
                    PacketGenerators.GetSource(compilationValue, symbol);

                if (source is not null)
                {
                    spc.AddSource(BuildHintName(symbol), source);
                }
            });
    }

    /// <summary>
    /// Generates the PacketRegistry.g.cs script.
    /// </summary>
    private static void GeneratePacketRegistryClass(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<((INamedTypeSymbol Left, ImmutableArray<INamedTypeSymbol> Right) Left, ImmutableArray<INamedTypeSymbol> Right)> registryInput)
    {
        context.RegisterSourceOutput(registryInput,
            static (spc, triple) =>
            {
                Logger.Init(spc);

                INamedTypeSymbol registrySymbol = triple.Left.Left;
                ImmutableArray<INamedTypeSymbol> clients = triple.Left.Right;
                ImmutableArray<INamedTypeSymbol> servers = triple.Right;

                string opcodePacketTypeName = GetPacketSizeTypeName(registrySymbol);

                string source = PacketRegistryGenerator.GetSource(registrySymbol, opcodePacketTypeName,
                    [.. clients],
                    [.. servers]);

                spc.AddSource(BuildHintName(registrySymbol), source);
            });
    }

    /// <summary>
    /// Finds all [PacketRegistry] attributes in the assembly.
    /// </summary>
    private static IncrementalValuesProvider<INamedTypeSymbol> FindPacketRegistryAttributes(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) =>
                node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
            transform: static (ctx, _) =>
            {
                ClassDeclarationSyntax syntax = (ClassDeclarationSyntax)ctx.Node;

                if (ctx.SemanticModel.GetDeclaredSymbol(syntax) is not INamedTypeSymbol symbol)
                {
                    return null;
                }

                foreach (AttributeData attribute in symbol.GetAttributes())
                {
                    if (attribute.AttributeClass?.Name == "PacketRegistryAttribute")
                    {
                        return symbol;
                    }
                }

                return null;
            })
            .Where(static s => s is not null)
            .Select(static (s, _) => s!);
    }

    private static bool IsPacketType(INamedTypeSymbol symbol)
    {
        return InheritsFrom(symbol, "ClientPacket") || InheritsFrom(symbol, "ServerPacket");
    }

    private static bool InheritsFrom(INamedTypeSymbol symbol, string baseTypeName)
    {
        INamedTypeSymbol? current = symbol.BaseType;
        while (current is not null)
        {
            if (current.Name == baseTypeName)
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static string BuildHintName(INamedTypeSymbol symbol)
    {
        string fullName = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        StringBuilder hintBuilder = new(fullName.Length + 5);

        foreach (char c in fullName)
        {
            if (char.IsLetterOrDigit(c))
            {
                hintBuilder.Append(c);
            }
            else
            {
                hintBuilder.Append('_');
            }
        }

        hintBuilder.Append(".g.cs");
        return hintBuilder.ToString();
    }

    /// <summary>
    /// Gets the type that defines the packet size from the [PacketRegistry] attribute.
    /// For example if it's [PacketRegistry(typeof(ushort))] then "ushort" would be returned.
    /// </summary>
    private static string GetPacketSizeTypeName(INamedTypeSymbol registrySymbol)
    {
        foreach (AttributeData attribute in registrySymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name != "PacketRegistryAttribute")
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 1)
            {
                TypedConstant arg = attribute.ConstructorArguments[0];
                if (arg.Kind == TypedConstantKind.Type && arg.Value is ITypeSymbol typeSymbol)
                {
                    return typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                }
            }
        }

        return "byte";
    }
}
