namespace PacketGen.Tests;

internal static class PacketGeneratedSourceAssertions
{
    public const string WriteOverrideSignature = "public override void Write(" + PacketGenConstants.PacketWriterTypeName + " writer)";
    public const string ReadOverrideSignature = "public override void Read(" + PacketGenConstants.PacketReaderTypeName + " reader)";
    public const string EqualsOverrideSignature = "public override bool Equals(object? obj)";
    public const string GetHashCodeOverrideSignature = "public override int GetHashCode()";

    public static void AssertContainsCoreOverrides(string generatedSource)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(generatedSource, Does.Contain(WriteOverrideSignature));
            Assert.That(generatedSource, Does.Contain(ReadOverrideSignature));
            Assert.That(generatedSource, Does.Contain(EqualsOverrideSignature));
            Assert.That(generatedSource, Does.Contain(GetHashCodeOverrideSignature));
        }
    }
}
