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

        // Process client packets
        foreach (INamedTypeSymbol symbol in clientSymbols)
        {
            // The last opcode value is always reserved for packet fragmentation.
            if (clientOpcode >= maxOpcode)
                throw new InvalidOperationException($"Client packet opcode overflow (max assignable {maxOpcode - 1} for type '{idTypeName}', {maxOpcode} is reserved for fragmentation)");

            // Use fully-qualified name to avoid ambiguity when two packets share a simple name across namespaces.
            string typeName = symbol.ToDisplayString();

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
        }

        // Process server packets
        foreach (INamedTypeSymbol symbol in serverSymbols)
        {
            // The last opcode value is always reserved for packet fragmentation.
            if (serverOpcode >= maxOpcode)
                throw new InvalidOperationException($"Server packet opcode overflow (max assignable {maxOpcode - 1} for type '{idTypeName}', {maxOpcode} is reserved for fragmentation)");

            // Use fully-qualified name to avoid ambiguity when two packets share a simple name across namespaces.
            string typeName = symbol.ToDisplayString();

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
        }

        string registryNamespace = registryClassSymbol.ContainingNamespace.ToDisplayString();
        string registryClassName = registryClassSymbol.Name;

        // The last value is reserved as the fragment opcode and never assigned to a user packet.
        string fragmentOpcodeValue = maxOpcode.ToString();

        // Helpers used by the runtime for type-aware opcode serialization.
        string readOpcodeExpression = idTypeName switch
        {
            "byte" => "(ushort)reader.ReadByte()",
            "sbyte" => "(ushort)(byte)reader.ReadSByte()",
            "short" => "(ushort)reader.ReadShort()",
            "ushort" => "reader.ReadUShort()",
            "int" => "(ushort)reader.ReadInt()",
            "uint" => "(ushort)reader.ReadUInt()",
            _ => "reader.ReadUShort()"
        };

        string isFragmentExpression = idTypeName switch
        {
            "byte" or "sbyte" =>
                "bytes.Length >= OpcodeSize && bytes[0] == unchecked((byte)FragmentOpcode)",
            "short" or "ushort" =>
                "bytes.Length >= OpcodeSize && (ushort)(bytes[0] | (bytes[1] << 8)) == unchecked((ushort)FragmentOpcode)",
            "int" or "uint" =>
                "bytes.Length >= OpcodeSize && unchecked((int)((uint)bytes[0] | ((uint)bytes[1] << 8) | ((uint)bytes[2] << 16) | ((uint)bytes[3] << 24))) == unchecked((int)FragmentOpcode)",
            _ =>
                "bytes.Length >= OpcodeSize && (ushort)(bytes[0] | (bytes[1] << 8)) == unchecked((ushort)FragmentOpcode)"
        };

        string writeFragmentStatements = idTypeName switch
        {
            "byte" or "sbyte" =>
                "destination[0] = unchecked((byte)FragmentOpcode);",
            "short" or "ushort" =>
                "destination[0] = unchecked((byte)FragmentOpcode);\n        destination[1] = unchecked((byte)((ushort)FragmentOpcode >> 8));",
            "int" or "uint" =>
                "destination[0] = unchecked((byte)(uint)FragmentOpcode);\n        destination[1] = unchecked((byte)((uint)FragmentOpcode >> 8));\n        destination[2] = unchecked((byte)((uint)FragmentOpcode >> 16));\n        destination[3] = unchecked((byte)((uint)FragmentOpcode >> 24));",
            _ =>
                "destination[0] = unchecked((byte)FragmentOpcode);\n        destination[1] = unchecked((byte)((ushort)FragmentOpcode >> 8));"
        };

        // Generate the source code
        return $$"""
using System;
using System.Collections.Generic;
using System.Linq;
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
    public const {{idTypeName}} FragmentOpcode = {{fragmentOpcodeValue}};

    /// <summary>Number of bytes occupied by one opcode on the wire.</summary>
    public const int OpcodeSize = sizeof({{idTypeName}});

    /// <summary>
    /// Reads one opcode from <paramref name="reader"/>, consuming exactly <see cref="OpcodeSize"/> bytes.
    /// Returns the value widened to ushort for lookup in <see cref="ClientPacketTypesWire"/> or <see cref="ServerPacketTypesWire"/>.
    /// </summary>
    /// <param name="reader">Packet reader positioned at an opcode boundary.</param>
    /// <returns>Decoded opcode value widened to <see cref="ushort"/>.</returns>
    public static ushort ReadOpcodeFromReader(PacketReader reader) => {{readOpcodeExpression}};

    /// <summary>Returns true when the leading <see cref="OpcodeSize"/> bytes of <paramref name="bytes"/> match <see cref="FragmentOpcode"/>.</summary>
    /// <param name="bytes">Packet bytes to inspect.</param>
    /// <returns><see langword="true"/> when the packet begins with the fragment opcode.</returns>
    public static bool IsFragmentHeader(byte[] bytes) =>
        {{isFragmentExpression}};

    /// <summary>Writes <see cref="FragmentOpcode"/> into <paramref name="destination"/> starting at byte offset 0.</summary>
    /// <param name="destination">Destination span receiving opcode bytes.</param>
    public static void WriteFragmentOpcodeToSpan(Span<byte> destination)
    {
        {{writeFragmentStatements}}
    }

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
    }
}
