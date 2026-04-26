using __TEMPLATE__.Netcode.Client;
using __TEMPLATE__.Netcode.Server;
using Godot;
using GodotUtils;
using System;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Shared net control panel implementation for generic client/server types.
/// </summary>
/// <typeparam name="TGameClient">Client implementation type.</typeparam>
/// <typeparam name="TGameServer">Server implementation type.</typeparam>
public abstract partial class NetControlPanelLow<TGameClient, TGameServer> : Control, ISceneDependencyReceiver
    where TGameClient : GodotClient, new()
    where TGameServer : GodotServer, new()
{
    [Export] private LineEdit _usernameLineEdit = null!;
    [Export] private LineEdit _ipLineEdit = null!;
    [Export] private Button _startServerBtn = null!;
    [Export] private Button _stopServerBtn = null!;
    [Export] private Button _startClientBtn = null!;
    [Export] private Button _stopClientBtn = null!;

    private string _username = string.Empty;
    private ushort _port;
    private string _ip = null!;
    private GodotClient? _subscribedClient;
    private ILoggerService _loggerService = null!;
    private IApplicationLifetime _applicationLifetime = null!;
    private IBackgroundTaskTracker _backgroundTasks = null!;
    private bool _isConfigured;

    /// <summary>
    /// Gets active net coordinator instance.
    /// </summary>
    public Net<TGameClient, TGameServer>? Net { get; private set; }

    /// <summary>
    /// Gets currently selected port value.
    /// </summary>
    public ushort CurrentPort => _port;

    /// <summary>
    /// Gets configured max client count.
    /// </summary>
    public int CurrentMaxClients => DefaultMaxClients;

    /// <summary>
    /// Gets or sets ENet options used when starting server.
    /// </summary>
    protected abstract ENetOptions Options { get; set; }

    /// <summary>
    /// Gets default max clients used by the panel.
    /// </summary>
    protected virtual int DefaultMaxClients { get; } = 100;

    /// <summary>
    /// Gets default localhost address.
    /// </summary>
    protected virtual string DefaultLocalIp { get; } = "127.0.0.1";

    /// <summary>
    /// Gets default server port.
    /// </summary>
    protected virtual ushort DefaultPort { get; } = 25565;

    /// <summary>
    /// Injects runtime services required by this panel.
    /// </summary>
    /// <param name="services">Resolved scene services.</param>
    public void Configure(GameServices services)
    {
        _loggerService = services.Logger;
        _applicationLifetime = services.ApplicationLifetime;
        _backgroundTasks = services.BackgroundTasks;
        _isConfigured = true;
    }

    public override void _EnterTree()
    {
        SceneComposition.ConfigureNodeFromGame(this);
    }

    public override void _Ready()
    {
        // Fail fast when dependency injection was skipped before scene readiness.
        if (!_isConfigured)
            throw new InvalidOperationException($"{nameof(NetControlPanelLow<,>)} was not configured before _Ready.");

        _port = DefaultPort;
        _ip = DefaultLocalIp;

        Net = new Net<TGameClient, TGameServer>(_loggerService, _applicationLifetime, _backgroundTasks);
        BindUiEvents();
        BindNetEvents();
    }

    public override void _Process(double delta)
    {
        // Pump client packets each frame only while a client instance exists.
        if (Net?.Client != null)
            Net.Client.HandlePackets();
    }

    public override void _ExitTree()
    {
        UnsubscribeFromClient(_subscribedClient);
        UnbindUiEvents();
        UnbindNetEvents();
        Net?.Dispose();
        Net = null;
    }

    /// <summary>
    /// Subscribes UI control events to panel handlers.
    /// </summary>
    private void BindUiEvents()
    {
        _startServerBtn.Pressed += OnStartServerPressed;
        _stopServerBtn.Pressed += OnStopServerPressed;
        _startClientBtn.Pressed += OnStartClientBtnPressed;
        _stopClientBtn.Pressed += OnStopClientBtnPressed;
        _ipLineEdit.TextChanged += OnIpChanged;
        _usernameLineEdit.TextChanged += OnUsernameChanged;
    }

    /// <summary>
    /// Unsubscribes UI control events from panel handlers.
    /// </summary>
    private void UnbindUiEvents()
    {
        _startServerBtn?.Pressed -= OnStartServerPressed;
        _stopServerBtn?.Pressed -= OnStopServerPressed;
        _startClientBtn?.Pressed -= OnStartClientBtnPressed;
        _stopClientBtn?.Pressed -= OnStopClientBtnPressed;
        _ipLineEdit?.TextChanged -= OnIpChanged;
        _usernameLineEdit?.TextChanged -= OnUsernameChanged;
    }

    /// <summary>
    /// Subscribes net coordinator lifecycle events.
    /// </summary>
    private void BindNetEvents()
    {
        // Guard against disposal timing where Net may already be null.
        if (Net != null)
        {
            Net.ClientCreated += OnClientCreated;
            Net.ClientDestroyed += OnClientDestroyed;
        }
    }

    /// <summary>
    /// Unsubscribes net coordinator lifecycle events.
    /// </summary>
    private void UnbindNetEvents()
    {
        // Guard against disposal timing where Net may already be null.
        if (Net != null)
        {
            Net.ClientCreated -= OnClientCreated;
            Net.ClientDestroyed -= OnClientDestroyed;
        }
    }

    /// <summary>
    /// Subscribes to client connection lifecycle events.
    /// </summary>
    /// <param name="client">Client to subscribe.</param>
    private void SubscribeToClient(GodotClient client)
    {
        // Replace prior client subscription before wiring a new one.
        if (_subscribedClient != null)
            UnsubscribeFromClient(_subscribedClient);

        // Subscribe only when a valid client instance is provided.
        if (client != null)
        {
            _subscribedClient = client;
            _subscribedClient.Connected += OnClientConnected;
            _subscribedClient.Disconnected += OnClientDisconnected;
        }
    }

    /// <summary>
    /// Unsubscribes client connection lifecycle events.
    /// </summary>
    /// <param name="client">Client to unsubscribe.</param>
    private void UnsubscribeFromClient(GodotClient? client)
    {
        // Remove handlers only when a client instance exists.
        if (client != null)
        {
            client.Connected -= OnClientConnected;
            client.Disconnected -= OnClientDisconnected;
        }

        // Clear tracking only when unsubscribing the currently tracked client.
        if (_subscribedClient == client)
            _subscribedClient = null;
    }

    private void OnClientConnected()
    {
        // Lock server controls when client connected to a remote host.
        if (!Net!.Server.IsRunning)
            DisableServerButtons();

        GetTree().UnfocusCurrentControl();
    }

    /// <summary>
    /// Disables server start/stop controls.
    /// </summary>
    private void DisableServerButtons()
    {
        _startServerBtn.Disabled = true;
        _stopServerBtn.Disabled = true;
    }

    /// <summary>
    /// Enables server start/stop controls.
    /// </summary>
    private void EnableServerButtons()
    {
        _startServerBtn.Disabled = false;
        _stopServerBtn.Disabled = false;
    }

    // Handlers
    private void OnStartServerPressed()
    {
        Net!.StartServer(_port, DefaultMaxClients, Options);
    }

    private void OnStopServerPressed()
    {
        Net!.StopServer();
    }

    private void OnStartClientBtnPressed()
    {
        Net!.StartClient(_ip!, _port);
    }

    private void OnStopClientBtnPressed()
    {
        Net!.StopClient();
    }

    private void OnIpChanged(string text)
    {
        _ip = ParseIpAndPort(text, ref _port);
    }

    private void OnUsernameChanged(string text)
    {
        // Accept usernames only when characters are alphanumeric.
        if (text.IsAlphaNumeric())
            _username = text;
    }

    private void OnClientCreated(GodotClient client)
    {
        SubscribeToClient(client);
    }

    private void OnClientDestroyed(GodotClient client)
    {
        UnsubscribeFromClient(client);
        EnableServerButtons();
    }

    private void OnClientDisconnected(DisconnectOpcode opcode)
    {
        EnableServerButtons();
    }

    /// <summary>
    /// Parses host:port input and updates referenced port when present.
    /// </summary>
    /// <param name="input">Input text in host or host:port form.</param>
    /// <param name="port">Port reference updated on successful parse.</param>
    /// <returns>Parsed host value.</returns>
    private static string ParseIpAndPort(string input, ref ushort port)
    {
        string[] addressParts = input.Split(':');
        string ip = addressParts[0];

        // Apply parsed port only when host:port format includes a valid port.
        if (addressParts.Length > 1 && ushort.TryParse(addressParts[1], out ushort parsedPort))
            port = parsedPort;

        return ip;
    }
}
