using GdUnit4;
using static GdUnit4.Assertions;
using System;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

[TestSuite]
public class ENetTests
{
    [TestCase]
    [RequireGodotRuntime]
    public static async Task ServerClientConnects()
    {
        await using ENetTestHarness harness = new((_, _) => { });
        bool connected = await harness.ConnectAsync(TimeSpan.FromSeconds(2));
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
        bool connected = await harness.ConnectAsync(TimeSpan.FromSeconds(2));
        AssertBool(connected).IsTrue();

        harness.Send(expected);

        bool received = await capture.WaitAsync(TimeSpan.FromSeconds(2));
        AssertBool(received).IsTrue();

        if (received)
        {
            AssertBool(expected.Equals(capture.Packet)).IsTrue();
        }
    }
}
