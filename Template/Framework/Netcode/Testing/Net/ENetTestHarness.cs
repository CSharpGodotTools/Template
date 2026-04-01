using __TEMPLATE__;
using __TEMPLATE__.Netcode;
using __TEMPLATE__.Netcode.Client;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Setup.Testing;

/// <summary>
/// Hosts a temporary ENet server/client pair for packet integration tests.
/// </summary>
/// <typeparam name="TPacket">Packet type sent from client to server.</typeparam>
public sealed class ENetTestHarness<TPacket> : IAsyncDisposable
    where TPacket : ClientPacket
{
    private const ushort Port = 25565;
    private const int MaxClients = 100;
    private const int ShutdownPollIntervalMs = 25;
    private static readonly TimeSpan _shutdownTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the test server that captures incoming packets.
    /// </summary>
    public TestServer<TPacket> Server { get; }

    /// <summary>
    /// Gets the client used to send test packets.
    /// </summary>
    public TestClient Client { get; }

    /// <summary>
    /// Gets the active asynchronous connection task.
    /// </summary>
    public Task ConnectTask { get; private set; } = null!;

    private readonly Logger _loggerService;

    private static int _enetRefCount;

    /// <summary>
    /// Initializes a new harness and wires shared logging for both endpoints.
    /// </summary>
    /// <param name="onPacket">Callback invoked when a packet reaches the server.</param>
    public ENetTestHarness(Action<TPacket, uint> onPacket)
    {
        AddEnetRef();
        Server = new TestServer<TPacket>(onPacket);
        Client = new TestClient();
        _loggerService = new Logger();
        Server.ConfigureLoggerService(_loggerService);
        Client.ConfigureLoggerService(_loggerService);
    }

    /// <summary>
    /// Starts the server and client and waits for a connected client state.
    /// </summary>
    /// <param name="timeout">Maximum time allowed for startup/connection.</param>
    /// <param name="options">Optional ENet options, or default options when null.</param>
    /// <returns><see langword="true"/> if the client connects before timeout.</returns>
    public async Task<bool> ConnectAsync(TimeSpan timeout, ENetOptions? options = null)
    {
        options ??= new ENetOptions();

        // Start the server first so client connect does not race endpoint startup.
        Console.WriteLine("[Test] Starting server...");
        Server.Start(Port, MaxClients, options);
        bool serverRunning = await WaitForRunningAsync(Server, timeout);
        Console.WriteLine($"[Test] Server running: {serverRunning}");

        // Abort connect flow when server did not reach running state in time.
        if (!serverRunning)
        {
            return false;
        }

        // Keep the connect task so disposal can await outstanding connection work.
        Console.WriteLine("[Test] Starting client...");
        Stopwatch connectWatch = Stopwatch.StartNew();
        ConnectTask = Client.Connect("127.0.0.1", Port, options);
        bool connected = await WaitForConnectedAsync(Client, timeout);
        connectWatch.Stop();
        Console.Write($"[Test] Client connected: {connected}");
        TestOutput.WriteMsInParens(connectWatch.ElapsedMilliseconds);
        Console.WriteLine();
        return connected;
    }

    /// <summary>
    /// Sends a packet from the harness client to the harness server.
    /// </summary>
    /// <param name="packet">Packet instance to transmit.</param>
    /// <param name="log">Whether to print a send log line.</param>
    public void Send(TPacket packet, bool log = true)
    {
        // Print packet-send diagnostics when requested by caller.
        if (log)
        {
            Console.WriteLine($"[Test] Sending packet {packet.GetType().Name}...");
        }

        Client.Send(packet);
    }

    /// <summary>
    /// Stops endpoints, awaits completion, and releases shared ENet resources.
    /// </summary>
    /// <returns>A value task that completes when both endpoints are stopped and ENet refs are released.</returns>
    public async ValueTask DisposeAsync()
    {
        // Stop client before waiting for final shutdown state.
        if (Client.IsRunning)
        {
            Client.Stop();
        }

        // Stop server before waiting for final shutdown state.
        if (Server.IsRunning)
        {
            Server.Stop();
        }

        // Await outstanding connect task to avoid orphaned background work.
        if (ConnectTask != null)
        {
            await ConnectTask;
        }

        await WaitForStoppedAsync("client", () => Client.IsRunning, _shutdownTimeout);
        await WaitForStoppedAsync("server", () => Server.IsRunning, _shutdownTimeout);
        _loggerService.Dispose();
        ReleaseEnetRef();
    }

    /// <summary>
    /// Waits until a client reports connected state.
    /// </summary>
    /// <param name="client">Client to monitor.</param>
    /// <param name="timeout">Maximum wait duration.</param>
    /// <returns><see langword="true"/> if connected before timeout.</returns>
    private static async Task<bool> WaitForConnectedAsync(GodotClient client, TimeSpan timeout)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Polling keeps this helper deterministic in headless test runs.
        while (stopwatch.Elapsed < timeout)
        {
            // Return immediately once client reports connected.
            if (client.IsConnected)
            {
                return true;
            }

            await Task.Delay(10);
        }

        return false;
    }

    /// <summary>
    /// Waits until an ENet endpoint transitions to running.
    /// </summary>
    /// <param name="enet">Endpoint to monitor.</param>
    /// <param name="timeout">Maximum wait duration.</param>
    /// <returns><see langword="true"/> if running before timeout.</returns>
    private static async Task<bool> WaitForRunningAsync(ENetLow enet, TimeSpan timeout)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            // Return immediately once endpoint reports running.
            if (enet.IsRunning)
            {
                return true;
            }

            await Task.Delay(10);
        }

        return enet.IsRunning;
    }

    /// <summary>
    /// Waits for an endpoint to stop and logs if it remains running past timeout.
    /// </summary>
    /// <param name="name">Endpoint name used in diagnostics.</param>
    /// <param name="isRunning">Function that returns endpoint running state.</param>
    /// <param name="timeout">Maximum wait duration.</param>
    /// <returns>A task that completes when the endpoint stops or timeout is reached.</returns>
    private static async Task WaitForStoppedAsync(string name, Func<bool> isRunning, TimeSpan timeout)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (isRunning() && stopwatch.Elapsed < timeout)
        {
            await Task.Delay(ShutdownPollIntervalMs);
        }

        // Warn when endpoint remains running after timeout window.
        if (isRunning())
        {
            Console.WriteLine($"[Test] Warning: {name} did not stop within {timeout.TotalSeconds:0.##}s");
        }
    }

    /// <summary>
    /// Increments ENet reference count and initializes the library on first use.
    /// </summary>
    private static void AddEnetRef()
    {
        // Initialize ENet only on first active harness instance.
        if (Interlocked.Increment(ref _enetRefCount) == 1)
        {
            ENet.Library.Initialize();
        }
    }

    /// <summary>
    /// Decrements ENet reference count and deinitializes the library on last use.
    /// </summary>
    private static void ReleaseEnetRef()
    {
        // Deinitialize ENet after the last harness is released.
        if (Interlocked.Decrement(ref _enetRefCount) == 0)
        {
            ENet.Library.Deinitialize();
        }
    }
}
