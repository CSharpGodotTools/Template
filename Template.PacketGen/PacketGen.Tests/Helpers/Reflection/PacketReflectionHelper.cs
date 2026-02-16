using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace PacketGen.Tests;

internal static class PacketReflectionHelper
{
    public static void AssertHasWriteReadMethods(Type packetType, Type writerType, Type readerType)
    {
        MethodInfo? write = FindMethod(packetType, "Write", writerType);
        Assert.That(write, Is.Not.Null, $"Write({writerType.Name}) not found on {packetType.FullName}.");
        Assert.That(write!.GetBaseDefinition().DeclaringType, Is.Not.EqualTo(write.DeclaringType), $"Write is not an override on {packetType.FullName}.");

        MethodInfo? read = FindMethod(packetType, "Read", readerType);
        Assert.That(read, Is.Not.Null, $"Read({readerType.Name}) not found on {packetType.FullName}.");
        Assert.That(read!.GetBaseDefinition().DeclaringType, Is.Not.EqualTo(read.DeclaringType), $"Read is not an override on {packetType.FullName}.");
    }

    public static object CreatePacketInstance(Type packetType)
    {
        object? instance = Activator.CreateInstance(packetType);
        Assert.That(instance, Is.Not.Null, $"Failed to create instance of '{packetType.FullName}'.");
        return instance!;
    }

    public static void SetProperty(object instance, string propertyName, object? value)
    {
        PropertyInfo? prop = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.That(prop, Is.Not.Null, $"Property '{propertyName}' not found on {instance.GetType().FullName}.");
        prop!.SetValue(instance, value);
    }

    public static object? GetProperty(object instance, string propertyName)
    {
        PropertyInfo? prop = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.That(prop, Is.Not.Null, $"Property '{propertyName}' not found on {instance.GetType().FullName}.");
        return prop!.GetValue(instance);
    }

    public static void InvokeWrite(object packet, object writer)
    {
        MethodInfo? write = FindMethod(packet.GetType(), "Write", writer.GetType());
        Assert.That(write, Is.Not.Null, $"Write method not found on {packet.GetType().FullName}.");
        write!.Invoke(packet, [writer]);
    }

    public static void InvokeRead(object packet, object reader)
    {
        MethodInfo? read = FindMethod(packet.GetType(), "Read", reader.GetType());
        Assert.That(read, Is.Not.Null, $"Read method not found on {packet.GetType().FullName}.");
        read!.Invoke(packet, [reader]);
    }

    private static MethodInfo? FindMethod(Type type, string name, Type parameterType)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m =>
            {
                if (m.Name != name)
                    return false;

                ParameterInfo[] parameters = m.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == parameterType;
            });
    }
}
