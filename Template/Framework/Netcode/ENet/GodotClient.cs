using GodotUtils;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Netcode.Client;

public abstract class GodotClient : ENetClient
{
    private const string LogTag = "Client";
    private readonly ConcurrentDictionary<Type, Action<ServerPacket>> _serverPacketHandlers = new();

    protected GodotClient()
    {
        RegisterPackets();
    }

    /// <summary>
    /// Register all packet handlers for this client.
    /// </summary>
    protected abstract void RegisterPackets();

    /// <summary>
    /// Registers a handler for a specific server packet type.
    /// Handlers run on the Godot main thread.
    /// </summary>
    protected void OnPacket<TPacket>(Action<TPacket> handler)
        where TPacket : ServerPacket
    {
        ArgumentNullException.ThrowIfNull(handler);

        _serverPacketHandlers[typeof(TPacket)] = packet => handler((TPacket)packet);
    }

    /// <summary>
    /// Fires when the client connects to the server. Thread safe.
    /// </summary>
    public event Action Connected;

    /// <summary>
    /// Fires when the client disconnects or times out from the server. Thread safe.
    /// </summary>
    public event Action<DisconnectOpcode> Disconnected;

    /// <summary>
    /// Fires when the client times out from the server. Thread safe.
    /// </summary>
    public event Action TimedOut;

    /// <summary>
    /// Is the client connected to the server? Thread safe.
    /// </summary>
    public bool IsConnected => Interlocked.Read(ref _connected) == 1;

    /// <summary>
    /// <para>
    /// Thread-safe connect entrypoint. IP can be set to "127.0.0.1" for localhost and
    /// port can be set to values such as 25565.
    /// </para>
    ///
    /// <para>
    /// Options contains logging controls. Ignored packets skip logging output.
    /// </para>
    /// </summary>
    public async Task Connect(string ip, ushort port, ENetOptions options = default, params Type[] ignoredPackets)
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
    /// Stop the client. This function is thread safe.
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
    /// Sends a packet to the server. Packets are reliable by default. Thread safe.
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

    private void ProcessGodotPackets()
    {
        while (MainThreadPackets.TryDequeue(out PacketData packetData))
        {
            PacketReader packetReader = packetData.PacketReader;
            ServerPacket packet = packetData.HandlePacket;
            Type packetType = packetData.Type;

            try
            {
                packet.Read(packetReader);

                if (!_serverPacketHandlers.TryGetValue(packetType, out Action<ServerPacket> handler))
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

    private void ProcessGodotCommands()
    {
        while (MainThreadCommands.TryDequeue(out Cmd<GodotOpcode> command))
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
