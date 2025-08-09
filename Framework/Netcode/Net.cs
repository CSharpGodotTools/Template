using Godot;
using System;
using System.Threading.Tasks;
using GodotUtils;
using GodotUtils.Netcode;
using GodotUtils.Netcode.Client;
using GodotUtils.Netcode.Server;

namespace __TEMPLATE__.Netcode;

public class Net
{
    public event Action<ENetServer> ServerCreated;
    public event Action<ENetClient> ClientCreated;

    public static int HeartbeatPosition { get; } = 20;

    public ENetServer Server { get; private set; }
    public ENetClient Client { get; private set; }

    private const int ShutdownPollIntervalMs = 1;

    private IGameServerFactory _serverFactory;
    private IGameClientFactory _clientFactory;

    public Net(Node node, IGameServerFactory serverFactory, IGameClientFactory clientFactory)
    {
        Global.Instance.PreQuit += StopThreads;

        _serverFactory = serverFactory;
        _clientFactory = clientFactory;

        Server = serverFactory.CreateServer();
        Client = clientFactory.CreateClient();
    }

    ~Net()
    {
        GD.Print("Net deconstructor");
    }

    public void StopServer()
    {
        Server.Stop();
    }

    public void StartServer()
    {
        if (Server.IsRunning)
        {
            Server.Log("Server is running already");
            return;
        }

        Server = _serverFactory.CreateServer();
        ServerCreated?.Invoke(Server);
        Server.Start(25565, 100, new ENetOptions
        {
            PrintPacketByteSize = false,
            PrintPacketData = false,
            PrintPacketReceived = false,
            PrintPacketSent = false
        });

        Services.Get<UI.PopupMenu>().MainMenuBtnPressed += () =>
        {
            Server.Stop();
        };
    }

    public void StartClient(string ip, ushort port)
    {
        if (Client.IsRunning)
        {
            Client.Log("Client is running already");
            return;
        }

        Client = _clientFactory.CreateClient();

        ClientCreated?.Invoke(Client);

        Client.Connect(ip, port, new ENetOptions
        {
            PrintPacketByteSize = false,
            PrintPacketData = false,
            PrintPacketReceived = false,
            PrintPacketSent = false
        });
    }

    public void StopClient()
    {
        if (!Client.IsRunning)
        {
            Client.Log("Client was stopped already");
            return;
        }

        Client.Stop();
    }

    private async Task StopThreads()
    {
        // Stop the server and client
        if (ENetLow.ENetInitialized)
        {
            if (Server.IsRunning)
            {
                Server.Stop();

                while (Server.IsRunning)
                {
                    await Task.Delay(ShutdownPollIntervalMs);
                }
            }

            if (Client.IsRunning)
            {
                Client.Stop();

                while (Client.IsRunning)
                {
                    await Task.Delay(ShutdownPollIntervalMs);
                }
            }

            ENet.Library.Deinitialize();
        }

        // Wait for the logger to finish enqueing the remaining logs
        while (Logger.StillWorking())
        {
            await Task.Delay(ShutdownPollIntervalMs);
        }
    }
}
