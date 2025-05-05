using Godot;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

namespace Template.Netcode;

public class PacketWriter : IDisposable
{
    public MemoryStream Stream { get; } = new();

    private readonly BinaryWriter _writer;

    public PacketWriter() => _writer = new BinaryWriter(Stream);

    public void Write<T>(T v)
    {
        if (v == null)
        {
            throw new ArgumentNullException(nameof(v));
        }

        Type t = v.GetType();

        if (t.IsPrimitive || t == typeof(string))
        {
            WritePrimitive(v);
            return;
        }

        if (t == typeof(Vector2))
        {
            WriteVector2((Vector2)(object)v);
            return;
        }

        if (t == typeof(Vector3))
        {
            WriteVector3((Vector3)(object)v);
            return;
        }

        if (t.IsEnum)
        {
            WriteEnum(v);
            return;
        }

        if (t.IsArray)
        {
            WriteArray((Array)(object)v);
            return;
        }

        if (t.IsGenericType)
        {
            WriteGeneric(v, t);
            return;
        }

        if (t.IsClass || t.IsValueType)
        {
            WriteStructOrClass(v, t);
            return;
        }

        throw new NotImplementedException("PacketWriter: " + t + " is not a supported type.");
    }

    private void WritePrimitive<T>(T v)
    {
        switch (v)
        {
            case byte k: _writer.Write(k); break;
            case sbyte k: _writer.Write(k); break;
            case char k: _writer.Write(k); break;
            case string k: _writer.Write(k); break;
            case bool k: _writer.Write(k); break;
            case short k: _writer.Write(k); break;
            case ushort k: _writer.Write(k); break;
            case int k: _writer.Write(k); break;
            case uint k: _writer.Write(k); break;
            case float k: _writer.Write(k); break;
            case double k: _writer.Write(k); break;
            case long k: _writer.Write(k); break;
            case ulong k: _writer.Write(k); break;
            default:
                throw new NotImplementedException("PacketWriter: " + v.GetType() + " is not a supported primitive type.");
        }
    }

    private void WriteVector2(Vector2 v)
    {
        Write(v.X);
        Write(v.Y);
    }

    private void WriteVector3(Vector3 v)
    {
        Write(v.X);
        Write(v.Y);
        Write(v.Z);
    }

    private void WriteEnum<T>(T v)
    {
        // Convert enum to byte and write
        Write((byte)Convert.ChangeType(v, typeof(byte)));
    }

    private void WriteArray(Array array)
    {
        // Write array length
        Write(array.Length);

        // Write each item in the array
        foreach (object item in array)
        {
            Write(item);
        }
    }

    private void WriteGeneric(object v, Type t)
    {
        // Get generic type definition
        Type g = t.GetGenericTypeDefinition();

        // Check if type is list
        if (g == typeof(IList<>) || g == typeof(List<>))
        {
            IList list = (IList)v;
            // Write list count
            Write(list.Count);

            // Write each item in the list
            foreach (object item in list)
            {
                Write(item);
            }
        }
        // Check if type is dictionary
        else if (g == typeof(IDictionary<,>) || g == typeof(Dictionary<,>))
        {
            IDictionary dict = (IDictionary)v;
            // Write dictionary count
            Write(dict.Count);

            // Write each key-value pair in the dictionary
            foreach (DictionaryEntry item in dict)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }
        else
        {
            throw new NotImplementedException("PacketWriter: " + t + " is not a supported generic type.");
        }
    }

    private void WriteStructOrClass<T>(T v, Type t)
    {
        // Serialize public instance fields in metadata order
        FieldInfo[] fields = t
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(field => field.MetadataToken)
            .ToArray();

        // Write each field value
        foreach (FieldInfo field in fields)
        {
            Write(field.GetValue(v));
        }

        // Serialize public instance properties with getters in metadata order
        PropertyInfo[] properties = t
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetCustomAttributes(typeof(NetExcludeAttribute), true).Length == 0)
            .OrderBy(property => property.MetadataToken)
            .ToArray();

        // Write each property value
        foreach (PropertyInfo property in properties)
        {
            Write(property.GetValue(v));
        }
    }

    public void Dispose()
    {
        Stream.Dispose();
        _writer.Dispose();

        GC.SuppressFinalize(this); // Not sure why this is needed
    }
}
