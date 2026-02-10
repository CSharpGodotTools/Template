using Framework.Netcode;
using Framework.Netcode.Client;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

public sealed class ENetTestHarness : IAsyncDisposable
{
    private const ushort Port = 25565;
    private const int MaxClients = 100;
    private const int ShutdownPollIntervalMs = 25;
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(5);

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
        bool serverRunning = await WaitForRunningAsync(Server, timeout);
        Console.WriteLine($"[Test] Server running: {serverRunning}");
        if (!serverRunning)
        {
            return false;
        }

        Console.WriteLine("[Test] Starting client...");
        Stopwatch connectWatch = Stopwatch.StartNew();
        ConnectTask = Client.Connect("127.0.0.1", Port, new ENetOptions());
        bool connected = await WaitForConnectedAsync(Client, timeout);
        connectWatch.Stop();
        Console.Write($"[Test] Client connected: {connected}");
        TestOutput.WriteMsInParens(connectWatch.ElapsedMilliseconds);
        Console.WriteLine();
        return connected;
    }

    public void Send(ClientPacket packet)
    {
        Console.WriteLine($"[Test] Sending packet {packet.GetType().Name}...");
        Client.Send(packet);
    }

    public async ValueTask DisposeAsync()
    {
        if (Client.IsRunning)
        {
            Client.Stop();
        }

        if (Server.IsRunning)
        {
            Server.Stop();
        }

        if (ConnectTask != null)
        {
            await ConnectTask;
        }

        await WaitForStoppedAsync("client", () => Client.IsRunning, ShutdownTimeout);
        await WaitForStoppedAsync("server", () => Server.IsRunning, ShutdownTimeout);
        ReleaseEnetRef();
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

    private static async Task<bool> WaitForRunningAsync(ENetLow enet, TimeSpan timeout)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (enet.IsRunning)
            {
                return true;
            }

            await Task.Delay(10);
        }

        return enet.IsRunning;
    }

    private static async Task WaitForStoppedAsync(string name, Func<bool> isRunning, TimeSpan timeout)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (isRunning() && stopwatch.Elapsed < timeout)
        {
            await Task.Delay(ShutdownPollIntervalMs);
        }

        if (isRunning())
        {
            Console.WriteLine($"[Test] Warning: {name} did not stop within {timeout.TotalSeconds:0.##}s");
        }
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
