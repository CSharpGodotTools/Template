using GdUnit4;
using System;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

namespace Template.Setup.Testing;

[TestSuite]
/// <summary>
/// Integration tests that validate ENet connectivity and packet round trips.
/// </summary>
public class ENetTests
{
    private static readonly TimeSpan _connectTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _packetTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan _batchTimeout = TimeSpan.FromSeconds(10);
    private const bool SuppressBatchLogs = true;

    /// <summary>
    /// Verifies that the test server and client establish a connection.
    /// </summary>
    /// <returns>A task that completes when the connection assertion has run.</returns>
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

    /// <summary>
    /// Verifies nested-collection packet round trip using baseline fixture data.
    /// </summary>
    /// <returns>A task that completes when the round-trip assertion has run.</returns>
    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketNestedCollections_To_Server()
    {
        TestOutput.Header(nameof(Client_Sends_PacketNestedCollections_To_Server));

        CPacketNestedCollections expected = PacketNestedCollectionsFactory.CreateSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    /// <summary>
    /// Verifies nested-collection packet round trip using deep fixture data.
    /// </summary>
    /// <returns>A task that completes when the round-trip assertion has run.</returns>
    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketNestedCollections_To_Server_Deep()
    {
        TestOutput.Header(nameof(Client_Sends_PacketNestedCollections_To_Server_Deep));

        CPacketNestedCollections expected = PacketNestedCollectionsFactory.CreateDeepSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    /// <summary>
    /// Verifies primitive packet round trip using baseline fixture data.
    /// </summary>
    /// <returns>A task that completes when the round-trip assertion has run.</returns>
    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketPrimitives_To_Server()
    {
        TestOutput.Header(nameof(Client_Sends_PacketPrimitives_To_Server));

        CPacketPrimitives expected = PacketPrimitivesFactory.CreateSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    /// <summary>
    /// Verifies primitive packet round trip using alternate fixture data.
    /// </summary>
    /// <returns>A task that completes when the round-trip assertion has run.</returns>
    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketPrimitives_To_Server_Deep()
    {
        TestOutput.Header(nameof(Client_Sends_PacketPrimitives_To_Server_Deep));

        CPacketPrimitives expected = PacketPrimitivesFactory.CreateDeepSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    /// <summary>
    /// Verifies struct-based packet round trip behavior.
    /// </summary>
    /// <returns>A task that completes when the round-trip assertion has run.</returns>
    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketStructTypes_To_Server()
    {
        TestOutput.Header(nameof(Client_Sends_PacketStructTypes_To_Server));

        CPacketStructTypes expected = PacketStructTypesFactory.CreateSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    /// <summary>
    /// Verifies class-based packet round trip behavior.
    /// </summary>
    /// <returns>A task that completes when the round-trip assertion has run.</returns>
    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_PacketClassTypes_To_Server()
    {
        TestOutput.Header(nameof(Client_Sends_PacketClassTypes_To_Server));

        CPacketClassTypes expected = PacketClassTypesFactory.CreateSample();
        await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
    }

    /// <summary>
    /// Verifies reliable receipt for burst packet sends across supported packet types.
    /// </summary>
    /// <returns>A task that completes when all burst-send assertions have run.</returns>
    [TestCase]
    [RequireGodotRuntime]
    public static async Task Client_Sends_Packet_Batches()
    {
        TestOutput.Header(nameof(Client_Sends_Packet_Batches));

        await PacketBatchRunner.RunAsync(
            PacketPrimitivesFactory.CreateSample,
            100,
            _connectTimeout,
            _batchTimeout,
            suppressLogs: SuppressBatchLogs);

        await PacketBatchRunner.RunAsync(
            PacketNestedCollectionsFactory.CreateSample,
            20,
            _connectTimeout,
            _batchTimeout,
            suppressLogs: SuppressBatchLogs);
    }
}
