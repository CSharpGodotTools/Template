using GodotUtils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Netcode.Server;

public abstract class GodotServer : ENetServer
{
    private const string LogTag = "Server";

    public GodotServer()
    {
        RegisterPackets();
    }

    /// <summary>
    /// Register all packet handlers for this server.
    /// </summary>
    protected abstract void RegisterPackets();

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
        RequestKick(id, opcode);
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

        RequestStop();
    }

    /// <summary>
    /// Send a packet to one client by peer ID. Thread safe.
    /// </summary>
    public void Send(ServerPacket packet, uint peerId)
    {
        ArgumentNullException.ThrowIfNull(packet);

        packet.Write();
        LogSend(packet, $"to client {peerId}");
        EnqueueOutgoing(OutgoingMessage.Unicast(packet.GetData(), peerId));
    }

    /// <summary>
    /// Broadcast a packet to all connected clients. Thread safe.
    /// </summary>
    public void Broadcast(ServerPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        packet.Write();
        LogSend(packet, "to everyone");
        EnqueueOutgoing(OutgoingMessage.Broadcast(packet.GetData()));
    }

    /// <summary>
    /// Broadcast a packet to all clients except the specified peer. Thread safe.
    /// </summary>
    public void Broadcast(ServerPacket packet, uint excludePeerId)
    {
        ArgumentNullException.ThrowIfNull(packet);

        packet.Write();
        LogSend(packet, $"to everyone except {excludePeerId}");
        EnqueueOutgoing(OutgoingMessage.BroadcastExcept(packet.GetData(), excludePeerId));
    }

    private void LogSend(ServerPacket packet, string targetDescription)
    {
        Type packetType = packet.GetType();

        if (!Options.PrintPacketSent || IgnoredPackets.Contains(packetType))
        {
            return;
        }

        string byteSize = FormatByteSize(packet.GetSize());

        string packetData = string.Empty;
        if (Options.PrintPacketData)
        {
            packetData = $"\n{packet.ToFormattedString()}";
        }

        Log($"Sending packet {packetType.Name} {byteSize}{targetDescription}{packetData}");
    }
}
