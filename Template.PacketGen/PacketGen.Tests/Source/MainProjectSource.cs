using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGen.Tests;

internal static class MainProjectSource
{
    public static string NetExcludeAttribute => """
        public sealed class NetExcludeAttribute : System.Attribute {}
        """;

    public static string PacketRegistryAttribute => """
        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public sealed class PacketRegistryAttribute : System.Attribute
        {
            public System.Type OpcodeType { get; }
        
            public PacketRegistryAttribute()
            {
                OpcodeType = typeof(byte);
            }
        
            public PacketRegistryAttribute(System.Type opcodeType)
            {
                OpcodeType = opcodeType;
            }
        }
        """;

    public static string PacketStubs => """
        using System;
        using System.Collections.Generic;

        namespace Framework.Netcode;

        public abstract class GamePacket
        {
            public virtual void Write(PacketWriter writer) { }
            public virtual void Read(PacketReader reader) { }
        }
        
        public abstract class ClientPacket : GamePacket { }
        public abstract class ServerPacket : GamePacket { }
        
        public class PacketWriter 
        {
            public List<object?> Values { get; } = new();

            public void Write<T>(T v)
            {
                Values.Add(v);
            }
        }
        
        public class PacketReader 
        {
            private readonly Queue<object?> _values;

            public PacketReader(IEnumerable<object?> values)
            {
                _values = new Queue<object?>(values);
            }

            private T ReadValue<T>() => (T)_values.Dequeue()!;

            public byte    ReadByte()           => ReadValue<byte>();
            public sbyte   ReadSByte()          => ReadValue<sbyte>();
            public char    ReadChar()           => ReadValue<char>();
            public string  ReadString()         => ReadValue<string>();
            public bool    ReadBool()           => ReadValue<bool>();
            public short   ReadShort()          => ReadValue<short>();
            public ushort  ReadUShort()         => ReadValue<ushort>();
            public int     ReadInt()            => ReadValue<int>();
            public uint    ReadUInt()           => ReadValue<uint>();
            public float   ReadFloat()          => ReadValue<float>();
            public double  ReadDouble()         => ReadValue<double>();
            public decimal ReadDecimal()        => ReadValue<decimal>();
            public long    ReadLong()           => ReadValue<long>();
            public ulong   ReadULong()          => ReadValue<ulong>();
            public byte[]  ReadBytes(int count) => ReadValue<byte[]>();
            public byte[]  ReadBytes()          => ReadValue<byte[]>();
            public Godot.Vector2 ReadVector2()  => ReadValue<Godot.Vector2>();
            public Godot.Vector3 ReadVector3()  => ReadValue<Godot.Vector3>();

            public T Read<T>() => ReadValue<T>();

            public object Read(Type t) => _values.Dequeue()!;
        }
        """;
}
