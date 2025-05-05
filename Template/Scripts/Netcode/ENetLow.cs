using ENet;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace Template.Netcode;

/// <summary>
/// ENetServer and ENetClient both extend from this class.
/// </summary>
public abstract class ENetLow
{
    public static bool ENetInitialized { get; set; }

    static ENetLow()
    {
        try
        {
            Library.Initialize();
            ENetInitialized = true;
        }
        catch (DllNotFoundException e)
        {
            Game.LogErr(e);
            ENetInitialized = false;
        }
    }

    public bool IsRunning => Interlocked.Read(ref _running) == 1;
    public abstract void Log(object message, BBColor color);
    public abstract void Stop();

    protected Host Host { get; set; }
    protected CancellationTokenSource CTS { get; set; }
    protected List<Type> IgnoredPackets { get; set; } = [];
    
    protected ENetOptions Options;

    protected virtual void DisconnectCleanup(Peer peer)
    {
        CTS.Cancel();
    }

    protected void InitIgnoredPackets(Type[] ignoredPackets)
    {
        IgnoredPackets = ignoredPackets.ToList();
    }

    protected void WorkerLoop()
    {
        while (!CTS.IsCancellationRequested)
        {
            bool polled = false;

            ConcurrentQueues();

            while (!polled)
            {
                if (Host.CheckEvents(out Event netEvent) <= 0)
                {
                    if (Host.Service(15, out netEvent) <= 0)
                    {
                        break;
                    }

                    polled = true;
                }

                switch (netEvent.Type)
                {
                    case EventType.None:
                        // do nothing
                        break;
                    case EventType.Connect:
                        Connect(netEvent);
                        break;
                    case EventType.Disconnect:
                        Disconnect(netEvent);
                        break;
                    case EventType.Timeout:
                        Timeout(netEvent);
                        break;
                    case EventType.Receive:
                        Receive(netEvent);
                        break;
                }
            }
        }

        Host.Flush();
        _running = 0;
        Stopped();
    }

    /// <summary>
    /// A simple function that transforms the number of bytes into a readable string. For
    /// example if bytes is 1 then "1 byte" is returned. If bytes is 2 then "2 bytes" is 
    /// returned. A empty string is returned if printing the packet size is disabled in
    /// options.
    /// </summary>
    protected string FormatByteSize(long bytes)
    {
        return Options.PrintPacketByteSize ? $"({bytes} byte{(bytes == 1 ? "" : "s")}) " : "";
    }

    protected abstract void Stopped();
    protected virtual void Starting() { }
    protected abstract void Connect(Event netEvent);
    protected abstract void Disconnect(Event netEvent);
    protected abstract void Timeout(Event netEvent);
    protected abstract void Receive(Event netEvent);
    protected abstract void ConcurrentQueues();

    protected long _running;
}

public class ENetOptions
{
    public bool PrintPacketData { get; set; } = false;
    public bool PrintPacketByteSize { get; set; } = false;
    public bool PrintPacketReceived { get; set; } = true;
    public bool PrintPacketSent { get; set; } = true;
}

public enum DisconnectOpcode
{
    Disconnected,
    Maintenance,
    Restarting,
    Stopping,
    Timeout,
    Kicked,
    Banned
}

