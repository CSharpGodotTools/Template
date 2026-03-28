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

public class PacketWriter : IDisposable
{
    private sealed class PacketMemberMap
    {
        public required FieldInfo[] Fields { get; init; }
        public required PropertyInfo[] Properties { get; init; }
    }

    private static readonly ConcurrentDictionary<Type, PacketMemberMap> _structMemberCache = new();

    /// <summary>The backing in-memory stream containing the serialized bytes.</summary>
    public MemoryStream Stream { get; } = new();

    private readonly BinaryWriter _writer;

    /// <summary>
    /// Creates a packet writer backed by an in-memory stream.
    /// </summary>
    public PacketWriter()
    {
        _writer = new BinaryWriter(Stream);
    }

    /// <summary>
    /// Legacy reflection-based write; prefer PacketGen-generated Write methods for packet serialization.
    /// </summary>
    public void Write<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Type valueType = value.GetType();

        if (typeof(GamePacket).IsAssignableFrom(valueType))
        {
            throw new InvalidOperationException(
                $"PacketWriter: serializing nested {nameof(GamePacket)} types through legacy reflection fallback is not supported.");
        }

        if (IsPrimitiveLike(valueType))
        {
            WritePrimitive(value);
            return;
        }

        if (valueType == typeof(Vector2))
        {
            WriteVector2((Vector2)(object)value);
            return;
        }

        if (valueType == typeof(Vector3))
        {
            WriteVector3((Vector3)(object)value);
            return;
        }

        if (valueType == typeof(SysVector2))
        {
            WriteVector2Numerics((SysVector2)(object)value);
            return;
        }

        if (valueType.IsEnum)
        {
            WriteEnum(value);
            return;
        }

        if (valueType.IsArray)
        {
            WriteArray((Array)(object)value);
            return;
        }

        if (valueType.IsGenericType)
        {
            WriteGeneric(value, valueType);
            return;
        }

        if (valueType.IsClass || valueType.IsValueType)
        {
            WriteStructOrClass(value, valueType);
            return;
        }

        throw new NotImplementedException($"PacketWriter: {valueType} is not a supported type.");
    }

    /// <summary>
    /// Returns <c>true</c> for primitive types, <see cref="string"/>, and <see cref="decimal"/>.
    /// </summary>
    private static bool IsPrimitiveLike(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
    }

    /// <summary>
    /// Writes a primitive-like value directly to the underlying binary writer.
    /// </summary>
    private void WritePrimitive<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        WritePrimitiveByType(value.GetType(), value);
    }

    private void WritePrimitiveByType(Type type, object value)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte: _writer.Write((byte)value); break;
            case TypeCode.SByte: _writer.Write((sbyte)value); break;
            case TypeCode.Char: _writer.Write((char)value); break;
            case TypeCode.String: _writer.Write((string)value); break;
            case TypeCode.Boolean: _writer.Write((bool)value); break;
            case TypeCode.Int16: _writer.Write((short)value); break;
            case TypeCode.UInt16: _writer.Write((ushort)value); break;
            case TypeCode.Int32: _writer.Write((int)value); break;
            case TypeCode.UInt32: _writer.Write((uint)value); break;
            case TypeCode.Single: _writer.Write((float)value); break;
            case TypeCode.Double: _writer.Write((double)value); break;
            case TypeCode.Int64: _writer.Write((long)value); break;
            case TypeCode.UInt64: _writer.Write((ulong)value); break;
            case TypeCode.Decimal: _writer.Write((decimal)value); break;

            default:
                throw new NotImplementedException($"PacketWriter: {type} is not a supported primitive type.");
        }
    }

    /// <summary>
    /// Writes a <see cref="Vector2"/> as two consecutive <see cref="float"/> values.
    /// </summary>
    private void WriteVector2(Vector2 vector)
    {
        Write(vector.X);
        Write(vector.Y);
    }

    /// <summary>
    /// Writes a <see cref="System.Numerics.Vector2"/> as two consecutive <see cref="float"/> values.
    /// </summary>
    private void WriteVector2Numerics(SysVector2 vector)
    {
        Write(vector.X);
        Write(vector.Y);
    }

    /// <summary>
    /// Writes a <see cref="Vector3"/> as three consecutive <see cref="float"/> values.
    /// </summary>
    private void WriteVector3(Vector3 vector)
    {
        Write(vector.X);
        Write(vector.Y);
        Write(vector.Z);
    }

    /// <summary>
    /// Writes an enum value as a single <see cref="byte"/>.
    /// </summary>
    private void WriteEnum<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Type enumType = value.GetType();
        Type underlyingType = Enum.GetUnderlyingType(enumType);
        object primitiveValue = Convert.ChangeType(value, underlyingType)!;
        WritePrimitiveByType(underlyingType, primitiveValue);
    }

    /// <summary>
    /// Writes an array as a length-prefixed sequence of elements.
    /// </summary>
    private void WriteArray(Array array)
    {
        Write(array.Length);

        foreach (object item in array)
        {
            Write(item);
        }
    }

    /// <summary>
    /// Delegates writing of supported generic types (<see cref="List{T}"/>, <see cref="Dictionary{TKey, TValue}"/>) to specialized overloads.
    /// </summary>
    private void WriteGeneric(object value, Type valueType)
    {
        Type genericDefinition = valueType.GetGenericTypeDefinition();

        if (genericDefinition == typeof(IList<>) || genericDefinition == typeof(List<>))
        {
            WriteList((IList)value);
            return;
        }

        if (genericDefinition == typeof(IDictionary<,>) || genericDefinition == typeof(Dictionary<,>))
        {
            WriteDictionary((IDictionary)value);
            return;
        }

        throw new NotImplementedException($"PacketWriter: {valueType} is not a supported generic type.");
    }

    /// <summary>
    /// Writes a list as a length-prefixed sequence of elements.
    /// </summary>
    private void WriteList(IList list)
    {
        Write(list.Count);

        foreach (object item in list)
        {
            Write(item);
        }
    }

    /// <summary>
    /// Writes a dictionary as a count-prefixed sequence of key-value pairs.
    /// </summary>
    private void WriteDictionary(IDictionary dictionary)
    {
        Write(dictionary.Count);

        foreach (DictionaryEntry item in dictionary)
        {
            Write(item.Key);
            Write(item.Value);
        }
    }

    /// <summary>
    /// Writes all public fields and eligible properties of a struct or class in metadata token order.
    /// </summary>
    private void WriteStructOrClass<T>(T value, Type valueType)
    {
        PacketMemberMap members = GetMembersForStructOrClass(valueType);

        foreach (FieldInfo field in members.Fields)
        {
            Write(field.GetValue(value));
        }

        foreach (PropertyInfo property in members.Properties)
        {
            Write(property.GetValue(value));
        }
    }

    /// <summary>
    /// Returns a cached map of public fields and eligible properties for a struct or class type.
    /// </summary>
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
                .Where(ShouldIncludePropertyForWrite)
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
    /// Returns <c>true</c> for readable properties not decorated with <see cref="NetExcludeAttribute"/>.
    /// </summary>
    private static bool ShouldIncludePropertyForWrite(PropertyInfo property)
    {
        return property.CanRead
            && property.GetIndexParameters().Length == 0
            && property.GetCustomAttributes(typeof(NetExcludeAttribute), true).Length == 0;
    }

    private static int GetOrderOrFallback(MemberInfo member)
    {
        NetOrderAttribute? order = member.GetCustomAttribute<NetOrderAttribute>(inherit: true);
        return order?.Order ?? int.MaxValue;
    }

    /// <summary>
    /// Releases writer resources and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        _writer.Dispose();
        Stream.Dispose();
    }
}
