using Framework.Netcode.Client;
using Framework.Netcode.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Netcode;

public class Net : IDisposable
{
    private const int ShutdownPollIntervalMs = 50;

    private static readonly ENetOptions _defaultClientOptions = new()
    {
        PrintPacketByteSize = false,
        PrintPacketData = false,
        PrintPacketReceived = false,
        PrintPacketSent = false
    };

    private readonly IGameClientFactory _clientFactory;
    private readonly IGameServerFactory _serverFactory;
    private readonly UI.PopupMenu _popupMenu;
    private readonly bool _enetInitialized;
    private long _shutdownStarted;
    private int _disposed;
    private bool _stopRequestedByMainMenu;

    public event Action<GodotServer> ServerCreated;
    public event Action<GodotClient> ClientCreated;
    public event Action<GodotClient> ClientDestroyed;

    public static int HeartbeatPosition { get; } = 20;

    public GodotServer Server { get; private set; }
    public GodotClient Client { get; private set; }
    public ushort ServerPort { get; private set; }
    public int ServerMaxClients { get; private set; }

    /// <summary>
    /// Creates a network coordinator that owns the active server and client instances.
    /// </summary>
    public Net(IGameClientFactory clientFactory, IGameServerFactory serverFactory)
    {
        ArgumentNullException.ThrowIfNull(clientFactory);
        ArgumentNullException.ThrowIfNull(serverFactory);

        _clientFactory = clientFactory;
        _serverFactory = serverFactory;
        _popupMenu = GameFramework.Services.Get<UI.PopupMenu>();
        _enetInitialized = TryInitializeEnet();

        Autoloads.Instance.PreQuit += StopThreads;
        _popupMenu.MainMenuBtnPressed += OnMainMenuBtnPressed;

        Client = _clientFactory.CreateClient();
        Server = _serverFactory.CreateServer();
    }

    /// <summary>
    /// Creates and starts a new server instance.
    /// </summary>
    public void StartServer(ushort port, int maxClients, ENetOptions options)
    {
        if (!CanUseENet())
        {
            return;
        }

        if (Server.IsRunning)
        {
            Server.Log("Server is running already");
            return;
        }

        ServerPort = port;
        ServerMaxClients = maxClients;

        Server = _serverFactory.CreateServer();
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
    public async Task StartClient(string ip, ushort port)
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

        Client = _clientFactory.CreateClient();
        ClientCreated?.Invoke(Client);

        await Client.Connect(ip, port, CloneDefaultClientOptions());
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

    private static bool TryInitializeEnet()
    {
        try
        {
            ENet.Library.Initialize();
            return true;
        }
        catch (DllNotFoundException exception)
        {
            GameFramework.Logger.LogErr(exception);
            return false;
        }
    }

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

            while (GameFramework.Logger.StillWorking())
            {
                await Task.Delay(ShutdownPollIntervalMs);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _shutdownStarted, 0);
        }
    }

    private bool CanUseENet()
    {
        if (_enetInitialized)
        {
            return true;
        }

        GameFramework.Logger.LogWarning("ENet is not initialized. Network operation was ignored.");
        return false;
    }

    private static ENetOptions CloneDefaultClientOptions()
    {
        return new ENetOptions
        {
            PrintPacketByteSize = _defaultClientOptions.PrintPacketByteSize,
            PrintPacketData = _defaultClientOptions.PrintPacketData,
            PrintPacketReceived = _defaultClientOptions.PrintPacketReceived,
            PrintPacketSent = _defaultClientOptions.PrintPacketSent,
            ShowLogTimestamps = _defaultClientOptions.ShowLogTimestamps
        };
    }

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

    private void OnMainMenuBtnPressed()
    {
        _stopRequestedByMainMenu = true;
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

        Autoloads.Instance.PreQuit -= StopThreads;
        _popupMenu.MainMenuBtnPressed -= OnMainMenuBtnPressed;

        if (!_stopRequestedByMainMenu)
        {
            _ = StopThreads();
        }

        GC.SuppressFinalize(this);
    }
}
