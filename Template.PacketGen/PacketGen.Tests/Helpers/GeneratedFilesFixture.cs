using NUnit.Framework;

namespace PacketGen.Tests;

[SetUpFixture]
internal sealed class GeneratedFilesFixture
{
    [OneTimeSetUp]
    public void CleanGeneratedFiles()
    {
        string genDir = GeneratedFiles.GetGenDir();
        string[] genFiles = Directory.GetFiles(genDir);

        foreach (string genFile in genFiles)
        {
            File.Delete(genFile);
        }
    }
}
