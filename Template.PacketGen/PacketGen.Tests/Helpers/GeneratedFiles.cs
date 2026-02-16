using System.Diagnostics;

namespace PacketGen.Tests;

internal static class GeneratedFiles
{
    public static void Preview(string fileName)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = GetPath(fileName),
            UseShellExecute = true
        });
    }

    public static void Output(string fileName, string source)
    {
        string path = GetPath(fileName);

        File.WriteAllText(path, source);
    }

    public static string GetGenDir()
    {
        string dir = Path.Combine(TestContext.CurrentContext.TestDirectory, "_Generated");

        Directory.CreateDirectory(dir);

        return dir;
    }

    public static void OutputErrors(string fileName, string contents)
    {
        string dir = GetGenDir();

        File.WriteAllText(Path.Combine(dir, fileName), contents);
    }

    private static string GetPath(string fileName)
    {
        string genDir = GetGenDir();

        return Path.Combine(genDir, fileName);
    }
}
