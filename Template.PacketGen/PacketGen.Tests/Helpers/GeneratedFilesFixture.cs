namespace PacketGen.Tests;

/// <summary>
/// NUnit fixture that cleans generated artifacts before tests run.
/// </summary>
[SetUpFixture]
internal sealed class GeneratedFilesFixture
{
    /// <summary>
    /// Removes previously generated files to keep test output clean.
    /// </summary>
    [OneTimeSetUp]
    public void CleanGeneratedFiles()
    {
        string genDir = GeneratedFiles.GetGenDir();
        foreach (string genFile in Directory.GetFiles(genDir))
            File.Delete(genFile);
    }
}
