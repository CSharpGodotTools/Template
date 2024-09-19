using CSharpUtils;
using Godot;
using GodotUtils;
using Template.Netcode;

namespace Template;

public abstract partial class UINetControlPanelLow : Control
{
    private Net _net;
    private string _ip = "127.0.0.1";
    private ushort _port = 25565;
    private string _username = "";

    public abstract IGameServerFactory GameServerFactory();
    public abstract IGameClientFactory GameClientFactory();
    public abstract void StartClientButtonPressed(string username);

    public override void _Ready()
    {
        _net = new Net();
        _net.Initialize(GameServerFactory(), GameClientFactory());

        SetupButtons();
        SetupInputFields();
        SetupClientEvents();
    }

    private void SetupButtons()
    {
        GetNode<Button>("%Start Server").Pressed += _net.StartServer;
        GetNode<Button>("%Stop Server").Pressed += _net.StopServer;
        GetNode<Button>("%Start Client").Pressed += () =>
        {
            StartClientButtonPressed(_username);
            _net.StartClient(_ip, _port);
        };
        GetNode<Button>("%Stop Client").Pressed += _net.StopClient;
    }

    private void SetupInputFields()
    {
        GetNode<LineEdit>("%IP").TextChanged += text =>
        {
            string[] parts = text.Split(":");
            _ip = parts[0];
            if (parts.Length > 1 && ushort.TryParse(parts[1], out ushort port))
            {
                _port = port;
            }
        };

        GetNode<LineEdit>("%Username").TextChanged += text =>
        {
            _username = text.IsAlphaNumeric() ? text : _username;
        };
    }

    private void SetupClientEvents()
    {
        _net.OnClientCreated += client =>
        {
            client.OnConnected += () =>
            {
                if (!_net.Server.IsRunning)
                {
                    GetNode<Button>("%Start Server").Disabled = true;
                    GetNode<Button>("%Stop Server").Disabled = true;
                }
                GetTree().UnfocusCurrentControl();
            };

            client.OnDisconnected += _ =>
            {
                GetNode<Button>("%Start Server").Disabled = false;
                GetNode<Button>("%Stop Server").Disabled = false;
            };
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        _net.Client?.HandlePackets();
    }
}
