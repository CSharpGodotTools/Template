using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PacketGen.Generators;

/// <summary>
/// Generates PacketRegistry.g.cs containing deterministic opcode mappings for client and server packets.
/// </summary>
internal static class PacketRegistryGenerator
{
    /// <summary>
    /// Generates source code for the packet registry class with deterministic opcode assignments.
    /// </summary>
    /// <param name="registryClassSymbol">The symbol representing the [PacketRegistry] annotated class.</param>
    /// <param name="idTypeName">The type name to use for opcodes (e.g., "byte", "ushort").</param>
    /// <param name="clientSymbols">List of all ClientPacket-derived types.</param>
    /// <param name="serverSymbols">List of all ServerPacket-derived types.</param>
    /// <returns>Generated source code for the packet registry.</returns>
    public static string GetSource(
        INamedTypeSymbol registryClassSymbol,
        string idTypeName,
        List<INamedTypeSymbol> clientSymbols,
        List<INamedTypeSymbol> serverSymbols)
    {
        // Sort symbols by their display strings to ensure opcodes are always in a deterministic order
        clientSymbols = [.. clientSymbols.OrderBy(s => s.ToDisplayString())];
        serverSymbols = [.. serverSymbols.OrderBy(s => s.ToDisplayString())];

        int maxOpcode = idTypeName switch
        {
            "sbyte" => sbyte.MaxValue,
            "byte" => byte.MaxValue,
            "short" => short.MaxValue,
            "ushort" => ushort.MaxValue,
            _ => int.MaxValue
        };

        int clientOpcode = 0;
        int serverOpcode = 0;

        List<string> clientEntries = [];
        List<string> serverEntries = [];

        HashSet<string> namespaces = [];

        // Process client packets
        foreach (INamedTypeSymbol symbol in clientSymbols)
        {
            // The last opcode value is always reserved for packet fragmentation.
            if (clientOpcode >= maxOpcode)
                throw new InvalidOperationException($"Client packet opcode overflow (max assignable {maxOpcode - 1} for type '{idTypeName}', {maxOpcode} is reserved for fragmentation)");

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
        foreach (INamedTypeSymbol symbol in serverSymbols)
        {
            // The last opcode value is always reserved for packet fragmentation.
            if (serverOpcode >= maxOpcode)
                throw new InvalidOperationException($"Server packet opcode overflow (max assignable {maxOpcode - 1} for type '{idTypeName}', {maxOpcode} is reserved for fragmentation)");

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

        string usings = string.Join("\n", namespaces.OrderBy(ns => ns).Select(ns => $"using {ns};"));

        string registryNamespace = registryClassSymbol.ContainingNamespace.ToDisplayString();
        string registryClassName = registryClassSymbol.Name;

        // The last value is reserved as the fragment opcode and never assigned to a user packet.
        string fragmentOpcodeValue = maxOpcode.ToString();

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
    public static readonly Dictionary<ushort, Type> ClientPacketTypesWire;
    public static readonly Dictionary<Type, PacketInfo<ServerPacket>> ServerPacketInfo;
    public static readonly Dictionary<{{idTypeName}}, Type> ServerPacketTypes;
    public static readonly Dictionary<ushort, Type> ServerPacketTypesWire;

    /// <summary>
    /// Opcode reserved for packet fragmentation. Never assigned to a user-defined packet type.
    /// Always the maximum value of the configured opcode backing type.
    /// </summary>
    public const ushort FragmentOpcode = {{fragmentOpcodeValue}};

    static PacketRegistry()
    {
        ClientPacketInfo = new Dictionary<Type, PacketInfo<ClientPacket>>()
        {
            {{string.Join(",\n", clientEntries)}}
        };

        ClientPacketTypes = ClientPacketInfo.ToDictionary(kvp => kvp.Value.Opcode, kvp => kvp.Key);
        ClientPacketTypesWire = ClientPacketInfo.ToDictionary(kvp => (ushort)kvp.Value.Opcode, kvp => kvp.Key);

        ServerPacketInfo = new Dictionary<Type, PacketInfo<ServerPacket>>()
        {
            {{string.Join(",\n", serverEntries)}}
        };

        ServerPacketTypes = ServerPacketInfo.ToDictionary(kvp => kvp.Value.Opcode, kvp => kvp.Key);
        ServerPacketTypesWire = ServerPacketInfo.ToDictionary(kvp => (ushort)kvp.Value.Opcode, kvp => kvp.Key);
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
