namespace PacketGen;

/// <summary>
/// Centralized string constants shared by PacketGen generators and tests.
/// </summary>
public static class PacketGenConstants
{
    public const string TemplateNamespaceToken = "__TEMPLATE__";
    public const string NetcodeNamespaceSuffix = ".Netcode";

    public const string ClientPacketTypeName = "ClientPacket";
    public const string ServerPacketTypeName = "ServerPacket";
    public const string PacketWriterTypeName = "PacketWriter";
    public const string PacketReaderTypeName = "PacketReader";
    public const string PacketRegistryAttributeTypeName = "PacketRegistryAttribute";
    public const string NetExcludeAttributeTypeName = "NetExcludeAttribute";
}
