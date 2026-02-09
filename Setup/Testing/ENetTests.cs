using ENet;
using Framework.Netcode;
using Framework.Netcode.Client;
using Framework.Netcode.Server;
using GdUnit4;
using static GdUnit4.Assertions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Template.Setup.Testing;

[TestSuite]
public class ENetTests
{
    private const ushort Port = 25565;
    private const int MaxClients = 100;

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ServerClientConnects()
    {
        TestServer server = new((_, _) => { });
        TestClient client = new();

        Task connectTask = await StartServerAndClientAsync(server, client);

        try
        {
        }
        finally
        {
            client.Stop();
            server.Stop();
            await connectTask;
        }
    }

    [TestCase]
    [RequireGodotRuntime]
    public static async Task ClientSendsPacketListsToServer()
    {
        CPacketLists expected = new()
        {
            IntValues = [1, 2, 3],
            StringValues = ["Alpha", "Beta", "Gamma"],
            FloatValues = [1.5f, 2.5f, 3.5f]
        };

        bool dataMatches = false;
        TaskCompletionSource<bool> receivedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestServer server = new((packet, _) =>
        {
            dataMatches =
                AreEqual.Lists(expected.IntValues, packet.IntValues) &&
                AreEqual.Lists(expected.StringValues, packet.StringValues) &&
                AreEqual.Lists(expected.FloatValues, packet.FloatValues);

            receivedTcs.TrySetResult(true);
        });

        TestClient client = new();
        Task connectTask = await StartServerAndClientAsync(server, client);

        try
        {
            client.Send(expected);

            bool received = await WaitForTaskAsync(receivedTcs.Task, TimeSpan.FromSeconds(2));
            AssertBool(received).IsTrue();

            if (received)
            {
                AssertBool(dataMatches).IsTrue();
            }
        }
        finally
        {
            client.Stop();
            server.Stop();
            await connectTask;
        }
    }

    private static async Task<Task> StartServerAndClientAsync(GodotServer server, GodotClient client)
    {
        server.Start(Port, MaxClients, new ENetOptions());

        Task connectTask = client.Connect("127.0.0.1", Port, new ENetOptions());

        bool connected = await WaitForConnectedAsync(client, TimeSpan.FromSeconds(2));
        AssertBool(connected).IsTrue();

        return connectTask;
    }

    private static async Task<bool> WaitForConnectedAsync(GodotClient client, TimeSpan timeout)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (client.IsConnected)
            {
                return true;
            }

            await Task.Delay(10);
        }

        return false;
    }

    private static async Task<bool> WaitForTaskAsync(Task task, TimeSpan timeout)
    {
        Task completed = await Task.WhenAny(task, Task.Delay(timeout));
        return completed == task;
    }

    private sealed class TestServer : GodotServer
    {
        private readonly Action<CPacketLists, Peer> _onPacket;

        public TestServer(Action<CPacketLists, Peer> onPacket)
        {
            _onPacket = onPacket;
            if (_onPacket != null)
            {
                RegisterPacketHandler<CPacketLists>(HandlePacket);
            }
        }

        private void HandlePacket(CPacketLists packet, Peer peer)
        {
            _onPacket(packet, peer);
        }
    }

    private sealed class TestClient : GodotClient
    {
    }
}
