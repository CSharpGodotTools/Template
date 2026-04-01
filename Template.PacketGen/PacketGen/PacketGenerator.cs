using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PacketGen.Generators;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace PacketGen;

/// <summary>
/// Incremental source generator for packet serializers and packet registry metadata.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class PacketGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Registers incremental generation pipelines for per-packet source and the registry class.
    /// </summary>
    /// <param name="context">Generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<INamedTypeSymbol> packetSymbols = GetPacketSymbols(context);
        IncrementalValuesProvider<INamedTypeSymbol> clientPackets = packetSymbols.Where(static s => InheritsFrom(s, PacketGenConstants.ClientPacketTypeName));
        IncrementalValuesProvider<INamedTypeSymbol> serverPackets = packetSymbols.Where(static s => InheritsFrom(s, PacketGenConstants.ServerPacketTypeName));

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

    /// <summary>
    /// Builds a provider that yields all <see cref="ClientPacket"/> and <see cref="ServerPacket"/> subclasses in the compilation.
    /// </summary>
    /// <param name="context">Generator initialization context.</param>
    /// <returns>Incremental provider for discovered packet symbols.</returns>
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

    /// <summary>
    /// Registers per-packet source generation: emits a <c>.g.cs</c> partial class for each packet symbol.
    /// </summary>
    /// <param name="context">Generator initialization context.</param>
    /// <param name="packetSymbols">Provider of discovered packet symbols.</param>
    /// <param name="compilation">Current compilation provider.</param>
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

                // Emit source only when generator returned non-null content.
                if (source is not null)
                {
                    spc.AddSource(BuildHintName(symbol), source);
                }
            });
    }

    /// <summary>
    /// Generates the PacketRegistry.g.cs script.
    /// </summary>
    /// <param name="context">Generator initialization context.</param>
    /// <param name="registryInput">Combined registry symbol and packet collections.</param>
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
    /// <param name="context">Generator initialization context.</param>
    /// <returns>Provider of classes marked with <c>PacketRegistryAttribute</c>.</returns>
    private static IncrementalValuesProvider<INamedTypeSymbol> FindPacketRegistryAttributes(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) =>
                node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
            transform: static (ctx, _) =>
            {
                ClassDeclarationSyntax syntax = (ClassDeclarationSyntax)ctx.Node;

                // Ignore classes without a semantic type symbol.
                if (ctx.SemanticModel.GetDeclaredSymbol(syntax) is not INamedTypeSymbol symbol)
                {
                    return null;
                }

                foreach (AttributeData attribute in symbol.GetAttributes())
                {
                    // Keep only classes marked with PacketRegistryAttribute.
                    if (attribute.AttributeClass?.Name == PacketGenConstants.PacketRegistryAttributeTypeName)
                    {
                        return symbol;
                    }
                }

                return null;
            })
            .Where(static s => s is not null)
            .Select(static (s, _) => s!);
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="symbol"/> inherits from a class named <paramref name="baseTypeName"/>.
    /// </summary>
    /// <param name="symbol">Candidate symbol.</param>
    /// <param name="baseTypeName">Base type name to match.</param>
    /// <returns>True when symbol inherits from the named base type.</returns>
    private static bool IsPacketType(INamedTypeSymbol symbol)
    {
        return InheritsFrom(symbol, PacketGenConstants.ClientPacketTypeName)
            || InheritsFrom(symbol, PacketGenConstants.ServerPacketTypeName);
    }

    /// <summary>
    /// Walks the base-type chain checking for a class with the given name.
    /// </summary>
    /// <param name="symbol">Candidate symbol.</param>
    /// <param name="baseTypeName">Base type name to match.</param>
    /// <returns>True when the base chain contains the named type.</returns>
    private static bool InheritsFrom(INamedTypeSymbol symbol, string baseTypeName)
    {
        INamedTypeSymbol? current = symbol.BaseType;
        while (current is not null)
        {
            // Match by base-type name while walking inheritance chain.
            if (current.Name == baseTypeName)
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Converts a fully-qualified type name to a safe file hint name by replacing non-alphanumeric characters with underscores.
    /// </summary>
    /// <param name="symbol">Type symbol for the generated source file.</param>
    /// <returns>Safe Roslyn hint name ending in <c>.g.cs</c>.</returns>
    private static string BuildHintName(INamedTypeSymbol symbol)
    {
        string fullName = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        StringBuilder hintBuilder = new(fullName.Length + 5);

        foreach (char c in fullName)
        {
            // Preserve alphanumeric characters for deterministic hint names.
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
    /// <param name="registrySymbol">Packet-registry class symbol.</param>
    /// <returns>Packet-size type name, defaulting to <c>byte</c>.</returns>
    private static string GetPacketSizeTypeName(INamedTypeSymbol registrySymbol)
    {
        foreach (AttributeData attribute in registrySymbol.GetAttributes())
        {
            // Ignore attributes other than PacketRegistryAttribute.
            if (attribute.AttributeClass?.Name != PacketGenConstants.PacketRegistryAttributeTypeName)
            {
                continue;
            }

            // Expect a single constructor argument containing the opcode backing type.
            if (attribute.ConstructorArguments.Length == 1)
            {
                TypedConstant arg = attribute.ConstructorArguments[0];

                // Return minimally-qualified type name when argument is a type symbol.
                if (arg.Kind == TypedConstantKind.Type && arg.Value is ITypeSymbol typeSymbol)
                {
                    return typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                }
            }
        }

        return "byte";
    }
}
