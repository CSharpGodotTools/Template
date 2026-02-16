using Framework.Netcode.Client;
using Framework.Netcode.Server;
using Godot;
using GodotUtils;
using System;
using System.Threading.Tasks;

namespace Framework.Netcode;

public abstract partial class NetControlPanelLow<TGameClient, TGameServer> : Control
    where TGameClient : GodotClient, new()
    where TGameServer : GodotServer, new()
{
    [Export] private LineEdit _usernameLineEdit;
    [Export] private LineEdit _ipLineEdit;
    [Export] private Button _startServerBtn;
    [Export] private Button _stopServerBtn;
    [Export] private Button _startClientBtn;
    [Export] private Button _stopClientBtn;

    private string _username = string.Empty;
    private ushort _port;
    private string _ip;
    private GodotClient _subscribedClient;

    public Net Net { get; private set; }
    public ushort CurrentPort => _port;
    public int CurrentMaxClients => DefaultMaxClients;

    protected abstract ENetOptions Options { get; set; }
    protected virtual int DefaultMaxClients { get; } = 100;
    protected virtual string DefaultLocalIp { get; } = "127.0.0.1";
    protected virtual ushort DefaultPort { get; } = 25565;

    public override void _Ready()
    {
        _port = DefaultPort;
        _ip = DefaultLocalIp;

        Net = CreateNet();
        BindUiEvents();
        BindNetEvents();
    }

    public override void _Process(double delta)
    {
        if (Net != null && Net.Client != null)
        {
            Net.Client.HandlePackets();
        }
    }

    public override void _ExitTree()
    {
        UnsubscribeFromClient(_subscribedClient);
        UnbindUiEvents();
        UnbindNetEvents();
        Net.Dispose();
        Net = null;
    }

    private static Net CreateNet()
    {
        ServerFactory serverFactory = new(() => new TGameServer());
        ClientFactory clientFactory = new(() => new TGameClient());
        return new Net(clientFactory, serverFactory);
    }

    private void BindUiEvents()
    {
        _startServerBtn.Pressed += OnStartServerPressed;
        _stopServerBtn.Pressed += OnStopServerPressed;
        _startClientBtn.Pressed += OnStartClientBtnPressed;
        _stopClientBtn.Pressed += OnStopClientBtnPressed;
        _ipLineEdit.TextChanged += OnIpChanged;
        _usernameLineEdit.TextChanged += OnUsernameChanged;
    }

    private void UnbindUiEvents()
    {
        if (_startServerBtn != null)
        {
            _startServerBtn.Pressed -= OnStartServerPressed;
        }

        if (_stopServerBtn != null)
        {
            _stopServerBtn.Pressed -= OnStopServerPressed;
        }

        if (_startClientBtn != null)
        {
            _startClientBtn.Pressed -= OnStartClientBtnPressed;
        }

        if (_stopClientBtn != null)
        {
            _stopClientBtn.Pressed -= OnStopClientBtnPressed;
        }

        if (_ipLineEdit != null)
        {
            _ipLineEdit.TextChanged -= OnIpChanged;
        }

        if (_usernameLineEdit != null)
        {
            _usernameLineEdit.TextChanged -= OnUsernameChanged;
        }
    }

    private void BindNetEvents()
    {
        if (Net != null)
        {
            Net.ClientCreated += OnClientCreated;
            Net.ClientDestroyed += OnClientDestroyed;
        }
    }

    private void UnbindNetEvents()
    {
        if (Net != null)
        {
            Net.ClientCreated -= OnClientCreated;
            Net.ClientDestroyed -= OnClientDestroyed;
        }
    }

    private void OnStartServerPressed()
    {
        Net.StartServer(_port, DefaultMaxClients, Options);
    }

    private void OnStopServerPressed()
    {
        Net.StopServer();
    }

    private void OnStartClientBtnPressed()
    {
        Task connectTask = Net.StartClient(_ip, _port);
        _ = connectTask.ContinueWith(
            task => GameFramework.Logger.LogErr(task.Exception!, "Client start failed"),
            TaskContinuationOptions.OnlyOnFaulted);
    }

    private void OnStopClientBtnPressed()
    {
        Net.StopClient();
    }

    private void OnIpChanged(string text)
    {
        _ip = ParseIpAndPort(text, ref _port);
    }

    private void OnUsernameChanged(string text)
    {
        if (text.IsAlphaNumeric())
        {
            _username = text;
        }
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

    private void SubscribeToClient(GodotClient client)
    {
        if (_subscribedClient != null)
        {
            UnsubscribeFromClient(_subscribedClient);
        }

        if (client != null)
        {
            _subscribedClient = client;
            _subscribedClient.Connected += OnClientConnected;
            _subscribedClient.Disconnected += OnClientDisconnected;
        }
    }

    private void UnsubscribeFromClient(GodotClient client)
    {
        if (client != null)
        {
            client.Connected -= OnClientConnected;
            client.Disconnected -= OnClientDisconnected;
        }

        if (_subscribedClient == client)
        {
            _subscribedClient = null;
        }
    }

    private void OnClientConnected()
    {
        if (!Net.Server.IsRunning)
        {
            DisableServerButtons();
        }

        GetTree().UnfocusCurrentControl();
    }

    private void OnClientDisconnected(DisconnectOpcode opcode)
    {
        EnableServerButtons();
    }

    private void DisableServerButtons()
    {
        _startServerBtn.Disabled = true;
        _stopServerBtn.Disabled = true;
    }

    private void EnableServerButtons()
    {
        _startServerBtn.Disabled = false;
        _stopServerBtn.Disabled = false;
    }

    private static string ParseIpAndPort(string input, ref ushort port)
    {
        string[] addressParts = input.Split(':');
        string ip = addressParts[0];

        if (addressParts.Length > 1 && ushort.TryParse(addressParts[1], out ushort parsedPort))
        {
            port = parsedPort;
        }

        return ip;
    }

    private record ClientFactory(Func<GodotClient> Creator) : IGameClientFactory
    {
        /// <summary>
        /// Creates a game client instance.
        /// </summary>
        public GodotClient CreateClient()
        {
            return Creator();
        }
    }

    private record ServerFactory(Func<GodotServer> Creator) : IGameServerFactory
    {
        /// <summary>
        /// Creates a game server instance.
        /// </summary>
        public GodotServer CreateServer()
        {
            return Creator();
        }
    }
}
