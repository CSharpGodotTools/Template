using Godot;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Framework.Netcode;

public class PacketWriter : IDisposable
{
    private sealed class PacketMemberMap
    {
        public FieldInfo[] Fields { get; init; }
        public PropertyInfo[] Properties { get; init; }
    }

    private static readonly ConcurrentDictionary<Type, PacketMemberMap> _structMemberCache = new();

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
    /// Legacy reflection-based write fallback retained for compatibility.
    /// PacketGen generates packet read/write source and should be preferred for packet serialization.
    /// </summary>
    public void Write<T>(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        Type valueType = value.GetType();

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

    private static bool IsPrimitiveLike(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
    }

    private void WritePrimitive<T>(T value)
    {
        switch (value)
        {
            case byte primitive: _writer.Write(primitive); break;
            case sbyte primitive: _writer.Write(primitive); break;
            case char primitive: _writer.Write(primitive); break;
            case string primitive: _writer.Write(primitive); break;
            case bool primitive: _writer.Write(primitive); break;
            case short primitive: _writer.Write(primitive); break;
            case ushort primitive: _writer.Write(primitive); break;
            case int primitive: _writer.Write(primitive); break;
            case uint primitive: _writer.Write(primitive); break;
            case float primitive: _writer.Write(primitive); break;
            case double primitive: _writer.Write(primitive); break;
            case long primitive: _writer.Write(primitive); break;
            case ulong primitive: _writer.Write(primitive); break;
            case decimal primitive: _writer.Write(primitive); break;

            default:
                throw new NotImplementedException($"PacketWriter: {value.GetType()} is not a supported primitive type.");
        }
    }

    private void WriteVector2(Vector2 vector)
    {
        Write(vector.X);
        Write(vector.Y);
    }

    private void WriteVector3(Vector3 vector)
    {
        Write(vector.X);
        Write(vector.Y);
        Write(vector.Z);
    }

    private void WriteEnum<T>(T value)
    {
        Write((byte)Convert.ChangeType(value, typeof(byte)));
    }

    private void WriteArray(Array array)
    {
        Write(array.Length);

        foreach (object item in array)
        {
            Write(item);
        }
    }

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

    private void WriteList(IList list)
    {
        Write(list.Count);

        foreach (object item in list)
        {
            Write(item);
        }
    }

    private void WriteDictionary(IDictionary dictionary)
    {
        Write(dictionary.Count);

        foreach (DictionaryEntry item in dictionary)
        {
            Write(item.Key);
            Write(item.Value);
        }
    }

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

    private static PacketMemberMap GetMembersForStructOrClass(Type type)
    {
        return _structMemberCache.GetOrAdd(type, static cachedType =>
        {
            FieldInfo[] fields = [.. cachedType
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(field => field.MetadataToken)];

            PropertyInfo[] properties = [.. cachedType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(ShouldIncludePropertyForWrite)
                .OrderBy(property => property.MetadataToken)];

            return new PacketMemberMap
            {
                Fields = fields,
                Properties = properties
            };
        });
    }

    private static bool ShouldIncludePropertyForWrite(PropertyInfo property)
    {
        return property.CanRead
            && property.GetCustomAttributes(typeof(NetExcludeAttribute), true).Length == 0;
    }

    /// <summary>
    /// Releases writer resources and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        _writer.Dispose();
        Stream.Dispose();
        GC.SuppressFinalize(this);
    }
}
