using ENet;
using GodotUtils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Template.Netcode.Server;

// ENet API Reference: https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md
public abstract class ENetServer : ENetLow
{
    #region Godot Thread
    /// <summary>
    /// <para>
    /// A thread safe way to start the server. Max clients could be 100 and port could
    /// be set to something like 25565.
    /// </para>
    /// 
    /// <para>
    /// Options contains settings for enabling certain logging features and ignored 
    /// packets are packets that do not get logged to the console.
    /// </para>
    /// </summary>
    public async void Start(ushort port, int maxClients, ENetOptions options, params Type[] ignoredPackets)
    {
        if (_running == 1)
        {
            Log("Server is running already");
            return;
        }

        Options = options;

        InitIgnoredPackets(ignoredPackets);

        EmitLoop = new(100, Emit, false);
        EmitLoop.Start();

        _running = 1;

        CTS = new CancellationTokenSource();

        Starting();

        using Task task = Task.Run(() => WorkerThread(port, maxClients), CTS.Token);

        try
        {
            await task;
        }
        catch (Exception e)
        {
            Game.LogErr(e, "Server");
        }
    }

    /// <summary>
    /// Ban someone by their ID. Thread safe.
    /// </summary>
    public void Ban(uint id)
    {
        Kick(id, DisconnectOpcode.Banned);
    }

    /// <summary>
    /// Ban everyone on the server. Thread safe.
    /// </summary>
    public void BanAll()
    {
        KickAll(DisconnectOpcode.Banned);
    }

    /// <summary>
    /// Kick everyone on the server with a specified opcode. Thread safe.
    /// </summary>
    public void KickAll(DisconnectOpcode opcode)
    {
        enetCmds.Enqueue(new Cmd<ENetServerOpcode>(ENetServerOpcode.KickAll, opcode));
    }

    /// <summary>
    /// Kick someone by their ID with a specified opcode. Thread safe.
    /// </summary>
    public void Kick(uint id, DisconnectOpcode opcode)
    {
        enetCmds.Enqueue(new Cmd<ENetServerOpcode>(ENetServerOpcode.Kick, id, opcode));
    }

    /// <summary>
    /// Stop the server. Thread safe.
    /// </summary>
    public override void Stop()
    {
        if (_running == 0)
        {
            Log("Server has stopped already");
            return;
        }

        EmitLoop.Stop();
        EmitLoop.Dispose();
        enetCmds.Enqueue(new Cmd<ENetServerOpcode>(ENetServerOpcode.Stop));
    }

    /// <summary>
    /// Send a packet to a client. Thread safe.
    /// </summary>
    public void Send(ServerPacket packet, Peer peer)
    {
        packet.Write();

        Type type = packet.GetType();

        if (!IgnoredPackets.Contains(type) && Options.PrintPacketSent)
        {
            Log($"Sending packet {type.Name} {FormatByteSize(packet.GetSize())}to client {peer.ID}" +
                $"{(Options.PrintPacketData ? $"\n{packet.ToFormattedString()}" : "")}");
        }

        packet.SetSendType(SendType.Peer);
        packet.SetPeer(peer);

        EnqueuePacket(packet);
    }

    /// <summary>
    /// If no clients are specified, then the packet will be sent to everyone. If
    /// one client is specified then that client will be excluded from the broadcast.
    /// If more than one client is specified then the packet will only be sent to
    /// those clients. This function is thread safe.
    /// </summary>
    public void Broadcast(ServerPacket packet, params Peer[] clients)
    {
        packet.Write();

        Type type = packet.GetType();

        if (!IgnoredPackets.Contains(type) && Options.PrintPacketSent)
        {
            // This is messy but I don't know how I will clean it up right
            // now so I'm leaving it as is for now..
            string byteSize = Options.PrintPacketByteSize ?
                $"({packet.GetSize()} bytes)" : "";

            string peerArr = clients.Select(x => x.ID).ToFormattedString();

            string message = $"Broadcasting packet {type.Name} {byteSize}" + (clients.Length == 0 ?
                 "to everyone" : clients.Length == 1 ?
                $"to everyone except peer {peerArr}" :
                $"to peers {peerArr}") + (Options.PrintPacketData ?
                $"\n{packet.ToFormattedString()}" : "");

            Log(message);
        }

        packet.SetSendType(SendType.Broadcast);
        packet.SetPeers(clients);

        EnqueuePacket(packet);
    }

    /// <summary>
    /// Log a message as the server. This function is thread safe.
    /// </summary>
    public override void Log(object message, BBColor color = BBColor.Green)
    {
        Game.Log($"[Server] {message}", color);
    }
    #endregion

    #region ENet Thread
    /// <summary>
    /// This Dictionary is NOT thread safe and should only be accessed on the ENet Thread
    /// </summary>
    public Dictionary<uint, Peer> Peers { get; } = [];
    protected STimer EmitLoop { get; set; }

    private readonly ConcurrentQueue<(Packet, Peer)> incoming = new();
    private readonly ConcurrentQueue<ServerPacket> outgoing = new();
    private readonly ConcurrentQueue<Cmd<ENetServerOpcode>> enetCmds = new();

    static ENetServer()
    {
        ClientPacket.MapOpcodes();
    }

    protected abstract void Emit();

    private void EnqueuePacket(ServerPacket packet)
    {
        outgoing.Enqueue(packet);
    }

    protected override void ConcurrentQueues()
    {
        // ENet Cmds
        while (enetCmds.TryDequeue(out Cmd<ENetServerOpcode> cmd))
        {
            if (cmd.Opcode == ENetServerOpcode.Stop)
            {
                KickAll(DisconnectOpcode.Stopping);

                if (CTS.IsCancellationRequested)
                {
                    Log("Server is in the middle of stopping");
                    break;
                }

                CTS.Cancel();
            }
            else if (cmd.Opcode == ENetServerOpcode.Kick)
            {
                uint id = (uint)cmd.Data[0];
                DisconnectOpcode opcode = (DisconnectOpcode)cmd.Data[1];

                if (!Peers.ContainsKey(id))
                {
                    Log($"Tried to kick peer with id '{id}' but this peer does not exist");
                    break;
                }

                if (opcode == DisconnectOpcode.Banned)
                {
                    /* 
                     * TODO: Save the peer ip to banned.json and
                     * check banned.json whenever a peer tries to
                     * rejoin
                     */
                }

                Peers[id].DisconnectNow((uint)opcode);
                Peers.Remove(id);
            }
            else if (cmd.Opcode == ENetServerOpcode.KickAll)
            {
                DisconnectOpcode opcode = (DisconnectOpcode)cmd.Data[0];

                Peers.Values.ForEach(peer =>
                {
                    if (opcode == DisconnectOpcode.Banned)
                    {
                        /* 
                         * TODO: Save the peer ip to banned.json and
                         * check banned.json whenever a peer tries to
                         * rejoin
                         */
                    }

                    peer.DisconnectNow((uint)opcode);
                });
                Peers.Clear();
            }
        }

        // Incoming
        while (incoming.TryDequeue(out (Packet, Peer) packetPeer))
        {
            PacketReader packetReader = new(packetPeer.Item1);
            byte opcode = packetReader.ReadByte();

            if (!ClientPacket.PacketMapBytes.TryGetValue(opcode, out Type value))
            {
                Log($"Received malformed opcode: {opcode} (Ignoring)");
                return;
            }

            Type type = value;
            ClientPacket handlePacket = ClientPacket.PacketMap[type].Instance;
            try
            {
                handlePacket.Read(packetReader);
            }
            catch (System.IO.EndOfStreamException e)
            {
                Log($"Received malformed packet: {opcode} {e.Message} (Ignoring)");
                return;
            }

            packetReader.Dispose();

            handlePacket.Handle(this, packetPeer.Item2);

            if (!IgnoredPackets.Contains(type) && Options.PrintPacketReceived)
            {
                Log($"Received packet: {type.Name} from client {packetPeer.Item2.ID}" +
                    $"{(Options.PrintPacketData ? $"\n{handlePacket.ToFormattedString()}" : "")}", BBColor.LightGreen);
            }
        }

        // Outgoing
        while (outgoing.TryDequeue(out ServerPacket packet))
        {
            SendType sendType = packet.GetSendType();

            if (sendType == SendType.Peer)
            {
                packet.Send();
            }
            else if (sendType == SendType.Broadcast)
            {
                packet.Broadcast(Host);
            }
        }
    }

    protected override void Connect(Event netEvent)
    {
        Peers[netEvent.Peer.ID] = netEvent.Peer;
        Log("Client connected - ID: " + netEvent.Peer.ID);
    }

    protected abstract void Disconnected(Event netEvent);

    protected override void Disconnect(Event netEvent)
    {
        Peers.Remove(netEvent.Peer.ID);
        Log("Client disconnected - ID: " + netEvent.Peer.ID);
        Disconnected(netEvent);
    }

    protected override void Timeout(Event netEvent)
    {
        Peers.Remove(netEvent.Peer.ID);
        Log("Client timeout - ID: " + netEvent.Peer.ID);
        Disconnected(netEvent);
    }

    protected override void Receive(Event netEvent)
    {
        Packet packet = netEvent.Packet;

        if (packet.Length > GamePacket.MaxSize)
        {
            Log($"Tried to read packet from client of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
            packet.Dispose();
            return;
        }

        incoming.Enqueue((packet, netEvent.Peer));
    }

    private void WorkerThread(ushort port, int maxClients)
    {
        Host = new Host();

        try
        {
            Host.Create(new Address { Port = port }, maxClients);
        }
        catch (InvalidOperationException e)
        {
            Log($"A server is running on port {port} already! {e.Message}");
            return;
        }

        Log("Server is running");

        WorkerLoop();

        Host.Dispose();
        Log("Server has stopped");
    }

    protected override void DisconnectCleanup(Peer peer)
    {
        base.DisconnectCleanup(peer);
        Peers.Remove(peer.ID);
    }
    #endregion
}

public enum ENetServerOpcode
{
    Stop,
    Kick,
    KickAll
}

