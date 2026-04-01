namespace PacketGen.Tests;

/// <summary>
/// Shared type-name constants used by PacketGen test fixtures.
/// </summary>
internal static class PacketGenTestConstants
{
    /// <summary>
    /// Expected packet namespace used by generated test sources.
    /// </summary>
    public const string PacketNamespace = PacketGenConstants.TemplateNamespaceToken + PacketGenConstants.NetcodeNamespaceSuffix;

    /// <summary>
    /// Fully qualified packet writer type name.
    /// </summary>
    public const string PacketWriterFullName = PacketNamespace + "." + PacketGenConstants.PacketWriterTypeName;

    /// <summary>
    /// Fully qualified packet reader type name.
    /// </summary>
    public const string PacketReaderFullName = PacketNamespace + "." + PacketGenConstants.PacketReaderTypeName;
}
