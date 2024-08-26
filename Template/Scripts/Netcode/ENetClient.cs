﻿namespace Template.Netcode.Client;

using ENet;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

// ENet API Reference: https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md
public abstract class ENetClient : ENetLow
{
    #region Godot Thread
    /// <summary>
    /// Fires when the client connects to the server. Thread safe.
    /// </summary>
    public event Action OnConnected;

    /// <summary>
    /// Fires when the client disconnects or times out from the server. Thread safe.
    /// </summary>
    public event Action<DisconnectOpcode> OnDisconnected;

    /// <summary>
    /// Fires when the client times out from the server. Thread safe.
    /// </summary>
    public event Action OnTimeout;

    /// <summary>
    /// Is the client connected to the server? Thread safe.
    /// </summary>
    public bool IsConnected => Interlocked.Read(ref connected) == 1;

    /// <summary>
    /// <para>
    /// A thread safe way to connect to the server. IP can be set to "127.0.0.1" for 
    /// localhost and port can be set to something like 25565.
    /// </para>
    /// 
    /// <para>
    /// Options contains settings for enabling certain logging features and ignored 
    /// packets are packets that do not get logged to the console.
    /// </para>
    /// </summary>
    public async void Connect(string ip, ushort port, ENetOptions options = default, params Type[] ignoredPackets)
    {
        this.options = options;
        Log("Client is starting");
        Starting();
        InitIgnoredPackets(ignoredPackets);

        _running = 1;
        CTS = new CancellationTokenSource();
        using Task task = Task.Run(() => WorkerThread(ip, port), CTS.Token);

        try
        {
            await task;
        }
        catch (Exception e)
        {
            GU.Services.Get<Logger>().LogErr(e, "Client");
        }
    }

    /// <summary>
    /// Stop the client. This function is thread safe.
    /// </summary>
    public override void Stop()
    {
        if (_running == 0)
        {
            Log("Client has stopped already");
            return;
        }

        eNetCmds.Enqueue(new Cmd<ENetClientOpcode>(ENetClientOpcode.Disconnect));
    }

    /// <summary>
    /// Send a packet to the server. Packets are defined to be reliable by default. This
    /// function is thread safe.
    /// </summary>
    public void Send(ClientPacket packet, PacketFlags flags = PacketFlags.Reliable)
    {
        if (!IsConnected)
        {
            Log($"Can not send packet '{packet.GetType()}' because client is not connected to the server");
            return;
        }

        packet.Write();
        packet.SetPeer(peer);
        Outgoing.Enqueue(packet);
    }

    /// <summary>
    /// This function should be called in the _PhysicsProcess in the Godot thread. 
    /// </summary>
    public void HandlePackets()
    {
        while (godotPackets.TryDequeue(out PacketData packetData))
        {
            PacketReader packetReader = packetData.PacketReader;
            ServerPacket handlePacket = packetData.HandlePacket;
            Type type = packetData.Type;

            handlePacket.Read(packetReader);
            packetReader.Dispose();

            handlePacket.Handle(this);

            if (!IgnoredPackets.Contains(type) && options.PrintPacketReceived)
                Log($"Received packet: {type.Name}" +
                    $"{(options.PrintPacketData ? $"\n{handlePacket.PrintFull()}" : "")}", BBColor.Deepskyblue);
        }

        while (godotCmdsInternal.TryDequeue(out Cmd<GodotOpcode> cmd))
        {
            GodotOpcode opcode = cmd.Opcode;

            if (opcode == GodotOpcode.Connected)
            {
                OnConnected?.Invoke();
            }
            else if (opcode == GodotOpcode.Disconnected)
            {
                DisconnectOpcode disconnectOpcode = (DisconnectOpcode)cmd.Data[0];
                OnDisconnected?.Invoke(disconnectOpcode);
            }
            else if (opcode == GodotOpcode.Timeout)
            {
                OnTimeout?.Invoke();
            }
        }
    }

    /// <summary>
    /// Log messages as the client. Thread safe.
    /// </summary>
    public override void Log(object message, BBColor color = BBColor.Aqua) =>
        GU.Services.Get<Logger>().Log($"[Client] {message}", color);

    #endregion

    #region ENet Thread
    // Protected
    protected ConcurrentQueue<ClientPacket> Outgoing { get; } = new();

    // Private
    private readonly ConcurrentQueue<Cmd<GodotOpcode>> godotCmdsInternal = new();
    private const uint PING_INTERVAL = 1000;
    private const uint PEER_TIMEOUT = 5000;
    private const uint PEER_TIMEOUT_MINIMUM = 5000;
    private const uint PEER_TIMEOUT_MAXIMUM = 5000;

    private readonly ConcurrentQueue<PacketData> godotPackets = new();
    private readonly ConcurrentQueue<ENet.Packet> incoming = new();
    private readonly ConcurrentQueue<Cmd<ENetClientOpcode>> eNetCmds = new();

    Peer peer;
    long connected;

    static ENetClient()
    {
        ServerPacket.MapOpcodes();
    }

    protected override void ConcurrentQueues()
    {
        // ENetCmds
        while (eNetCmds.TryDequeue(out Cmd<ENetClientOpcode> cmd))
        {
            if (cmd.Opcode == ENetClientOpcode.Disconnect)
            {
                if (CTS.IsCancellationRequested)
                {
                    Log("Client is in the middle of stopping");
                    break;
                }

                peer.Disconnect(0);
                DisconnectCleanup(peer);
            }
        }

        // Incoming
        while (incoming.TryDequeue(out ENet.Packet packet))
        {
            PacketReader packetReader = new(packet);
            byte opcode = packetReader.ReadByte();

            Type type = ServerPacket.PacketMapBytes[opcode];
            ServerPacket handlePacket = ServerPacket.PacketMap[type].Instance;

            /*
            * Instead of packets being handled client-side, they are handled
            * on the Godot thread.
            * 
            * Note that handlePacket AND packetReader need to be sent over
            */
            godotPackets.Enqueue(new PacketData
            {
                Type = type,
                PacketReader = packetReader,
                HandlePacket = handlePacket
            });
        }

        // Outgoing
        while (Outgoing.TryDequeue(out ClientPacket clientPacket))
        {
            Type type = clientPacket.GetType();

            if (!IgnoredPackets.Contains(type) && options.PrintPacketSent)
                Log($"Sent packet: {type.Name} {FormatByteSize(clientPacket.GetSize())}" +
                    $"{(options.PrintPacketData ? $"\n{clientPacket.PrintFull()}" : "")}");

            clientPacket.Send();
        }
    }

    protected override void Connect(Event netEvent)
    {
        connected = 1;
        godotCmdsInternal.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Connected));
        Log("Client connected to server");
    }

    protected override void Disconnect(Event netEvent)
    {
        DisconnectOpcode opcode = (DisconnectOpcode)netEvent.Data;
        
        godotCmdsInternal.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Disconnected, opcode));
        
        DisconnectCleanup(peer);

        Log($"Received disconnect opcode from server: " +
            $"{opcode.ToString().ToLower()}");
    }

    protected override void Timeout(Event netEvent)
    {
        godotCmdsInternal.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Disconnected, DisconnectOpcode.Timeout));
        godotCmdsInternal.Enqueue(new Cmd<GodotOpcode>(GodotOpcode.Timeout));

        DisconnectCleanup(peer);
        Log("Client connection timeout");
    }

    protected override void Receive(Event netEvent)
    {
        ENet.Packet packet = netEvent.Packet;
        if (packet.Length > GamePacket.MaxSize)
        {
            Log($"Tried to read packet from server of size " +
                $"{packet.Length} when max packet size is " +
                $"{GamePacket.MaxSize}");

            packet.Dispose();
            return;
        }

        incoming.Enqueue(packet);
    }

    void WorkerThread(string ip, ushort port)
    {
        Host = new Host();
        Address address = new()
        {
            Port = port
        };

        address.SetHost(ip);
        Host.Create();

        peer = Host.Connect(address);
        peer.PingInterval(PING_INTERVAL);
        peer.Timeout(PEER_TIMEOUT, PEER_TIMEOUT_MINIMUM, PEER_TIMEOUT_MAXIMUM);

        WorkerLoop();

        Host.Dispose();
        Log("Client is no longer running");
    }

    protected override void DisconnectCleanup(Peer peer)
    {
        base.DisconnectCleanup(peer);
        connected = 0;
    }
    #endregion
}

public enum ENetClientOpcode
{
    Disconnect
}

public enum GodotOpcode
{
    Connected,
    Timeout,
    Disconnected
}

public class PacketData
{
    public Type Type { get; set; }
    public PacketReader PacketReader { get; set; }
    public ServerPacket HandlePacket { get; set; }
}
