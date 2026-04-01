namespace __TEMPLATE__.Netcode;

/// <summary>
/// Declares opcode backing type metadata consumed by generated packet registry code.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PacketRegistryAttribute : System.Attribute
{
    /// <summary>
    /// Numeric opcode backing type used by generated packet registry code.
    /// </summary>
    public System.Type OpcodeType { get; }

    /// <summary>
    /// Creates a registry marker using <see cref="byte"/> opcodes.
    /// </summary>
    public PacketRegistryAttribute()
    {
        OpcodeType = typeof(byte);
    }

    /// <summary>
    /// Creates a registry marker using a custom opcode backing type.
    /// </summary>
    /// <param name="opcodeType">Numeric type used to encode packet opcodes on the wire.</param>
    public PacketRegistryAttribute(System.Type opcodeType)
    {
        System.ArgumentNullException.ThrowIfNull(opcodeType);
        OpcodeType = opcodeType;
    }
}
