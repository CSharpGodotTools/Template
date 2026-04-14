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
        string[] genFiles = Directory.GetFiles(genDir);

        foreach (string genFile in genFiles)
            File.Delete(genFile);
    }
}
