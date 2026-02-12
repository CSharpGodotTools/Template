using System;
using System.Collections;
using System.Reflection;
using Framework.Netcode;

namespace Template.Setup.Testing;

public static class PacketTimeoutDiagnostics
{
    public static string Build<TPacket>(
        TPacket expected,
        ENetTestHarness<TPacket> harness,
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
            $"ServerRunning={waitDiagnostics.ServerRunning}, " +
            $"OutgoingCount={waitDiagnostics.LastOutgoingCount}, " +
            $"OutgoingEnqueued={waitDiagnostics.SawOutgoingEnqueue}, " +
            $"OutgoingDrained={waitDiagnostics.SawOutgoingDrain}, " +
            $"GodotPacketCount={waitDiagnostics.LastGodotPacketCount}, " +
            $"CommandCount={waitDiagnostics.LastCommandCount}, " +
            $"PeerId={harness.Client.PeerId}. " +
            $"{sizeInfo} {registryInfo}";
    }

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

    private static string DescribeRegistryCollection(
        Type registryType,
        string memberName,
        Type packetType,
        bool checkValues)
    {
        object value = GetStaticMemberValue(registryType, memberName);
        if (value is null)
        {
            return $"{memberName}=<missing>.";
        }

        if (value is IDictionary dictionary)
        {
            bool containsType = false;
            if (checkValues)
            {
                foreach (object entryValue in dictionary.Values)
                {
                    if (ReferenceEquals(entryValue, packetType) || Equals(entryValue, packetType))
                    {
                        containsType = true;
                        break;
                    }
                }
            }
            else
            {
                containsType = dictionary.Contains(packetType);
            }

            return $"{memberName}.Count={dictionary.Count} ContainsType={containsType}.";
        }

        return $"{memberName}=<{value.GetType().Name}>.";
    }

    private static object GetStaticMemberValue(Type type, string memberName)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        PropertyInfo prop = type.GetProperty(memberName, flags);
        if (prop != null)
        {
            return prop.GetValue(null);
        }

        FieldInfo field = type.GetField(memberName, flags);
        if (field != null)
        {
            return field.GetValue(null);
        }

        return null;
    }
}
