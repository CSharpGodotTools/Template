using Framework.Netcode;
using Framework.Netcode.Client;
using System;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

public sealed class ENetTestHarness : IAsyncDisposable
{
    private const ushort Port = 25565;
    private const int MaxClients = 100;

    public TestServer Server { get; }
    public TestClient Client { get; }
    public Task ConnectTask { get; private set; }

    public ENetTestHarness(Action<CPacketNestedCollections, ENet.Peer> onPacket)
    {
        Server = new TestServer(onPacket);
        Client = new TestClient();
    }

    public async Task<bool> ConnectAsync(TimeSpan timeout)
    {
        Server.Start(Port, MaxClients, new ENetOptions());
        ConnectTask = Client.Connect("127.0.0.1", Port, new ENetOptions());
        return await WaitForConnectedAsync(Client, timeout);
    }

    public void Send(ClientPacket packet)
    {
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
}
