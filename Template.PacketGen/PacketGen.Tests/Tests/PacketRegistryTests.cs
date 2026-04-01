using System.Collections;
using System.Reflection;

namespace PacketGen.Tests;

[TestFixture]
internal sealed class PacketRegistryTests
{
    [Test]
    public void Registry_UsesConfiguredOpcodeTypeAndSortedOpcodes()
    {
        string testCode = $$"""
        namespace {{PacketGenTestConstants.PacketNamespace}};

        public partial class CPacketZulu : ClientPacket {}
        public partial class CPacketAlpha : ClientPacket {}
        public partial class CPacketBeta : ClientPacket {}

        public partial class SPacketGamma : ServerPacket {}
        public partial class SPacketAlpha : ServerPacket {}

        {{MainProjectSource.PacketRegistryAttribute}}

        [PacketRegistry(typeof(ushort))]
        public partial class PacketRegistry
        {
        }
        """;

        GeneratorTestRunResult result = RunAndRequireGeneratedSource(testCode, "PacketRegistry.g.cs");
        string source = result.GeneratedSource;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(source, Does.Contain("Dictionary<ushort, Type> ClientPacketTypes"));
            Assert.That(source, Does.Contain("Dictionary<ushort, Type> ServerPacketTypes"));
        }

        GeneratedFileStore fileStore = new();
        fileStore.Write(result.GeneratedFile, source);
        Assembly assembly = GeneratedAssemblyCompiler.Compile(result, fileStore);
        Type registryType = assembly.GetType(PacketGenTestConstants.PacketNamespace + ".PacketRegistry")!;

        IDictionary clientInfo = ReadDictionaryField(registryType, "ClientPacketInfo");
        IDictionary serverInfo = ReadDictionaryField(registryType, "ServerPacketInfo");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(clientInfo, Has.Count.EqualTo(3));
            Assert.That(serverInfo, Has.Count.EqualTo(2));
        }

        AssertOpcode(clientInfo, PacketGenTestConstants.PacketNamespace + ".CPacketAlpha", 0);
        AssertOpcode(clientInfo, PacketGenTestConstants.PacketNamespace + ".CPacketBeta", 1);
        AssertOpcode(clientInfo, PacketGenTestConstants.PacketNamespace + ".CPacketZulu", 2);

        AssertOpcode(serverInfo, PacketGenTestConstants.PacketNamespace + ".SPacketAlpha", 0);
        AssertOpcode(serverInfo, PacketGenTestConstants.PacketNamespace + ".SPacketGamma", 1);
    }

    [Test]
    public void Registry_DefaultOpcodeType_IsByte()
    {
        string testCode = $$"""
        namespace {{PacketGenTestConstants.PacketNamespace}};

        public partial class CPacketOnly : ClientPacket {}

        {{MainProjectSource.PacketRegistryAttribute}}

        [PacketRegistry]
        public partial class PacketRegistry
        {
        }
        """;

        GeneratorTestRunResult result = RunAndRequireGeneratedSource(testCode, "PacketRegistry.g.cs");
        Assert.That(result.GeneratedSource, Does.Contain("Dictionary<byte, Type> ClientPacketTypes"));
    }

    /// <summary>
    /// Reads a generated registry dictionary field via reflection.
    /// </summary>
    /// <param name="registryType">Generated PacketRegistry type.</param>
    /// <param name="fieldName">Static field name to retrieve.</param>
    /// <returns>Dictionary field value.</returns>
    private static IDictionary ReadDictionaryField(Type registryType, string fieldName)
    {
        FieldInfo? field = registryType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        Assert.That(field, Is.Not.Null, $"Registry field '{fieldName}' was not generated.");

        object? value = field!.GetValue(null);
        Assert.That(value, Is.Not.Null, $"Registry field '{fieldName}' was null.");

        return (IDictionary)value!;
    }

    /// <summary>
    /// Asserts that a packet type entry exists and carries the expected opcode.
    /// </summary>
    /// <param name="packetInfoDictionary">Registry dictionary keyed by packet type.</param>
    /// <param name="packetTypeName">Fully qualified packet type name.</param>
    /// <param name="expectedOpcode">Expected opcode value.</param>
    private static void AssertOpcode(IDictionary packetInfoDictionary, string packetTypeName, int expectedOpcode)
    {
        foreach (DictionaryEntry entry in packetInfoDictionary)
        {
            Type packetType = (Type)entry.Key;
            // Skip entries until the requested packet type is found.
            if (packetType.FullName != packetTypeName)
            {
                continue;
            }

            object packetInfo = entry.Value!;
            FieldInfo opcodeField = packetInfo.GetType().GetField("Opcode", BindingFlags.Public | BindingFlags.Instance)!;
            int opcode = Convert.ToInt32(opcodeField.GetValue(packetInfo)!);
            Assert.That(opcode, Is.EqualTo(expectedOpcode), $"Unexpected opcode for '{packetTypeName}'.");
            return;
        }

        Assert.Fail($"Packet type '{packetTypeName}' was not found in PacketRegistry.");
    }

    /// <summary>
    /// Runs the packet generator and fails the test when the expected file is not produced.
    /// </summary>
    /// <param name="source">Input source used for generator execution.</param>
    /// <param name="generatedFile">Expected generated file name.</param>
    /// <returns>Successful generator run result.</returns>
    private static GeneratorTestRunResult RunAndRequireGeneratedSource(string source, string generatedFile)
    {
        GeneratorTestOptions options = new GeneratorTestBuilder<PacketGenerator>(source)
            .WithGeneratedFile(generatedFile)
            .Build();

        GeneratorTestRunResult? result = GeneratorTestRunner<PacketGenerator>.Run(options);
        Assert.That(result, Is.Not.Null, $"{generatedFile} failed to generate.");
        return result!;
    }
}
