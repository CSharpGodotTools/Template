using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace __TEMPLATE__.Netcode.Client;

/// <summary>
/// Main-thread client facade. Provides packet handler registration and exposes
/// connection lifecycle events that fire on the Godot main thread.
/// </summary>
public abstract class GodotClient : ENetClient
{
    private const string LogTag = "Client";
    private readonly ConcurrentDictionary<Type, Action<ServerPacket>> _serverPacketHandlers = new();

    /// <summary>
    /// Initializes a new client facade.
    /// </summary>
    protected GodotClient()
    {
        // subclasses should register packet handlers in their constructors
    }


    /// <summary>
    /// Registers a handler for incoming <typeparamref name="TPacket"/> packets, dispatched on the Godot main thread.
    /// </summary>
    /// <typeparam name="TPacket">Server packet type handled by the callback.</typeparam>
    /// <param name="handler">Handler invoked for each received packet of the registered type.</param>
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
    /// <param name="ip">Server IP or host name.</param>
    /// <param name="port">Server UDP port.</param>
    /// <param name="options">Optional ENet runtime options; defaults are used when null.</param>
    /// <param name="ignoredPackets">Packet types excluded from verbose packet logging.</param>
    /// <returns>A task that completes when the client worker loop exits.</returns>
    public async Task Connect(string ip, ushort port, ENetOptions? options = null, params Type[] ignoredPackets)
    {
        // Ignore duplicate connect requests while client is already running.
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
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            Interlocked.Exchange(ref _running, 0);
            LoggerService.LogErr(exception, LogTag);
        }
    }

    /// <summary>
    /// Stop the client.
    /// </summary>
    public sealed override void Stop()
    {
        // Treat repeated stop requests as a no-op.
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
    /// <param name="packet">Client packet instance to serialize and enqueue.</param>
    public void Send(ClientPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        // Reject sends when not currently connected.
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

                // Skip packets without a registered handler callback.
                if (!_serverPacketHandlers.TryGetValue(packetType, out Action<ServerPacket>? handler))
                {
                    Log($"No handler registered for server packet {packetType.Name} (Ignoring)");
                    continue;
                }

                handler(packet);
                LogReceivedPacket(packetType, packet);
            }
            catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
            {
                LoggerService.LogErr(exception, LogTag);
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
    /// <param name="packetType">Runtime type of the received packet.</param>
    /// <param name="packet">Packet payload used for optional formatted dump output.</param>
    private void LogReceivedPacket(Type packetType, ServerPacket packet)
    {
        // Skip receive logging when disabled or packet type is ignored.
        if (!Options.PrintPacketReceived || IgnoredPackets.Contains(packetType))
            return;

        string packetData = string.Empty;

        // Include payload dump only when verbose packet-data logging is enabled.
        if (Options.PrintPacketData)
            packetData = $"\n{packet.ToFormattedString()}";

        Log($"Received packet: {packetType.Name}{packetData}");
    }

    /// <summary>
    /// Logs an outgoing packet when packet-sent logging is enabled.
    /// </summary>
    /// <param name="packet">Packet payload being sent.</param>
    private void LogOutgoing(ClientPacket packet)
    {
        Type packetType = packet.GetType();

        // Skip send logging when disabled or packet type is ignored.
        if (!Options.PrintPacketSent || IgnoredPackets.Contains(packetType))
            return;

        string packetData = string.Empty;

        // Include payload dump only when verbose packet-data logging is enabled.
        if (Options.PrintPacketData)
            packetData = $"\n{packet.ToFormattedString()}";

        Log($"Sent packet: {packetType.Name} {FormatByteSize(packet.GetSize())}{packetData}");
    }

    /// <summary>
    /// Invokes an action, catching and logging any exceptions thrown during event dispatch.
    /// </summary>
    /// <param name="action">Action to invoke safely.</param>
    private void TryInvoke(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception) when (ExceptionGuard.IsNonFatal(exception))
        {
            LoggerService.LogErr(exception, LogTag);
        }
    }
}
