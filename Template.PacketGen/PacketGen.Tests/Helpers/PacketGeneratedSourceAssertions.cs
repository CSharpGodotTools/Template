namespace PacketGen.Tests;

/// <summary>
/// Shared assertions and signature constants for generated packet source tests.
/// </summary>
internal static class PacketGeneratedSourceAssertions
{
    /// <summary>
    /// Expected write-override signature.
    /// </summary>
    public const string WriteOverrideSignature = "public override void Write(" + PacketGenConstants.PacketWriterTypeName + " writer)";

    /// <summary>
    /// Expected read-override signature.
    /// </summary>
    public const string ReadOverrideSignature = "public override void Read(" + PacketGenConstants.PacketReaderTypeName + " reader)";

    /// <summary>
    /// Expected equals-override signature.
    /// </summary>
    public const string EqualsOverrideSignature = "public override bool Equals(object? obj)";

    /// <summary>
    /// Expected hash-code override signature.
    /// </summary>
    public const string GetHashCodeOverrideSignature = "public override int GetHashCode()";

    /// <summary>
    /// Asserts that core packet override methods exist in generated source.
    /// </summary>
    /// <param name="generatedSource">Generated source text under test.</param>
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
