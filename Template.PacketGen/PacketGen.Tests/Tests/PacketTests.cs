using NUnit.Framework;

namespace PacketGen.Tests;

[TestFixture]
internal sealed class PacketTests
{
    [Test]
    public void NetExcludeAttribute_ExcludesPropertyFromGeneratedReadWrite()
    {
        string testCode = $$"""
        using Godot;
        using Framework.Netcode;

        namespace TestPackets;

        {{MainProjectSource.NetExcludeAttribute}}

        public partial class CPacketPlayerPosition : ClientPacket
        {
            public uint Id { get; set; }
            public Vector2 Position { get; set; }

            [NetExclude]
            public Vector2 PrevPosition { get; set; }
        }
        """;

        GeneratorTestRunResult result = RunAndRequireGeneratedSource(testCode, "CPacketPlayerPosition.g.cs");
        string source = result.GeneratedSource;

        GeneratedFileStore fileStore = new();
        fileStore.Write(result.GeneratedFile, source);

        int idWriteIndex = source.IndexOf("writer.Write(Id);");
        int positionWriteIndex = source.IndexOf("writer.Write(Position);");
        int idReadIndex = source.IndexOf("Id = reader.ReadUInt();");
        int positionReadIndex = source.IndexOf("Position = reader.ReadVector2();");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(source, Does.Contain("public override void Write(PacketWriter writer)"));
            Assert.That(source, Does.Contain("public override void Read(PacketReader reader)"));

            Assert.That(idWriteIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(positionWriteIndex, Is.GreaterThan(idWriteIndex));
            Assert.That(idReadIndex, Is.GreaterThan(positionWriteIndex));
            Assert.That(positionReadIndex, Is.GreaterThan(idReadIndex));

            Assert.That(source, Does.Not.Contain("writer.Write(PrevPosition);"));
            Assert.That(source, Does.Not.Contain("PrevPosition = reader.ReadVector2();"));
        }
    }

    [Test]
    public void EmptyPacket_DoesNotGenerateSource()
    {
        string testCode = $$"""
        using Framework.Netcode;

        namespace TestPackets;

        public partial class CPacketEmpty : ClientPacket
        {
        }
        """;

        GeneratorTestOptions options = new GeneratorTestBuilder<PacketGenerator>(testCode)
            .WithGeneratedFile("CPacketEmpty.g.cs")
            .Build();

        GeneratorTestRunResult? result = GeneratorTestRunner<PacketGenerator>.Run(options);
        Assert.That(result, Is.Null, "Packets without properties should not generate methods.");
    }

    [Test]
    public void ManualWriteReadMethods_DoesNotGenerateSource()
    {
        string testCode = $$"""
        using Framework.Netcode;

        namespace TestPackets;

        public partial class CPacketManual : ClientPacket
        {
            public int Id { get; set; }

            public override void Write(PacketWriter writer)
            {
                writer.Write(Id);
            }

            public override void Read(PacketReader reader)
            {
                Id = reader.ReadInt();
            }
        }
        """;

        GeneratorTestOptions options = new GeneratorTestBuilder<PacketGenerator>(testCode)
            .WithGeneratedFile("CPacketManual.g.cs")
            .Build();

        GeneratorTestRunResult? result = GeneratorTestRunner<PacketGenerator>.Run(options);
        Assert.That(result, Is.Null, "Packets with manual Write/Read should not be regenerated.");
    }

    [Test]
    public void GeneratedSource_CompilesIntoAssembly()
    {
        string className = "CSimplePacket";
        string testCode = $$"""
        using Framework.Netcode;

        namespace TestPackets;

        public partial class {{className}} : ClientPacket
        {
            public int Id { get; set; }
        }
        """;

        GeneratorTestRunResult result = RunAndRequireGeneratedSource(testCode, $"{className}.g.cs");

        GeneratedFileStore fileStore = new();
        fileStore.Write(result.GeneratedFile, result.GeneratedSource);
        GeneratedAssemblyCompiler.Compile(result, fileStore);
    }

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
