using System.Diagnostics;

namespace PacketGen.Tests;

/// <summary>
/// File-output helpers for generated source and test diagnostics artifacts.
/// </summary>
internal static class GeneratedFiles
{
    /// <summary>
    /// Opens a generated file using the system shell.
    /// </summary>
    /// <param name="fileName">File name inside the generated-output directory.</param>
    public static void Preview(string fileName)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = GetPath(fileName),
            UseShellExecute = true
        });
    }

    /// <summary>
    /// Writes generated source to disk.
    /// </summary>
    /// <param name="fileName">Output file name.</param>
    /// <param name="source">Source contents.</param>
    public static void Output(string fileName, string source)
    {
        string path = GetPath(fileName);

        File.WriteAllText(path, source);
    }

    /// <summary>
    /// Returns the generated-artifacts directory for tests, creating it when needed.
    /// </summary>
    /// <returns>Generated-artifacts directory path.</returns>
    public static string GetGenDir()
    {
        string dir = Path.Combine(TestContext.CurrentContext.TestDirectory, "_Generated");

        Directory.CreateDirectory(dir);

        return dir;
    }

    /// <summary>
    /// Writes diagnostics/error content to the generated-artifacts directory.
    /// </summary>
    /// <param name="fileName">Output file name.</param>
    /// <param name="contents">Diagnostics text.</param>
    public static void OutputErrors(string fileName, string contents)
    {
        string dir = GetGenDir();

        File.WriteAllText(Path.Combine(dir, fileName), contents);
    }

    /// <summary>
    /// Resolves an output file path inside the generated-artifacts directory.
    /// </summary>
    /// <param name="fileName">File name to resolve.</param>
    /// <returns>Absolute output file path.</returns>
    private static string GetPath(string fileName)
    {
        string genDir = GetGenDir();

        return Path.Combine(genDir, fileName);
    }
}
