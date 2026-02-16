namespace PacketGen.Tests;

internal sealed class GeneratedFileStore : IGeneratedFileStore
{
    public void Write(string fileName, string source) => GeneratedFiles.Output(fileName, source);

    public void Preview(string fileName) => GeneratedFiles.Preview(fileName);

    public void WriteErrors(string fileName, string contents) => GeneratedFiles.OutputErrors(fileName, contents);
}
