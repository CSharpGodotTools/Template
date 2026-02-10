using GdUnit4;
using static GdUnit4.Assertions;
using System;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

[TestSuite]
public class ENetTests
{
    private static readonly TimeSpan _connectTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _packetTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan _batchTimeout = TimeSpan.FromSeconds(10);

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ServerClientConnects()
    {
        TestOutput.Header(nameof(ServerClientConnects));

        await using ENetTestHarness<CPacketNestedCollections> harness = new((_, _) => { });
        TestOutput.Step("Connecting client/server");
        bool connected = await harness.ConnectAsync(_connectTimeout);
        AssertBool(connected).IsTrue();
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketNestedCollections_To_Server()
    {
        TestOutput.Header(nameof(Client_Sends_PacketNestedCollections_To_Server));

        CPacketNestedCollections expected = PacketNestedCollectionsFactory.CreateSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketNestedCollections_To_Server_Deep()
    {
        TestOutput.Header(nameof(Client_Sends_PacketNestedCollections_To_Server_Deep));

        CPacketNestedCollections expected = PacketNestedCollectionsFactory.CreateDeepSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketPrimitives_To_Server()
    {
        TestOutput.Header(nameof(Client_Sends_PacketPrimitives_To_Server));

        CPacketPrimitives expected = PacketPrimitivesFactory.CreateSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketPrimitives_To_Server_Deep()
    {
        TestOutput.Header(nameof(Client_Sends_PacketPrimitives_To_Server_Deep));

        CPacketPrimitives expected = PacketPrimitivesFactory.CreateDeepSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_Packet_Batches()
    {
        TestOutput.Header(nameof(Client_Sends_Packet_Batches));

        await PacketBatchRunner.RunAsync(
            PacketPrimitivesFactory.CreateSample,
            100,
            _connectTimeout,
            _batchTimeout);

        await PacketBatchRunner.RunAsync(
            PacketNestedCollectionsFactory.CreateSample,
            20,
            _connectTimeout,
            _batchTimeout);
    }
}
