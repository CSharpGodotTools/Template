using ENet;
using GodotUtils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Netcode.Server;

public abstract class GodotServer : ENetServer
{
    private const string LogTag = "Server";

    /// <summary>
    /// <para>
    /// Thread-safe server start entrypoint.
    /// </para>
    ///
    /// <para>
    /// Options controls logging behavior and ignored packets are excluded from logging.
    /// </para>
    /// </summary>
    public void Start(ushort port, int maxClients, ENetOptions options, params Type[] ignoredPackets)
    {
        if (IsRunning)
        {
            Log("Server is running already");
            return;
        }

        Options = options ?? new ENetOptions();
        InitIgnoredPackets(ignoredPackets);
        CTS = new CancellationTokenSource();
        _ = StartWorkerThreadAsync(port, maxClients);
    }

    private async Task StartWorkerThreadAsync(ushort port, int maxClients)
    {
        try
        {
            await Task.Factory.StartNew(
                () => WorkerThread(port, maxClients),
                CTS.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping the server.
        }
        catch (Exception exception)
        {
            GameFramework.Logger.LogErr(exception, LogTag);
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
    /// Kick someone by their ID with a specified opcode. Thread safe.
    /// </summary>
    public void Kick(uint id, DisconnectOpcode opcode)
    {
        ENetCmds.Enqueue(new Cmd<ENetServerOpcode>(ENetServerOpcode.Kick, id, opcode));
    }

    /// <summary>
    /// Stop the server. Thread safe.
    /// </summary>
    public sealed override void Stop()
    {
        if (!IsRunning)
        {
            Log("Server has stopped already");
            return;
        }

        ENetCmds.Enqueue(new Cmd<ENetServerOpcode>(ENetServerOpcode.Stop));
    }

    /// <summary>
    /// Send a packet to one client. Thread safe.
    /// </summary>
    public void Send(ServerPacket packet, Peer peer)
    {
        ArgumentNullException.ThrowIfNull(packet);

        packet.Write();
        LogSend(packet, $"to client {peer.ID}");

        packet.SetSendType(SendType.Peer);
        packet.SetPeer(peer);
        EnqueuePacket(packet);
    }

    /// <summary>
    /// Broadcast a packet to all clients or all except provided peers. Thread safe.
    /// </summary>
    public void Broadcast(ServerPacket packet, params Peer[] clients)
    {
        ArgumentNullException.ThrowIfNull(packet);

        Peer[] peers = clients ?? [];
        packet.Write();

        string peerDescription = GetBroadcastPeerDescription(peers);
        LogSend(packet, peerDescription, includeSeparatorPadding: true);

        packet.SetSendType(SendType.Broadcast);
        packet.SetPeers(peers);
        EnqueuePacket(packet);
    }

    private void LogSend(ServerPacket packet, string targetDescription, bool includeSeparatorPadding = false)
    {
        Type packetType = packet.GetType();
        if (!Options.PrintPacketSent || IgnoredPackets.Contains(packetType))
        {
            return;
        }

        string byteSize = FormatByteSize(packet.GetSize());
        if (includeSeparatorPadding && string.IsNullOrEmpty(byteSize))
        {
            byteSize = " ";
        }

        string packetData = string.Empty;
        if (Options.PrintPacketData)
        {
            packetData = $"\n{packet.ToFormattedString()}";
        }

        Log($"Sending packet {packetType.Name} {byteSize}{targetDescription}{packetData}");
    }

    private static string GetBroadcastPeerDescription(Peer[] peers)
    {
        if (peers.Length == 0)
        {
            return "to everyone";
        }

        if (peers.Length == 1)
        {
            return $"to everyone except peer {peers[0].ID}";
        }

        string peerIds = peers.Select(peer => peer.ID).ToFormattedString();
        return $"to peers {peerIds}";
    }
}
