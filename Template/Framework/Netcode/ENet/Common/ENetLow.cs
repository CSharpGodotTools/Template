using ENet;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace __TEMPLATE__.Netcode;

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
    private ILoggerService? _loggerService;

    protected long _running;

    protected ILoggerService LoggerService =>
        _loggerService ?? throw new InvalidOperationException("ENet logger service was not configured.");

    /// <summary>True while the worker thread is active.</summary>
    public bool IsRunning => Interlocked.Read(ref _running) == 1;

    /// <summary>
    /// Configures logger service used by transport implementations.
    /// </summary>
    /// <param name="loggerService">Logger service instance.</param>
    public void ConfigureLoggerService(ILoggerService loggerService)
    {
        _loggerService = loggerService;
    }

    /// <summary>
    /// Logs a message with transport-specific context.
    /// </summary>
    /// <param name="message">Message payload to emit.</param>
    /// <param name="color">Color token used for rendering.</param>
    public abstract void Log(object message, BBColor color);

    /// <summary>
    /// Requests shutdown for the transport implementation.
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// Performs shared cleanup after disconnect/timeout handling.
    /// </summary>
    /// <param name="peer">Peer associated with the disconnect or timeout event.</param>
    protected virtual void OnDisconnectCleanup(Peer peer)
    {
        CTS?.Cancel();
    }

    /// <summary>
    /// Stores packet types that should be excluded from verbose logging.
    /// </summary>
    /// <param name="ignoredPackets">Packet types to ignore in verbose packet logging output.</param>
    protected void InitIgnoredPackets(Type[] ignoredPackets)
    {
        // Use an empty set when no ignored packet types were provided.
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
            // Stop this poll cycle when there are no more pending events.
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
    /// <param name="netEvent">Retrieved ENet event when available.</param>
    /// <param name="hasServiced">Whether <c>Service</c> was called during retrieval.</param>
    /// <returns><c>true</c> when an event is available in <paramref name="netEvent"/>.</returns>
    private bool TryGetNextEvent(out Event netEvent, out bool hasServiced)
    {
        // Prefer queued events before waiting on service polling.
        if (Host.CheckEvents(out netEvent) > 0)
        {
            hasServiced = false;
            return true;
        }

        // Fall back to timed service polling for fresh network events.
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
    /// <param name="netEvent">Low-level ENet event to dispatch.</param>
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
    /// <param name="netEvent">Connect event payload from ENet.</param>
    protected abstract void OnConnectLow(Event netEvent);

    /// <summary>
    /// Handles a low-level ENet disconnect event.
    /// </summary>
    /// <param name="netEvent">Disconnect event payload from ENet.</param>
    protected abstract void OnDisconnectLow(Event netEvent);

    /// <summary>
    /// Handles a low-level ENet timeout event.
    /// </summary>
    /// <param name="netEvent">Timeout event payload from ENet.</param>
    protected abstract void OnTimeoutLow(Event netEvent);

    /// <summary>
    /// Handles a low-level ENet packet receive event.
    /// </summary>
    /// <param name="netEvent">Receive event payload from ENet.</param>
    protected abstract void OnReceiveLow(Event netEvent);

    /// <summary>
    /// Returns a human-readable byte-count string (e.g. "1 byte", "2 bytes"). Returns empty when byte-size logging is disabled.
    /// </summary>
    /// <param name="bytes">Byte count to format.</param>
    /// <returns>Formatted byte-size suffix or empty string when disabled.</returns>
    protected string FormatByteSize(long bytes)
    {
        // Omit byte-size decorations when packet-size logging is disabled.
        if (!Options.PrintPacketByteSize)
        {
            return string.Empty;
        }

        return $"({bytes} byte{(bytes == 1 ? "" : "s")}) ";
    }

    /// <summary>
    /// Logs a non-fatal outgoing packet send failure with a consistent context-specific message.
    /// </summary>
    /// <param name="exception">Exception thrown while sending an outgoing packet.</param>
    /// <param name="logTag">Context tag identifying sender role in logs.</param>
    protected void LogOutgoingSendFailure(Exception exception, string logTag)
    {
        string message = exception switch
        {
            ObjectDisposedException => $"{logTag}: outgoing packet target disposed",
            InvalidOperationException => $"{logTag}: invalid outgoing packet state",
            ArgumentException => $"{logTag}: invalid outgoing packet arguments",
            _ => logTag,
        };

        LoggerService.LogErr(exception, message);
    }

    /// <summary>
    /// Logs a <see cref="GamePacket"/> as formatted JSON.
    /// </summary>
    /// <param name="packet">Packet to render and log.</param>
    /// <param name="color">Color token used for rendering.</param>
    public void Log(GamePacket packet, BBColor color = BBColor.Gray)
    {
        Log($"\n{packet.ToFormattedString()}", color);
    }

    /// <summary>
    /// Creates a reliable ENet packet from a serialized byte buffer.
    /// </summary>
    /// <param name="data">Serialized packet bytes.</param>
    /// <returns>Reliable ENet packet created from the provided bytes.</returns>
    protected static Packet CreateReliablePacket(byte[] data)
    {
        Packet packet = default;
        packet.Create(data, PacketFlags.Reliable);
        return packet;
    }
}
