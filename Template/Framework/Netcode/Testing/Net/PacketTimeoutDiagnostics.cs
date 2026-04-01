using __TEMPLATE__.Netcode;
using System;
using System.Collections;
using System.Reflection;

namespace Template.Setup.Testing;

/// <summary>
/// Builds detailed diagnostic text when packet capture times out in tests.
/// </summary>
public static class PacketTimeoutDiagnostics
{
    /// <summary>
    /// Builds a timeout message with runtime, packet-size, and registry metadata.
    /// </summary>
    /// <typeparam name="TPacket">Packet type that timed out.</typeparam>
    /// <param name="expected">Expected packet instance.</param>
    /// <param name="_">Active harness (reserved for future diagnostic expansion).</param>
    /// <param name="waitDiagnostics">Connection and server/client wait status.</param>
    /// <param name="timeout">Configured timeout that expired.</param>
    /// <returns>Formatted timeout diagnostic string.</returns>
    public static string Build<TPacket>(
        TPacket expected,
        ENetTestHarness<TPacket> _,
        PacketWaitDiagnostics waitDiagnostics,
        TimeSpan timeout)
        where TPacket : ClientPacket
    {
        string sizeInfo = GetPacketSizeInfo(expected);
        string registryInfo = GetPacketRegistryInfo(typeof(TPacket));

        return
            $"Timed out after {timeout.TotalSeconds:0.##}s waiting for packet capture " +
            $"({typeof(TPacket).Name}). " +
            $"ClientRunning={waitDiagnostics.ClientRunning}, " +
            $"ClientConnected={waitDiagnostics.ClientConnected}, " +
            $"ServerRunning={waitDiagnostics.ServerRunning}. " +
            $"{sizeInfo} {registryInfo}";
    }

    /// <summary>
    /// Computes packet size diagnostics and reports size-calculation failures.
    /// </summary>
    /// <param name="expected">Expected packet instance.</param>
    /// <returns>Packet size diagnostic segment.</returns>
    private static string GetPacketSizeInfo(ClientPacket expected)
    {
        try
        {
            expected.Write();
            long size = expected.GetSize();
            bool exceeds = size > GamePacket.MaxSize;
            return $"PacketSize={size} MaxSize={GamePacket.MaxSize} ExceedsMax={exceeds}.";
        }
        catch (Exception ex)
        {
            return $"PacketSizeError={ex.GetType().Name}:{ex.Message}.";
        }
    }

    /// <summary>
    /// Computes packet registry diagnostics for the specified packet type.
    /// </summary>
    /// <param name="packetType">Packet type under test.</param>
    /// <returns>Registry diagnostic segment.</returns>
    private static string GetPacketRegistryInfo(Type packetType)
    {
        try
        {
            Type registryType = typeof(PacketRegistry);
            string clientTypesInfo = DescribeRegistryCollection(registryType, "ClientPacketTypes", packetType, checkValues: true);
            string clientInfoInfo = DescribeRegistryCollection(registryType, "ClientPacketInfo", packetType, checkValues: false);
            return $"{clientTypesInfo} {clientInfoInfo}".Trim();
        }
        catch (Exception ex)
        {
            return $"PacketRegistryError={ex.GetType().Name}:{ex.Message}.";
        }
    }

    /// <summary>
    /// Describes whether a registry collection exists and contains packet metadata.
    /// </summary>
    /// <param name="registryType">Type that exposes registry members.</param>
    /// <param name="memberName">Registry member name to inspect.</param>
    /// <param name="packetType">Packet type being searched.</param>
    /// <param name="checkValues">Whether to search values instead of keys.</param>
    /// <returns>Collection presence/count/contains diagnostic segment.</returns>
    private static string DescribeRegistryCollection(
        Type registryType,
        string memberName,
        Type packetType,
        bool checkValues)
    {
        object? value = GetStaticMemberValue(registryType, memberName);

        // Report missing registry members explicitly for timeout triage.
        if (value is null)
        {
            return $"{memberName}=<missing>.";
        }

        // Dictionaries can report size and packet-type presence details.
        if (value is IDictionary dictionary)
        {
            bool containsType = false;

            // Some registries map opcode->type and require value scanning.
            if (checkValues)
            {
                // Some registries map opcode->type, so compare against values.
                foreach (object entryValue in dictionary.Values)
                {
                    // Stop once the expected packet type is found in values.
                    if (ReferenceEquals(entryValue, packetType) || Equals(entryValue, packetType))
                    {
                        containsType = true;
                        break;
                    }
                }
            }
            else
            {
                // Type-keyed registries can use direct dictionary key lookup.
                containsType = dictionary.Contains(packetType);
            }

            return $"{memberName}.Count={dictionary.Count} ContainsType={containsType}.";
        }

        return $"{memberName}=<{value.GetType().Name}>.";
    }

    /// <summary>
    /// Reads a static property or field value by name via reflection.
    /// </summary>
    /// <param name="type">Declaring type to inspect.</param>
    /// <param name="memberName">Static member name to resolve.</param>
    /// <returns>Member value when found; otherwise <see langword="null"/>.</returns>
    private static object? GetStaticMemberValue(Type type, string memberName)
    {
        const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        PropertyInfo? prop = type.GetProperty(memberName, Flags);

        // Prefer properties before fields when both names are available.
        if (prop != null)
        {
            return prop.GetValue(null);
        }

        FieldInfo? field = type.GetField(memberName, Flags);

        // Fall back to a field lookup for registry data containers.
        if (field != null)
        {
            return field.GetValue(null);
        }

        return null;
    }
}
