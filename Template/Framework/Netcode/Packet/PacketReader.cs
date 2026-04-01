using Godot;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SysVector2 = System.Numerics.Vector2;

namespace __TEMPLATE__.Netcode;

/// <summary>
/// Reads packet payload bytes into primitive, collection, and object graph values.
/// </summary>
public class PacketReader : IDisposable
{
    private sealed class PacketMemberMap
    {
        public required FieldInfo[] Fields { get; init; }
        public required PropertyInfo[] Properties { get; init; }
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
    /// <param name="packet">ENet packet containing serialized payload bytes.</param>
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
    /// Creates a packet reader directly from a managed byte buffer (no ENet packet involved).
    /// Used internally after fragment reassembly.
    /// </summary>
    /// <param name="data">Serialized payload bytes.</param>
    internal PacketReader(byte[] data)
    {
        _readBuffer = data;
        _stream = new MemoryStream(_readBuffer, writable: false);
        _reader = new BinaryReader(_stream);
    }

    /// <summary>
    /// Reads a <see cref="byte"/> value.
    /// </summary>
    /// <returns>Read byte value.</returns>
    public byte ReadByte() => _reader.ReadByte();

    /// <summary>
    /// Reads an <see cref="sbyte"/> value.
    /// </summary>
    /// <returns>Read signed byte value.</returns>
    public sbyte ReadSByte() => _reader.ReadSByte();

    /// <summary>
    /// Reads a <see cref="char"/> value.
    /// </summary>
    /// <returns>Read character value.</returns>
    public char ReadChar() => _reader.ReadChar();

    /// <summary>
    /// Reads a <see cref="string"/> value.
    /// </summary>
    /// <returns>Read string value.</returns>
    public string ReadString() => _reader.ReadString();

    /// <summary>
    /// Reads a <see cref="bool"/> value.
    /// </summary>
    /// <returns>Read boolean value.</returns>
    public bool ReadBool() => _reader.ReadBoolean();

    /// <summary>
    /// Reads a <see cref="short"/> value.
    /// </summary>
    /// <returns>Read 16-bit signed integer.</returns>
    public short ReadShort() => _reader.ReadInt16();

    /// <summary>
    /// Reads a <see cref="ushort"/> value.
    /// </summary>
    /// <returns>Read 16-bit unsigned integer.</returns>
    public ushort ReadUShort() => _reader.ReadUInt16();

    /// <summary>
    /// Reads an <see cref="int"/> value.
    /// </summary>
    /// <returns>Read 32-bit signed integer.</returns>
    public int ReadInt() => _reader.ReadInt32();

    /// <summary>
    /// Reads a <see cref="uint"/> value.
    /// </summary>
    /// <returns>Read 32-bit unsigned integer.</returns>
    public uint ReadUInt() => _reader.ReadUInt32();

    /// <summary>
    /// Reads a <see cref="float"/> value.
    /// </summary>
    /// <returns>Read single-precision floating-point value.</returns>
    public float ReadFloat() => _reader.ReadSingle();

    /// <summary>
    /// Reads a <see cref="double"/> value.
    /// </summary>
    /// <returns>Read double-precision floating-point value.</returns>
    public double ReadDouble() => _reader.ReadDouble();

    /// <summary>
    /// Reads a <see cref="long"/> value.
    /// </summary>
    /// <returns>Read 64-bit signed integer.</returns>
    public long ReadLong() => _reader.ReadInt64();

    /// <summary>
    /// Reads a <see cref="ulong"/> value.
    /// </summary>
    /// <returns>Read 64-bit unsigned integer.</returns>
    public ulong ReadULong() => _reader.ReadUInt64();

    /// <summary>
    /// Reads a <see cref="decimal"/> value.
    /// </summary>
    /// <returns>Read decimal value.</returns>
    public decimal ReadDecimal() => _reader.ReadDecimal();

    /// <summary>
    /// Reads a fixed number of bytes.
    /// </summary>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>Buffer containing up to <paramref name="count"/> bytes.</returns>
    public byte[] ReadBytes(int count) => _reader.ReadBytes(count);

    /// <summary>
    /// Reads a length-prefixed byte array.
    /// </summary>
    /// <returns>Read byte array payload.</returns>
    public byte[] ReadBytes() => ReadBytes(ReadInt());

    /// <summary>
    /// Reads a <see cref="Vector2"/> value.
    /// </summary>
    /// <returns>Read <see cref="Vector2"/> value.</returns>
    public Vector2 ReadVector2() => new(ReadFloat(), ReadFloat());

    /// <summary>
    /// Reads a <see cref="Vector3"/> value.
    /// </summary>
    /// <returns>Read <see cref="Vector3"/> value.</returns>
    public Vector3 ReadVector3() => new(ReadFloat(), ReadFloat(), ReadFloat());

    /// <summary>
    /// Reads a <see cref="System.Numerics.Vector2"/> value.
    /// </summary>
    /// <returns>Read <see cref="System.Numerics.Vector2"/> value.</returns>
    public SysVector2 ReadVector2Numerics() => new(ReadFloat(), ReadFloat());

    /// <summary>
    /// Legacy reflection-based read; prefer PacketGen-generated Read methods for packet deserialization.
    /// </summary>
    /// <typeparam name="T">Target type to read.</typeparam>
    /// <returns>Deserialized value of type <typeparamref name="T"/>.</returns>
    public T Read<T>()
    {
        Type type = typeof(T);
        return ReadTyped<T>(type);
    }

    /// <summary>
    /// Legacy reflection-based read; prefer PacketGen-generated Read methods for packet deserialization.
    /// </summary>
    /// <param name="type">Runtime type to deserialize.</param>
    /// <returns>Deserialized object instance.</returns>
    public object Read(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        MethodInfo readMethod = _genericReadMethod.MakeGenericMethod(type);
        return readMethod.Invoke(this, null)!;
    }

    /// <summary>
    /// Dispatches a read call to the appropriate typed reader based on the runtime type.
    /// </summary>
    /// <param name="type">Runtime type to deserialize.</param>
    /// <typeparam name="T">Target return type.</typeparam>
    /// <returns>Deserialized value cast to <typeparamref name="T"/>.</returns>
    private T ReadTyped<T>(Type type)
    {
        // Disallow nested packet deserialization through the reflection fallback path.
        if (typeof(GamePacket).IsAssignableFrom(type))
        {
            throw new InvalidOperationException(
                $"PacketReader: deserializing nested {nameof(GamePacket)} types through legacy reflection fallback is not supported.");
        }

        // Handle primitive-like built-in values directly.
        if (IsPrimitiveLike(type))
        {
            return ReadPrimitive<T>(type);
        }

        // Handle Godot Vector2 values with dedicated read helpers.
        if (type == typeof(Vector2))
        {
            return (T)(object)ReadVector2();
        }

        // Handle Godot Vector3 values with dedicated read helpers.
        if (type == typeof(Vector3))
        {
            return (T)(object)ReadVector3();
        }

        // Handle System.Numerics Vector2 values with dedicated read helpers.
        if (type == typeof(SysVector2))
        {
            return (T)(object)ReadVector2Numerics();
        }

        // Delegate supported generic collections to specialized readers.
        if (type.IsGenericType)
        {
            return ReadGeneric<T>(type);
        }

        // Deserialize enum values via their underlying primitive type.
        if (type.IsEnum)
        {
            return ReadEnum<T>();
        }

        // Read arrays by deserializing each element.
        if (type.IsArray)
        {
            Type? elementType = type.GetElementType();
            return (T)(object)ReadArray(elementType!);
        }

        // Fallback to member-wise reflection for structs and classes.
        if (type.IsValueType || type.IsClass)
        {
            return ReadStructOrClass<T>(type);
        }

        throw new NotImplementedException($"PacketReader: {type} is not a supported type.");
    }

    /// <summary>
    /// Returns <c>true</c> for primitive types, <see cref="string"/>, and <see cref="decimal"/>.
    /// </summary>
    /// <param name="type">Runtime type to inspect.</param>
    /// <returns><see langword="true"/> when the type is treated as primitive-like.</returns>
    private static bool IsPrimitiveLike(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
    }

    /// <summary>
    /// Reads a primitive-like value from the underlying binary reader.
    /// </summary>
    /// <param name="type">Runtime primitive-like type to read.</param>
    /// <typeparam name="T">Target return type.</typeparam>
    /// <returns>Deserialized primitive-like value cast to <typeparamref name="T"/>.</returns>
    private T ReadPrimitive<T>(Type type)
    {
        object value = ReadPrimitiveObject(type);
        return (T)value;
    }

    /// <summary>
    /// Reads a primitive-like value based on an explicit runtime type.
    /// </summary>
    /// <param name="type">Runtime primitive type to read.</param>
    /// <returns>Boxed primitive value.</returns>
    private object ReadPrimitiveObject(Type type)
    {
        return type switch
        {
            Type t when t == typeof(byte) => ReadByte(),
            Type t when t == typeof(sbyte) => ReadSByte(),
            Type t when t == typeof(char) => ReadChar(),
            Type t when t == typeof(string) => ReadString(),
            Type t when t == typeof(bool) => ReadBool(),
            Type t when t == typeof(short) => ReadShort(),
            Type t when t == typeof(ushort) => ReadUShort(),
            Type t when t == typeof(int) => ReadInt(),
            Type t when t == typeof(uint) => ReadUInt(),
            Type t when t == typeof(float) => ReadFloat(),
            Type t when t == typeof(double) => ReadDouble(),
            Type t when t == typeof(long) => ReadLong(),
            Type t when t == typeof(ulong) => ReadULong(),
            Type t when t == typeof(decimal) => ReadDecimal(),
            _ => throw new NotImplementedException(
                $"PacketReader: {type} is not a supported primitive type.")
        };
    }

    /// <summary>
    /// Reads an enum value from a single byte.
    /// </summary>
    /// <typeparam name="T">Enum type to deserialize.</typeparam>
    /// <returns>Deserialized enum value.</returns>
    private T ReadEnum<T>()
    {
        Type enumType = typeof(T);
        Type underlyingType = Enum.GetUnderlyingType(enumType);
        object primitiveValue = ReadPrimitiveObject(underlyingType);
        return (T)Enum.ToObject(enumType, primitiveValue);
    }

    /// <summary>
    /// Delegates reading of supported generic types (<see cref="List{T}"/>, <see cref="Dictionary{TKey, TValue}"/>) to specialized overloads.
    /// </summary>
    /// <param name="genericType">Closed generic runtime type to deserialize.</param>
    /// <typeparam name="T">Target return type.</typeparam>
    /// <returns>Deserialized generic value cast to <typeparamref name="T"/>.</returns>
    private T ReadGeneric<T>(Type genericType)
    {
        Type genericDefinition = genericType.GetGenericTypeDefinition();

        // Read list-like collections as count-prefixed sequences.
        if (genericDefinition == typeof(IList<>) || genericDefinition == typeof(List<>))
        {
            return ReadList<T>(genericType);
        }

        // Read dictionary-like collections as count-prefixed key/value entries.
        if (genericDefinition == typeof(IDictionary<,>) || genericDefinition == typeof(Dictionary<,>))
        {
            return ReadDictionary<T>(genericType);
        }

        throw new NotImplementedException($"PacketReader: {genericType} is not a supported generic type.");
    }

    /// <summary>
    /// Reads a length-prefixed list of elements.
    /// </summary>
    /// <param name="listType">Closed list type to deserialize.</param>
    /// <typeparam name="T">Target return type.</typeparam>
    /// <returns>Deserialized list value cast to <typeparamref name="T"/>.</returns>
    private T ReadList<T>(Type listType)
    {
        Type valueType = listType.GetGenericArguments()[0];
        IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(valueType))!;

        int count = ReadInt();
        for (int index = 0; index < count; index++)
        {
            list.Add(Read(valueType));
        }

        return (T)list;
    }

    /// <summary>
    /// Reads a count-prefixed sequence of key-value pairs into a dictionary.
    /// </summary>
    /// <param name="dictionaryType">Closed dictionary type to deserialize.</param>
    /// <typeparam name="T">Target return type.</typeparam>
    /// <returns>Deserialized dictionary value cast to <typeparamref name="T"/>.</returns>
    private T ReadDictionary<T>(Type dictionaryType)
    {
        Type keyType = dictionaryType.GetGenericArguments()[0];
        Type valueType = dictionaryType.GetGenericArguments()[1];
        IDictionary dictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType))!;

        int count = ReadInt();
        for (int index = 0; index < count; index++)
        {
            object key = Read(keyType);
            object value = Read(valueType);
            dictionary.Add(key, value);
        }

        return (T)dictionary;
    }

    /// <summary>
    /// Reads a length-prefixed array of elements.
    /// </summary>
    /// <param name="elementType">Array element type.</param>
    /// <returns>Deserialized array instance.</returns>
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

    /// <summary>
    /// Reads all public fields and eligible properties of a struct or class in metadata token order.
    /// </summary>
    /// <param name="type">Runtime struct or class type to deserialize.</param>
    /// <typeparam name="T">Target return type.</typeparam>
    /// <returns>Deserialized object cast to <typeparamref name="T"/>.</returns>
    private T ReadStructOrClass<T>(Type type)
    {
        object instance = Activator.CreateInstance(type)!;
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

    /// <summary>
    /// Returns a cached map of public fields and eligible properties for a struct or class type.
    /// </summary>
    /// <param name="type">Runtime type to inspect for serializable members.</param>
    /// <returns>Cached field/property member map for the type.</returns>
    private static PacketMemberMap GetMembersForStructOrClass(Type type)
    {
        return _structMemberCache.GetOrAdd(type, static cachedType =>
        {
            FieldInfo[] fields = [.. cachedType
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(GetOrderOrFallback)
                .ThenBy(field => field.MetadataToken)];

            PropertyInfo[] properties = [.. cachedType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(ShouldIncludePropertyForRead)
                .OrderBy(GetOrderOrFallback)
                .ThenBy(property => property.MetadataToken)];

            return new PacketMemberMap
            {
                Fields = fields,
                Properties = properties
            };
        });
    }

    /// <summary>
    /// Returns <c>true</c> for writable properties not decorated with <see cref="NetExcludeAttribute"/>.
    /// </summary>
    /// <param name="property">Property metadata to evaluate.</param>
    /// <returns><see langword="true"/> when the property should be deserialized.</returns>
    private static bool ShouldIncludePropertyForRead(PropertyInfo property)
    {
        return property.CanWrite
            && property.GetIndexParameters().Length == 0
            && property.GetCustomAttributes(typeof(NetExcludeAttribute), true).Length == 0;
    }

    /// <summary>
    /// Gets explicit read order from <see cref="NetOrderAttribute"/> or fallback ordering sentinel.
    /// </summary>
    /// <param name="member">Field/property metadata member.</param>
    /// <returns>Configured order value or <see cref="int.MaxValue"/>.</returns>
    private static int GetOrderOrFallback(MemberInfo member)
    {
        NetOrderAttribute? order = member.GetCustomAttribute<NetOrderAttribute>(inherit: true);
        return order?.Order ?? int.MaxValue;
    }

    /// <summary>
    /// Releases reader resources and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        _reader.Dispose();
        _stream.Dispose();
    }
}
