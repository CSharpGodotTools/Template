namespace PacketGen;

/// <summary>
/// Centralized string constants shared by PacketGen generators and tests.
/// </summary>
public static class PacketGenConstants
{
    /// <summary>
    /// Placeholder token replaced with resolved project namespace during generation.
    /// </summary>
    public const string TemplateNamespaceToken = "__TEMPLATE__";

    /// <summary>
    /// Namespace suffix where runtime netcode packet types are expected.
    /// </summary>
    public const string NetcodeNamespaceSuffix = ".Netcode";

    /// <summary>
    /// Runtime base type name for client-bound packets.
    /// </summary>
    public const string ClientPacketTypeName = "ClientPacket";

    /// <summary>
    /// Runtime base type name for server-bound packets.
    /// </summary>
    public const string ServerPacketTypeName = "ServerPacket";

    /// <summary>
    /// Runtime packet writer type name.
    /// </summary>
    public const string PacketWriterTypeName = "PacketWriter";

    /// <summary>
    /// Runtime packet reader type name.
    /// </summary>
    public const string PacketReaderTypeName = "PacketReader";

    /// <summary>
    /// Attribute type name that marks packet registry classes.
    /// </summary>
    public const string PacketRegistryAttributeTypeName = "PacketRegistryAttribute";

    /// <summary>
    /// Attribute type name used to exclude packet members from generation.
    /// </summary>
    public const string NetExcludeAttributeTypeName = "NetExcludeAttribute";
}
