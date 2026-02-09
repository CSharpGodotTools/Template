using GdUnit4;
using static GdUnit4.Assertions;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

[TestSuite]
public class ENetTests
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PacketTimeout = TimeSpan.FromSeconds(60);

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ServerClientConnects()
    {
        await using ENetTestHarness harness = new((_, _) => { });
        bool connected = await harness.ConnectAsync(ConnectTimeout);
        AssertBool(connected).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ClientSendsPacketNestedCollectionsToServer()
    {
        CPacketNestedCollections expected = PacketNestedCollectionsFactory.CreateSample();
        await RunPacketRoundTripAsync(expected);
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ClientSendsPacketNestedCollectionsToServer_Deep()
    {
        CPacketNestedCollections expected = PacketNestedCollectionsFactory.CreateDeepSample();
        await RunPacketRoundTripAsync(expected);
    }

    private static async Task RunPacketRoundTripAsync(CPacketNestedCollections expected)
    {
        PacketCapture<CPacketNestedCollections> capture = new();

        await using ENetTestHarness harness = new((packet, _) => capture.Set(packet));
        bool connected = await harness.ConnectAsync(ConnectTimeout);
        AssertBool(connected).IsTrue();

        harness.Send(expected);

        Console.WriteLine("[Test] Waiting for packet capture...");
        PacketWaitDiagnostics waitDiagnostics = await WaitForPacketAsync(capture, harness, PacketTimeout);
        if (!waitDiagnostics.Received)
        {
            string diagnostic = BuildTimeoutDiagnostics(expected, harness, waitDiagnostics);
            throw new TimeoutException(diagnostic);
        }
        AssertBool(waitDiagnostics.Received).IsTrue();

        if (waitDiagnostics.Received)
        {
            string diff = PacketDiff.FindFirstDiff(expected, capture.Packet);
            if (diff != null)
            {
                Console.WriteLine(diff);
                throw new Exception(diff);
            }
            AssertBool(diff == null).IsTrue();
        }
    }

    private sealed class PacketWaitDiagnostics
    {
        public bool Received { get; set; }
        public bool SawOutgoingEnqueue { get; set; }
        public bool SawOutgoingDrain { get; set; }
        public int LastOutgoingCount { get; set; }
        public int LastGodotPacketCount { get; set; }
        public int LastCommandCount { get; set; }
        public bool ClientRunning { get; set; }
        public bool ClientConnected { get; set; }
        public bool ServerRunning { get; set; }
    }

    private static async Task<PacketWaitDiagnostics> WaitForPacketAsync(
        PacketCapture<CPacketNestedCollections> capture,
        ENetTestHarness harness,
        TimeSpan timeout)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        PacketWaitDiagnostics diagnostics = new();

        while (stopwatch.Elapsed < timeout)
        {
            harness.Client.HandlePackets();

            if (capture.IsSet)
            {
                diagnostics.Received = true;
                return diagnostics;
            }

            diagnostics.ClientRunning = harness.Client.IsRunning;
            diagnostics.ClientConnected = harness.Client.IsConnected;
            diagnostics.ServerRunning = harness.Server.IsRunning;
            diagnostics.LastOutgoingCount = harness.Client.OutgoingCount;
            diagnostics.LastGodotPacketCount = harness.Client.GodotPacketCount;
            diagnostics.LastCommandCount = harness.Client.CommandCount;

            if (diagnostics.LastOutgoingCount > 0)
            {
                diagnostics.SawOutgoingEnqueue = true;
            }
            else if (diagnostics.SawOutgoingEnqueue)
            {
                diagnostics.SawOutgoingDrain = true;
            }

            await Task.Delay(10);
        }

        return diagnostics;
    }

    private static string BuildTimeoutDiagnostics(
        CPacketNestedCollections expected,
        ENetTestHarness harness,
        PacketWaitDiagnostics waitDiagnostics)
    {
        string sizeInfo = GetPacketSizeInfo(expected);
        string registryInfo = GetPacketRegistryInfo(typeof(CPacketNestedCollections));

        return
            $"Timed out after {PacketTimeout.TotalSeconds:0.##}s waiting for packet capture " +
            $"({nameof(CPacketNestedCollections)}). " +
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

    private static string GetPacketSizeInfo(CPacketNestedCollections expected)
    {
        try
        {
            expected.Write();
            long size = expected.GetSize();
            bool exceeds = size > Framework.Netcode.GamePacket.MaxSize;
            return $"PacketSize={size} MaxSize={Framework.Netcode.GamePacket.MaxSize} ExceedsMax={exceeds}.";
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
            Type registryType = typeof(Framework.Netcode.PacketRegistry);
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
