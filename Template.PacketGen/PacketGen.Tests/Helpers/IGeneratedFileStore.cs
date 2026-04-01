namespace PacketGen.Tests;

/// <summary>
/// Abstraction for storing and previewing generated test artifacts.
/// </summary>
internal interface IGeneratedFileStore
{
    /// <summary>
    /// Writes generated source to a named file.
    /// </summary>
    /// <param name="fileName">Output file name.</param>
    /// <param name="source">Source contents.</param>
    void Write(string fileName, string source);

    /// <summary>
    /// Opens a generated file using the system shell.
    /// </summary>
    /// <param name="fileName">File name to preview.</param>
    void Preview(string fileName);

    /// <summary>
    /// Writes generator diagnostics to a named file.
    /// </summary>
    /// <param name="fileName">Output file name.</param>
    /// <param name="contents">Diagnostics contents.</param>
    void WriteErrors(string fileName, string contents);
}
