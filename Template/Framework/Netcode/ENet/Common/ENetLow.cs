using ENet;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Framework.Netcode;

/// <summary>
/// Shared ENet worker-thread lifecycle used by client and server implementations.
/// </summary>
public abstract class ENetLow
{
    protected const byte DefaultChannelId = 0;
    private const int WorkerPollTimeoutMs = 15;

    /// <summary>ENet host handle managed by the concrete transport.</summary>
    protected Host Host { get; set; } = null!;

    /// <summary>Cancels the worker loop on shutdown.</summary>
    protected CancellationTokenSource CTS { get; set; } = null!;

    /// <summary>Logging and diagnostic settings for this transport.</summary>
    protected ENetOptions Options { get; set; } = null!;

    /// <summary>Packet types excluded from verbose logging output.</summary>
    protected HashSet<Type> IgnoredPackets { get; private set; } = [];

    protected long _running;

    /// <summary>True while the worker thread is active.</summary>
    public bool IsRunning => Interlocked.Read(ref _running) == 1;

    /// <summary>
    /// Logs a message with transport-specific context.
    /// </summary>
    public abstract void Log(object message, BBColor color);

    /// <summary>
    /// Requests shutdown for the transport implementation.
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// Performs shared cleanup after disconnect/timeout handling.
    /// </summary>
    protected virtual void OnDisconnectCleanup(Peer peer)
    {
        CTS?.Cancel();
    }

    /// <summary>
    /// Stores packet types that should be excluded from verbose logging.
    /// </summary>
    protected void InitIgnoredPackets(Type[] ignoredPackets)
    {
        if (ignoredPackets == null || ignoredPackets.Length == 0)
        {
            IgnoredPackets = [];
            return;
        }

        IgnoredPackets = [.. ignoredPackets];
    }

    /// <summary>
    /// Runs the ENet worker loop and dispatches network events.
    /// </summary>
    protected void WorkerLoop()
    {
        while (!CTS.IsCancellationRequested)
        {
            ConcurrentQueues();
            PumpNetworkEvents();
        }

        Host.Flush();
    }

    /// <summary>
    /// Polls the ENet host and dispatches all pending events for this tick.
    /// </summary>
    private void PumpNetworkEvents()
    {
        bool hasServiced = false;

        while (!hasServiced)
        {
            if (!TryGetNextEvent(out Event netEvent, out hasServiced))
            {
                break;
            }

            DispatchEvent(netEvent);
        }
    }

    /// <summary>
    /// Attempts to retrieve the next pending ENet event via <c>CheckEvents</c> then <c>Service</c>.
    /// </summary>
    /// <returns><c>true</c> when an event is available in <paramref name="netEvent"/>.</returns>
    private bool TryGetNextEvent(out Event netEvent, out bool hasServiced)
    {
        if (Host.CheckEvents(out netEvent) > 0)
        {
            hasServiced = false;
            return true;
        }

        if (Host.Service(WorkerPollTimeoutMs, out netEvent) > 0)
        {
            hasServiced = true;
            return true;
        }

        hasServiced = false;
        return false;
    }

    /// <summary>
    /// Routes a low-level ENet event to the matching lifecycle hook.
    /// </summary>
    private void DispatchEvent(Event netEvent)
    {
        switch (netEvent.Type)
        {
            case EventType.None:
                break;

            case EventType.Connect:
                OnConnectLow(netEvent);
                break;

            case EventType.Disconnect:
                OnDisconnectLow(netEvent);
                break;

            case EventType.Timeout:
                OnTimeoutLow(netEvent);
                break;

            case EventType.Receive:
                OnReceiveLow(netEvent);
                break;
        }
    }

    /// <summary>
    /// Processes queues owned by the concrete transport.
    /// </summary>
    protected abstract void ConcurrentQueues();

    /// <summary>
    /// Handles a low-level ENet connect event.
    /// </summary>
    protected abstract void OnConnectLow(Event netEvent);

    /// <summary>
    /// Handles a low-level ENet disconnect event.
    /// </summary>
    protected abstract void OnDisconnectLow(Event netEvent);

    /// <summary>
    /// Handles a low-level ENet timeout event.
    /// </summary>
    protected abstract void OnTimeoutLow(Event netEvent);

    /// <summary>
    /// Handles a low-level ENet packet receive event.
    /// </summary>
    protected abstract void OnReceiveLow(Event netEvent);

    /// <summary>
    /// Returns a human-readable byte-count string (e.g. "1 byte", "2 bytes"). Returns empty when byte-size logging is disabled.
    /// </summary>
    protected string FormatByteSize(long bytes)
    {
        if (!Options.PrintPacketByteSize)
        {
            return string.Empty;
        }

        return $"({bytes} byte{(bytes == 1 ? "" : "s")}) ";
    }

    /// <summary>
    /// Logs a <see cref="GamePacket"/> as formatted JSON.
    /// </summary>
    public void Log(GamePacket packet, BBColor color = BBColor.Gray)
    {
        Log($"\n{packet.ToFormattedString()}", color);
    }

    /// <summary>
    /// Creates a reliable ENet packet from a serialized byte buffer.
    /// </summary>
    protected static Packet CreateReliablePacket(byte[] data)
    {
        Packet packet = default;
        packet.Create(data, PacketFlags.Reliable);
        return packet;
    }
}
