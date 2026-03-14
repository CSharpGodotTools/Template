namespace PacketGen.Tests;

internal static class PacketGenTestConstants
{
    public const string PacketNamespace = PacketGenConstants.TemplateNamespaceToken + PacketGenConstants.NetcodeNamespaceSuffix;
    public const string PacketWriterFullName = PacketNamespace + "." + PacketGenConstants.PacketWriterTypeName;
    public const string PacketReaderFullName = PacketNamespace + "." + PacketGenConstants.PacketReaderTypeName;
}
