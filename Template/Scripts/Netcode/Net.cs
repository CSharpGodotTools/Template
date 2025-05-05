using Godot;
using System;
using System.Threading.Tasks;
using Template.Netcode.Client;
using Template.Netcode.Server;
using Template.UI;

namespace Template.Netcode;

public class Net
{
    public event Action<ENetServer> OnServerCreated;
    public event Action<ENetClient> OnClientCreated;

    public static int HeartbeatPosition { get; } = 20;

    public ENetServer Server { get; private set; }
    public ENetClient Client { get; private set; }

    private IGameServerFactory _serverFactory;
    private IGameClientFactory _clientFactory;

    public void Initialize(IGameServerFactory serverFactory, IGameClientFactory clientFactory)
    {
        Global.OnQuit += StopThreads;

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
        OnServerCreated?.Invoke(Server);
        Server.Start(25565, 100, new ENetOptions
        {
            PrintPacketByteSize = false,
            PrintPacketData = false,
            PrintPacketReceived = false,
            PrintPacketSent = false
        });

        Services.Get<UIPopupMenu>().OnMainMenuBtnPressed += () =>
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

        OnClientCreated?.Invoke(Client);

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
                    await Task.Delay(1);
                }
            }

            if (Client.IsRunning)
            {
                Client.Stop();

                while (Client.IsRunning)
                {
                    await Task.Delay(1);
                }
            }

            ENet.Library.Deinitialize();
        }

        // Wait for the logger to finish enqueing the remaining logs
        while (Global.Logger.StillWorking())
        {
            await Task.Delay(1);
        }
    }
}

