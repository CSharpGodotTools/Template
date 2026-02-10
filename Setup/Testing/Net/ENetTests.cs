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

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ServerClientConnects()
    {
        TestOutput.Header(nameof(ServerClientConnects));
        try
        {
            await using ENetTestHarness<CPacketNestedCollections> harness = new((_, _) => { });
            TestOutput.Step("Connecting client/server");
            bool connected = await harness.ConnectAsync(_connectTimeout);
            AssertBool(connected).IsTrue();
        }
        finally
        {
            TestOutput.Footer();
        }
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ClientSendsPacketNestedCollectionsToServer()
    {
        TestOutput.Header(nameof(ClientSendsPacketNestedCollectionsToServer));
        try
        {
            CPacketNestedCollections expected = PacketNestedCollectionsFactory.CreateSample();
            await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
        }
        finally
        {
            TestOutput.Footer();
        }
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ClientSendsPacketNestedCollectionsToServer_Deep()
    {
        TestOutput.Header(nameof(ClientSendsPacketNestedCollectionsToServer_Deep));
        try
        {
            CPacketNestedCollections expected = PacketNestedCollectionsFactory.CreateDeepSample();
            await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
        }
        finally
        {
            TestOutput.Footer();
        }
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ClientSendsPacketPrimitivesToServer()
    {
        TestOutput.Header(nameof(ClientSendsPacketPrimitivesToServer));
        try
        {
            CPacketPrimitives expected = PacketPrimitivesFactory.CreateSample();
            await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
        }
        finally
        {
            TestOutput.Footer();
        }
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ClientSendsPacketPrimitivesToServer_Deep()
    {
        TestOutput.Header(nameof(ClientSendsPacketPrimitivesToServer_Deep));
        try
        {
            CPacketPrimitives expected = PacketPrimitivesFactory.CreateDeepSample();
            await PacketRoundTripRunner.RunAsync(expected, _connectTimeout, _packetTimeout);
        }
        finally
        {
            TestOutput.Footer();
        }
    }
}
