using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

public static class PacketRegistryGenerator
{
    public static string GetSource(
        INamedTypeSymbol registryClassSymbol,
        string idTypeName,
        List<INamedTypeSymbol> clientSymbols,
        List<INamedTypeSymbol> serverSymbols)
    {
        // Sort symbols by their display strings to ensure opcodes are always in a deterministic order
        clientSymbols = [.. clientSymbols.OrderBy(s => s.ToDisplayString())];
        serverSymbols = [.. serverSymbols.OrderBy(s => s.ToDisplayString())];

        int clientOpcode = 0;
        int serverOpcode = 0;

        var clientEntries = new List<string>();
        var serverEntries = new List<string>();

        var namespaces = new HashSet<string>();

        // Process client packets
        foreach (var symbol in clientSymbols)
        {
            if (clientOpcode > byte.MaxValue)
                throw new InvalidOperationException("Client packet opcode overflow");

            string typeName = symbol.Name;

            clientEntries.Add($@"
            {{
                typeof({typeName}),
                new PacketInfo<ClientPacket>
                {{
                    Opcode = {clientOpcode},
                    Instance = new {typeName}()
                }}
            }}"
            );

            clientOpcode++;

            string namespaceName = symbol.ContainingNamespace.ToDisplayString();
            namespaces.Add(namespaceName);
        }

        // Process server packets
        foreach (var symbol in serverSymbols)
        {
            if (serverOpcode > byte.MaxValue)
                throw new InvalidOperationException("Server packet opcode overflow");

            string typeName = symbol.Name;

            serverEntries.Add($@"
            {{
                typeof({typeName}),
                new PacketInfo<ServerPacket>
                {{
                    Opcode = {serverOpcode},
                    Instance = new {typeName}()
                }}
            }}"
            );

            serverOpcode++;

            string namespaceName = symbol.ContainingNamespace.ToDisplayString();
            namespaces.Add(namespaceName);
        }

        var usings = string.Join("\n", namespaces.OrderBy(ns => ns).Select(ns => $"using {ns};"));

        string registryNamespace = registryClassSymbol.ContainingNamespace.ToDisplayString();
        string registryClassName = registryClassSymbol.Name;

        // Generate the source code
        string sourceCode = $$"""
using System;
using System.Collections.Generic;
using System.Linq;
{{usings}}
namespace {{registryNamespace}};

public partial class {{registryClassName}}
{
    public static readonly Dictionary<Type, PacketInfo<ClientPacket>> ClientPacketInfo;
    public static readonly Dictionary<{{idTypeName}}, Type> ClientPacketTypes;
    public static readonly Dictionary<Type, PacketInfo<ServerPacket>> ServerPacketInfo;
    public static readonly Dictionary<{{idTypeName}}, Type> ServerPacketTypes;

    static PacketRegistry()
    {
        ClientPacketInfo = new Dictionary<Type, PacketInfo<ClientPacket>>()
        {
            {{string.Join(",\n", clientEntries)}}
        };

        ClientPacketTypes = ClientPacketInfo.ToDictionary(kvp => kvp.Value.Opcode, kvp => kvp.Key);

        ServerPacketInfo = new Dictionary<Type, PacketInfo<ServerPacket>>()
        {
            {{string.Join(",\n", serverEntries)}}
        };

        ServerPacketTypes = ServerPacketInfo.ToDictionary(kvp => kvp.Value.Opcode, kvp => kvp.Key);
    }
}

public sealed class PacketInfo<T>
{
    public {{idTypeName}} Opcode;
    public T Instance;
}

""";
        return sourceCode;
    }
}
