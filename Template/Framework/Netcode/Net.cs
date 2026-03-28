using __TEMPLATE__.Netcode.Client;
using __TEMPLATE__.Netcode.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Top-level network coordinator. Owns a single server and client pair, manages
/// ENet initialisation/deinitialisation, and exposes lifecycle events for both.
/// </summary>
public class Net<TGameClient, TGameServer> : IDisposable
    where TGameClient : GodotClient, new()
    where TGameServer : GodotServer, new()
{
    private const int ShutdownPollIntervalMs = 50;
    private const int DefaultMaxClients = 500;
    private const int ENetMaximumPeers = 4096;

    private static readonly ENetOptions _defaultOptions = new()
    {
        PrintPacketByteSize = false,
        PrintPacketData = false,
        PrintPacketReceived = false,
        PrintPacketSent = false
    };

    private readonly bool _enetInitialized;
    private readonly ILoggerService _loggerService;
    private readonly IApplicationLifetime _applicationLifetime;
    private long _shutdownStarted;
    private int _disposed;

    /// <summary>Raised when a new server instance is created.</summary>
    public event Action<GodotServer>? ServerCreated;

    /// <summary>Raised when a new client instance is created.</summary>
    public event Action<GodotClient>? ClientCreated;

    /// <summary>Raised when a client instance is stopped and destroyed.</summary>
    public event Action<GodotClient>? ClientDestroyed;

    /// <summary>
    /// Byte offset within a packet reserved for the heartbeat sequence number.
    /// </summary>
    public static int HeartbeatPosition { get; } = 20;

    /// <summary>The active server instance.</summary>
    public GodotServer Server { get; private set; }

    /// <summary>The active client instance.</summary>
    public GodotClient Client { get; private set; }

    /// <summary>Port the current server is listening on.</summary>
    public ushort ServerPort { get; private set; }

    /// <summary>Maximum concurrent clients for the current server.</summary>
    public int ServerMaxClients { get; private set; }

    /// <summary>
    /// Creates a network coordinator that owns the active server and client instances.
    /// </summary>
    public Net(ILoggerService loggerService, IApplicationLifetime applicationLifetime)
    {
        _loggerService = loggerService;
        _applicationLifetime = applicationLifetime;

        _enetInitialized = TryInitializeEnet();

        _applicationLifetime.PreQuit += StopThreads;

        Client = new TGameClient();
        Client.ConfigureLoggerService(_loggerService);
        Server = new TGameServer();
        Server.ConfigureLoggerService(_loggerService);
    }

    /// <summary>
    /// Creates and starts a new server instance.
    /// </summary>
    public void StartServer(ushort port, int maxClients = DefaultMaxClients, ENetOptions? options = null)
    {
        if (maxClients >= ENetMaximumPeers)
            throw new ArgumentException($"ENet only supports a maximum of {ENetMaximumPeers - 1} clients");

        options ??= _defaultOptions;

        if (!CanUseENet())
            return;

        if (Server.IsRunning)
        {
            Server.Log("Server is running already");
            return;
        }

        ServerPort = port;
        ServerMaxClients = maxClients;

        Server = new TGameServer();
        Server.ConfigureLoggerService(_loggerService);
        ServerCreated?.Invoke(Server);
        Server.Start(port, maxClients, options);
    }

    /// <summary>
    /// Requests the current server instance to stop.
    /// </summary>
    public void StopServer()
    {
        Server.Stop();
    }

    /// <summary>
    /// Creates and connects a new client instance.
    /// </summary>
    public void StartClient(string ip, ushort port)
    {
        if (!CanUseENet())
        {
            return;
        }

        if (Client.IsRunning)
        {
            Client.Log("Client is running already");
            return;
        }

        Client = new TGameClient();
        Client.ConfigureLoggerService(_loggerService);
        ClientCreated?.Invoke(Client);

        // Fire-and-forget connect (if Connect is async)
        _ = Client.Connect(ip, port, CloneDefaultOptions());
    }

    /// <summary>
    /// Requests the current client instance to stop.
    /// </summary>
    public void StopClient()
    {
        if (!Client.IsRunning)
        {
            Client.Log("Client was stopped already");
            return;
        }

        Client.Stop();
        ClientDestroyed?.Invoke(Client);
    }

    /// <summary>
    /// Attempts to initialize the ENet native library. Returns <c>false</c> if the DLL is missing.
    /// </summary>
    private bool TryInitializeEnet()
    {
        try
        {
            ENet.Library.Initialize();
            return true;
        }
        catch (DllNotFoundException exception)
        {
            _loggerService.LogErr(exception);
            return false;
        }
    }

    /// <summary>
    /// Gracefully stops both server and client workers, then deinitializes ENet.
    /// </summary>
    private async Task StopThreads()
    {
        if (Interlocked.CompareExchange(ref _shutdownStarted, 1, 0) != 0)
        {
            return;
        }

        try
        {
            if (_enetInitialized)
            {
                await StopServerIfRunning();
                await StopClientIfRunning();
                ENet.Library.Deinitialize();
            }

            while (_loggerService.StillWorking())
            {
                await Task.Delay(ShutdownPollIntervalMs);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _shutdownStarted, 0);
        }
    }

    /// <summary>
    /// Returns <c>true</c> when ENet is initialized; logs a warning and returns <c>false</c> otherwise.
    /// </summary>
    private bool CanUseENet()
    {
        if (_enetInitialized)
        {
            return true;
        }

        _loggerService.LogWarning("ENet is not initialized. Network operation was ignored.");
        return false;
    }

    /// <summary>
    /// Creates a copy of the default ENet logging options.
    /// </summary>
    private static ENetOptions CloneDefaultOptions()
    {
        return new ENetOptions
        {
            PrintPacketByteSize = _defaultOptions.PrintPacketByteSize,
            PrintPacketData = _defaultOptions.PrintPacketData,
            PrintPacketReceived = _defaultOptions.PrintPacketReceived,
            PrintPacketSent = _defaultOptions.PrintPacketSent,
            ShowLogTimestamps = _defaultOptions.ShowLogTimestamps
        };
    }

    /// <summary>
    /// Stops the server and polls until the worker thread has fully exited.
    /// </summary>
    private async Task StopServerIfRunning()
    {
        if (!Server.IsRunning)
        {
            return;
        }

        Server.Stop();

        while (Server.IsRunning)
        {
            await Task.Delay(ShutdownPollIntervalMs);
        }
    }

    /// <summary>
    /// Stops the client and polls until the worker thread has fully exited.
    /// </summary>
    private async Task StopClientIfRunning()
    {
        if (!Client.IsRunning)
        {
            return;
        }

        Client.Stop();

        while (Client.IsRunning)
        {
            await Task.Delay(ShutdownPollIntervalMs);
        }
    }


    /// <summary>
    /// Request to shutdown the server and client.
    /// </summary>
    public void RequestShutdown()
    {
        _ = StopThreads();
    }

    /// <summary>
    /// Unsubscribes lifecycle handlers and requests shutdown of active network workers.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        _applicationLifetime.PreQuit -= StopThreads;

        if (Interlocked.Read(ref _shutdownStarted) == 0)
        {
            _ = StopThreads();
        }
    }
}
