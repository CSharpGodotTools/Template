using Godot;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Framework.Netcode;

public class PacketReader : IDisposable
{
    private sealed class PacketMemberMap
    {
        public FieldInfo[] Fields { get; init; }
        public PropertyInfo[] Properties { get; init; }
    }

    private static readonly MethodInfo _genericReadMethod = typeof(PacketReader)
        .GetMethods(BindingFlags.Instance | BindingFlags.Public)
        .First(method => method.IsGenericMethod && method.Name == nameof(Read));

    private static readonly ConcurrentDictionary<Type, PacketMemberMap> _structMemberCache = new();

    private readonly MemoryStream _stream;
    private readonly BinaryReader _reader;
    private readonly byte[] _readBuffer;

    /// <summary>
    /// Creates a packet reader from an ENet packet payload.
    /// </summary>
    public PacketReader(ENet.Packet packet)
    {
        int packetLength = packet.Length;
        _readBuffer = new byte[packetLength];
        packet.CopyTo(_readBuffer);
        packet.Dispose();

        _stream = new MemoryStream(_readBuffer, writable: false);
        _reader = new BinaryReader(_stream);
    }

    /// <summary>
    /// Reads a <see cref="byte"/> value.
    /// </summary>
    public byte ReadByte() => _reader.ReadByte();

    /// <summary>
    /// Reads an <see cref="sbyte"/> value.
    /// </summary>
    public sbyte ReadSByte() => _reader.ReadSByte();

    /// <summary>
    /// Reads a <see cref="char"/> value.
    /// </summary>
    public char ReadChar() => _reader.ReadChar();

    /// <summary>
    /// Reads a <see cref="string"/> value.
    /// </summary>
    public string ReadString() => _reader.ReadString();

    /// <summary>
    /// Reads a <see cref="bool"/> value.
    /// </summary>
    public bool ReadBool() => _reader.ReadBoolean();

    /// <summary>
    /// Reads a <see cref="short"/> value.
    /// </summary>
    public short ReadShort() => _reader.ReadInt16();

    /// <summary>
    /// Reads a <see cref="ushort"/> value.
    /// </summary>
    public ushort ReadUShort() => _reader.ReadUInt16();

    /// <summary>
    /// Reads an <see cref="int"/> value.
    /// </summary>
    public int ReadInt() => _reader.ReadInt32();

    /// <summary>
    /// Reads a <see cref="uint"/> value.
    /// </summary>
    public uint ReadUInt() => _reader.ReadUInt32();

    /// <summary>
    /// Reads a <see cref="float"/> value.
    /// </summary>
    public float ReadFloat() => _reader.ReadSingle();

    /// <summary>
    /// Reads a <see cref="double"/> value.
    /// </summary>
    public double ReadDouble() => _reader.ReadDouble();

    /// <summary>
    /// Reads a <see cref="long"/> value.
    /// </summary>
    public long ReadLong() => _reader.ReadInt64();

    /// <summary>
    /// Reads a <see cref="ulong"/> value.
    /// </summary>
    public ulong ReadULong() => _reader.ReadUInt64();

    /// <summary>
    /// Reads a <see cref="decimal"/> value.
    /// </summary>
    public decimal ReadDecimal() => _reader.ReadDecimal();

    /// <summary>
    /// Reads a fixed number of bytes.
    /// </summary>
    public byte[] ReadBytes(int count) => _reader.ReadBytes(count);

    /// <summary>
    /// Reads a length-prefixed byte array.
    /// </summary>
    public byte[] ReadBytes() => ReadBytes(ReadInt());

    /// <summary>
    /// Reads a <see cref="Vector2"/> value.
    /// </summary>
    public Vector2 ReadVector2() => new(ReadFloat(), ReadFloat());

    /// <summary>
    /// Reads a <see cref="Vector3"/> value.
    /// </summary>
    public Vector3 ReadVector3() => new(ReadFloat(), ReadFloat(), ReadFloat());

    /// <summary>
    /// Legacy reflection-based read fallback retained for compatibility.
    /// PacketGen generates packet read/write source and should be preferred for packet serialization.
    /// </summary>
    public T Read<T>()
    {
        Type type = typeof(T);
        return ReadTyped<T>(type);
    }

    /// <summary>
    /// Legacy reflection-based read fallback retained for compatibility.
    /// PacketGen generates packet read/write source and should be preferred for packet serialization.
    /// </summary>
    public object Read(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        MethodInfo readMethod = _genericReadMethod.MakeGenericMethod(type);
        return readMethod.Invoke(this, null);
    }

    private T ReadTyped<T>(Type type)
    {
        if (IsPrimitiveLike(type))
        {
            return ReadPrimitive<T>(type);
        }

        if (type == typeof(Vector2))
        {
            return (T)(object)ReadVector2();
        }

        if (type == typeof(Vector3))
        {
            return (T)(object)ReadVector3();
        }

        if (type.IsGenericType)
        {
            return ReadGeneric<T>(type);
        }

        if (type.IsEnum)
        {
            return ReadEnum<T>();
        }

        if (type.IsArray)
        {
            Type elementType = type.GetElementType();
            return (T)(object)ReadArray(elementType);
        }

        if (type.IsValueType || type.IsClass)
        {
            return ReadStructOrClass<T>(type);
        }

        throw new NotImplementedException($"PacketReader: {type} is not a supported type.");
    }

    private static bool IsPrimitiveLike(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
    }

    private T ReadPrimitive<T>(Type type)
    {
        if (type == typeof(byte)) return (T)(object)ReadByte();
        if (type == typeof(sbyte)) return (T)(object)ReadSByte();
        if (type == typeof(char)) return (T)(object)ReadChar();
        if (type == typeof(string)) return (T)(object)ReadString();
        if (type == typeof(bool)) return (T)(object)ReadBool();
        if (type == typeof(short)) return (T)(object)ReadShort();
        if (type == typeof(ushort)) return (T)(object)ReadUShort();
        if (type == typeof(int)) return (T)(object)ReadInt();
        if (type == typeof(uint)) return (T)(object)ReadUInt();
        if (type == typeof(float)) return (T)(object)ReadFloat();
        if (type == typeof(double)) return (T)(object)ReadDouble();
        if (type == typeof(long)) return (T)(object)ReadLong();
        if (type == typeof(ulong)) return (T)(object)ReadULong();
        if (type == typeof(decimal)) return (T)(object)ReadDecimal();

        throw new NotImplementedException($"PacketReader: {type} is not a supported primitive type.");
    }

    private T ReadEnum<T>()
    {
        return (T)Enum.ToObject(typeof(T), ReadByte());
    }

    private T ReadGeneric<T>(Type genericType)
    {
        Type genericDefinition = genericType.GetGenericTypeDefinition();

        if (genericDefinition == typeof(IList<>) || genericDefinition == typeof(List<>))
        {
            return ReadList<T>(genericType);
        }

        if (genericDefinition == typeof(IDictionary<,>) || genericDefinition == typeof(Dictionary<,>))
        {
            return ReadDictionary<T>(genericType);
        }

        throw new NotImplementedException($"PacketReader: {genericType} is not a supported generic type.");
    }

    private T ReadList<T>(Type listType)
    {
        Type valueType = listType.GetGenericArguments()[0];
        IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(valueType));

        int count = ReadInt();
        for (int index = 0; index < count; index++)
        {
            list.Add(Read(valueType));
        }

        return (T)list;
    }

    private T ReadDictionary<T>(Type dictionaryType)
    {
        Type keyType = dictionaryType.GetGenericArguments()[0];
        Type valueType = dictionaryType.GetGenericArguments()[1];
        IDictionary dictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));

        int count = ReadInt();
        for (int index = 0; index < count; index++)
        {
            object key = Read(keyType);
            object value = Read(valueType);
            dictionary.Add(key, value);
        }

        return (T)dictionary;
    }

    private Array ReadArray(Type elementType)
    {
        int count = ReadInt();
        Array array = Array.CreateInstance(elementType, count);
        for (int index = 0; index < count; index++)
        {
            array.SetValue(Read(elementType), index);
        }

        return array;
    }

    private T ReadStructOrClass<T>(Type type)
    {
        object instance = Activator.CreateInstance(type);
        PacketMemberMap members = GetMembersForStructOrClass(type);

        foreach (FieldInfo field in members.Fields)
        {
            field.SetValue(instance, Read(field.FieldType));
        }

        foreach (PropertyInfo property in members.Properties)
        {
            property.SetValue(instance, Read(property.PropertyType));
        }

        return (T)instance;
    }

    private static PacketMemberMap GetMembersForStructOrClass(Type type)
    {
        return _structMemberCache.GetOrAdd(type, static cachedType =>
        {
            FieldInfo[] fields = [.. cachedType
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(field => field.MetadataToken)];

            PropertyInfo[] properties = [.. cachedType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(ShouldIncludePropertyForRead)
                .OrderBy(property => property.MetadataToken)];

            return new PacketMemberMap
            {
                Fields = fields,
                Properties = properties
            };
        });
    }

    private static bool ShouldIncludePropertyForRead(PropertyInfo property)
    {
        return property.CanWrite
            && property.GetCustomAttributes(typeof(NetExcludeAttribute), true).Length == 0;
    }

    /// <summary>
    /// Releases reader resources and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        _reader.Dispose();
        _stream.Dispose();
        GC.SuppressFinalize(this);
    }
}
