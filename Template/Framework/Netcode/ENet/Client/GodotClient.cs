using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Netcode.Client;

/// <summary>
/// Main-thread client facade. Provides packet handler registration and exposes
/// connection lifecycle events that fire on the Godot main thread.
/// </summary>
public abstract class GodotClient : ENetClient
{
    private const string LogTag = "Client";
    private readonly ConcurrentDictionary<Type, Action<ServerPacket>> _serverPacketHandlers = new();

    protected GodotClient()
    {
        // subclasses should register packet handlers in their constructors
    }


    /// <summary>
    /// Registers a handler for incoming <typeparamref name="TPacket"/> packets, dispatched on the Godot main thread.
    /// </summary>
    protected void OnPacket<TPacket>(Action<TPacket> handler)
        where TPacket : ServerPacket
    {
        ArgumentNullException.ThrowIfNull(handler);

        _serverPacketHandlers[typeof(TPacket)] = packet => handler((TPacket)packet);
    }

    /// <summary>
    /// Fires when the client connects to the server.
    /// </summary>
    public event Action? Connected;

    /// <summary>
    /// Fires when the client disconnects or times out from the server.
    /// </summary>
    public event Action<DisconnectOpcode>? Disconnected;

    /// <summary>
    /// Fires when the client times out from the server.
    /// </summary>
    public event Action? TimedOut;

    /// <summary>
    /// Is the client connected to the server?
    /// </summary>
    public bool IsConnected => Interlocked.Read(ref _connected) == 1;

    /// <summary>
    /// Connects to the server at <paramref name="ip"/>:<paramref name="port"/>. Options control logging; types in ignoredPackets are excluded.
    /// </summary>
    public async Task Connect(string ip, ushort port, ENetOptions? options = null, params Type[] ignoredPackets)
    {
        if (IsRunning)
        {
            Log("Client is running already");
            return;
        }

        Options = options ?? new ENetOptions();
        InitIgnoredPackets(ignoredPackets);

        CTS = new CancellationTokenSource();

        try
        {
            await Task.Factory.StartNew(
                () => WorkerThread(ip, port),
                CTS.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping the client.
        }
        catch (Exception exception)
        {
            Interlocked.Exchange(ref _running, 0);
            GameFramework.Logger.LogErr(exception, LogTag);
        }
    }

    /// <summary>
    /// Stop the client.
    /// </summary>
    public sealed override void Stop()
    {
        if (!IsRunning)
        {
            Log("Client has stopped already");
            return;
        }

        RequestDisconnect();
    }

    /// <summary>
    /// Sends a packet to the server. Packets are reliable by default.
    /// </summary>
    public void Send(ClientPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (!IsConnected)
        {
            Log($"Can not send packet '{packet.GetType()}' because client is not connected to the server");
            return;
        }

        packet.Write();
        LogOutgoing(packet);
        EnqueueOutgoing(packet.GetData());
    }

    /// <summary>
    /// Call this in <c>_PhysicsProcess</c> (or equivalent) on the Godot main thread.
    /// </summary>
    public void HandlePackets()
    {
        ProcessGodotPackets();
        ProcessGodotCommands();
    }

    /// <summary>
    /// Reads and dispatches pending server packets from the relay queue.
    /// </summary>
    private void ProcessGodotPackets()
    {
        while (MainThreadPackets.TryDequeue(out PacketData? packetData))
        {
            PacketReader packetReader = packetData.PacketReader;
            ServerPacket packet = packetData.HandlePacket;
            Type packetType = packetData.Type;

            try
            {
                packet.Read(packetReader);

                if (!_serverPacketHandlers.TryGetValue(packetType, out Action<ServerPacket>? handler))
                {
                    Log($"No handler registered for server packet {packetType.Name} (Ignoring)");
                    continue;
                }

                handler(packet);
                LogReceivedPacket(packetType, packet);
            }
            catch (Exception exception)
            {
                GameFramework.Logger.LogErr(exception, LogTag);
            }
            finally
            {
                packetReader.Dispose();
            }
        }
    }

    /// <summary>
    /// Reads and dispatches pending lifecycle commands (connected, disconnected, timeout) from the relay queue.
    /// </summary>
    private void ProcessGodotCommands()
    {
        while (MainThreadCommands.TryDequeue(out Cmd<GodotOpcode>? command))
        {
            switch (command.Opcode)
            {
                case GodotOpcode.Connected:
                    TryInvoke(() => Connected?.Invoke());
                    break;

                case GodotOpcode.Disconnected:
                    DisconnectOpcode disconnectOpcode = (DisconnectOpcode)command.Data[0];
                    TryInvoke(() => Disconnected?.Invoke(disconnectOpcode));
                    break;

                case GodotOpcode.Timeout:
                    TryInvoke(() => TimedOut?.Invoke());
                    break;
            }
        }
    }

    /// <summary>
    /// Logs an incoming server packet when packet-received logging is enabled.
    /// </summary>
    private void LogReceivedPacket(Type packetType, ServerPacket packet)
    {
        if (!Options.PrintPacketReceived || IgnoredPackets.Contains(packetType))
        {
            return;
        }

        string packetData = string.Empty;
        if (Options.PrintPacketData)
        {
            packetData = $"\n{packet.ToFormattedString()}";
        }

        Log($"Received packet: {packetType.Name}{packetData}");
    }

    /// <summary>
    /// Logs an outgoing packet when packet-sent logging is enabled.
    /// </summary>
    private void LogOutgoing(ClientPacket packet)
    {
        Type packetType = packet.GetType();

        if (!Options.PrintPacketSent || IgnoredPackets.Contains(packetType))
        {
            return;
        }

        string packetData = string.Empty;
        if (Options.PrintPacketData)
        {
            packetData = $"\n{packet.ToFormattedString()}";
        }

        Log($"Sent packet: {packetType.Name} {FormatByteSize(packet.GetSize())}{packetData}");
    }

    /// <summary>
    /// Invokes an action, catching and logging any exceptions thrown during event dispatch.
    /// </summary>
    private static void TryInvoke(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            GameFramework.Logger.LogErr(exception, LogTag);
        }
    }
}
