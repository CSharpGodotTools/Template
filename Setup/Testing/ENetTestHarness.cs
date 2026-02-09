using Framework.Netcode;
using Framework.Netcode.Client;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Template.Setup.Testing;

public sealed class ENetTestHarness : IAsyncDisposable
{
    private const ushort Port = 25565;
    private const int MaxClients = 100;

    public TestServer Server { get; }
    public TestClient Client { get; }
    public Task ConnectTask { get; private set; }

    private static int _enetRefCount;

    public ENetTestHarness(Action<CPacketNestedCollections, ENet.Peer> onPacket)
    {
        AddEnetRef();
        Server = new TestServer(onPacket);
        Client = new TestClient();
    }

    public async Task<bool> ConnectAsync(TimeSpan timeout)
    {
        Console.WriteLine("[Test] Starting server...");
        Server.Start(Port, MaxClients, new ENetOptions());
        Console.WriteLine("[Test] Starting client...");
        ConnectTask = Client.Connect("127.0.0.1", Port, new ENetOptions());
        bool connected = await WaitForConnectedAsync(Client, timeout);
        Console.WriteLine($"[Test] Client connected: {connected}");
        return connected;
    }

    public void Send(ClientPacket packet)
    {
        Console.WriteLine($"[Test] Sending packet {packet.GetType().Name}...");
        Client.Send(packet);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Stop();
        Server.Stop();
        if (ConnectTask != null)
        {
            await ConnectTask;
        }
        ReleaseEnetRef();
    }

    private static async Task<bool> WaitForConnectedAsync(GodotClient client, TimeSpan timeout)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

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

    private static void AddEnetRef()
    {
        if (Interlocked.Increment(ref _enetRefCount) == 1)
        {
            ENet.Library.Initialize();
        }
    }

    private static void ReleaseEnetRef()
    {
        if (Interlocked.Decrement(ref _enetRefCount) == 0)
        {
            ENet.Library.Deinitialize();
        }
    }
}
