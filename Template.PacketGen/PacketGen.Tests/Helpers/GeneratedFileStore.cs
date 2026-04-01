namespace PacketGen.Tests;

/// <summary>
/// File-store implementation backed by <see cref="GeneratedFiles"/>.
/// </summary>
internal sealed class GeneratedFileStore : IGeneratedFileStore
{
    /// <summary>
    /// Writes generated source to a named file.
    /// </summary>
    /// <param name="fileName">Output file name.</param>
    /// <param name="source">Source contents.</param>
    public void Write(string fileName, string source) => GeneratedFiles.Output(fileName, source);

    /// <summary>
    /// Opens a generated file using the system shell.
    /// </summary>
    /// <param name="fileName">File name to preview.</param>
    public void Preview(string fileName) => GeneratedFiles.Preview(fileName);

    /// <summary>
    /// Writes generator diagnostics to a named file.
    /// </summary>
    /// <param name="fileName">Output file name.</param>
    /// <param name="contents">Diagnostics contents.</param>
    public void WriteErrors(string fileName, string contents) => GeneratedFiles.OutputErrors(fileName, contents);
}
