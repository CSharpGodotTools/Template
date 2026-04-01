using System.Reflection;

namespace PacketGen.Tests;

/// <summary>
/// Reflection helpers for inspecting generated packet types.
/// </summary>
internal static class PacketReflectionHelper
{
    /// <summary>
    /// Asserts that a packet type overrides Write/Read methods.
    /// </summary>
    /// <param name="packetType">Packet type under test.</param>
    /// <param name="writerType">Writer type.</param>
    /// <param name="readerType">Reader type.</param>
    public static void AssertHasWriteReadMethods(Type packetType, Type writerType, Type readerType)
    {
        MethodInfo? write = FindMethod(packetType, "Write", writerType);
        Assert.That(write, Is.Not.Null, $"Write({writerType.Name}) not found on {packetType.FullName}.");
        Assert.That(write!.GetBaseDefinition().DeclaringType, Is.Not.EqualTo(write.DeclaringType), $"Write is not an override on {packetType.FullName}.");

        MethodInfo? read = FindMethod(packetType, "Read", readerType);
        Assert.That(read, Is.Not.Null, $"Read({readerType.Name}) not found on {packetType.FullName}.");
        Assert.That(read!.GetBaseDefinition().DeclaringType, Is.Not.EqualTo(read.DeclaringType), $"Read is not an override on {packetType.FullName}.");
    }

    /// <summary>
    /// Creates a packet instance for the provided type.
    /// </summary>
    /// <param name="packetType">Packet type to instantiate.</param>
    /// <returns>Created packet instance.</returns>
    public static object CreatePacketInstance(Type packetType)
    {
        object? instance = Activator.CreateInstance(packetType);
        Assert.That(instance, Is.Not.Null, $"Failed to create instance of '{packetType.FullName}'.");
        return instance!;
    }

    /// <summary>
    /// Sets a property value by name on the provided instance.
    /// </summary>
    /// <param name="instance">Target instance.</param>
    /// <param name="propertyName">Property name.</param>
    /// <param name="value">Value to assign.</param>
    public static void SetProperty(object instance, string propertyName, object? value)
    {
        PropertyInfo? prop = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.That(prop, Is.Not.Null, $"Property '{propertyName}' not found on {instance.GetType().FullName}.");
        prop!.SetValue(instance, value);
    }

    /// <summary>
    /// Gets a property value by name from the provided instance.
    /// </summary>
    /// <param name="instance">Target instance.</param>
    /// <param name="propertyName">Property name.</param>
    /// <returns>Property value.</returns>
    public static object? GetProperty(object instance, string propertyName)
    {
        PropertyInfo? prop = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.That(prop, Is.Not.Null, $"Property '{propertyName}' not found on {instance.GetType().FullName}.");
        return prop!.GetValue(instance);
    }

    /// <summary>
    /// Invokes the Write method on a packet instance.
    /// </summary>
    /// <param name="packet">Packet instance.</param>
    /// <param name="writer">Writer instance.</param>
    public static void InvokeWrite(object packet, object writer)
    {
        MethodInfo? write = FindMethod(packet.GetType(), "Write", writer.GetType());
        Assert.That(write, Is.Not.Null, $"Write method not found on {packet.GetType().FullName}.");
        write!.Invoke(packet, [writer]);
    }

    /// <summary>
    /// Invokes the Read method on a packet instance.
    /// </summary>
    /// <param name="packet">Packet instance.</param>
    /// <param name="reader">Reader instance.</param>
    public static void InvokeRead(object packet, object reader)
    {
        MethodInfo? read = FindMethod(packet.GetType(), "Read", reader.GetType());
        Assert.That(read, Is.Not.Null, $"Read method not found on {packet.GetType().FullName}.");
        read!.Invoke(packet, [reader]);
    }

    /// <summary>
    /// Finds a method with the given name and parameter type.
    /// </summary>
    /// <param name="type">Type to search.</param>
    /// <param name="name">Method name.</param>
    /// <param name="parameterType">Expected parameter type.</param>
    /// <returns>Matching method info, or null when not found.</returns>
    private static MethodInfo? FindMethod(Type type, string name, Type parameterType)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m =>
            {
                // Match by method name first.
                if (m.Name != name)
                    return false;

                ParameterInfo[] parameters = m.GetParameters();
                // Match methods with a single parameter of the expected type.
                return parameters.Length == 1 && parameters[0].ParameterType == parameterType;
            });
    }
}
