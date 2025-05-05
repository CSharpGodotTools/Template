using ENet;
using System.Linq;
using System.Reflection;

namespace Template.Netcode;

/// <summary>
/// A base class with common functionality for Client and Server packets
/// </summary>
public abstract class GamePacket
{
    private PropertyInfo[] _cachedProperties;

    public static int MaxSize { get; } = 8192;

    protected Peer[] Peers { get; set; }
    protected byte ChannelId { get; }

    // Packets are reliable by default
    private readonly PacketFlags _packetFlags = PacketFlags.Reliable;
    private long _size;
    private byte[] _data;

    public void Write()
    {
        using PacketWriter writer = new();
        writer.Write(GetOpcode());
        this?.Write(writer);

        _data = writer.Stream.ToArray();
        _size = writer.Stream.Length;
    }

    public void SetPeer(Peer peer)
    {
        Peers = [peer];
    }

    public void SetPeers(Peer[] peers)
    {
        Peers = peers;
    }

    public long GetSize()
    {
        return _size;
    }

    public abstract byte GetOpcode();

    public virtual void Write(PacketWriter writer)
    {
        PropertyInfo[] properties = GetProperties();

        foreach (PropertyInfo property in properties)
        {
            writer.Write(property.GetValue(this));
        }
    }

    public virtual void Read(PacketReader reader)
    {
        PropertyInfo[] properties = GetProperties();

        foreach (PropertyInfo property in properties)
        {
            property.SetValue(this, reader.Read(property.PropertyType));
        }
    }

    private PropertyInfo[] GetProperties()
    {
        _cachedProperties ??= GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetCustomAttributes(typeof(NetSendAttribute), true).Length != 0)
            .OrderBy(p => ((NetSendAttribute)p.GetCustomAttributes(typeof(NetSendAttribute), true).First()).Order)
            .ToArray();

        return _cachedProperties;
    }

    protected Packet CreateENetPacket()
    {
        Packet enetPacket = default;
        enetPacket.Create(_data, _packetFlags);
        return enetPacket;
    }
}

