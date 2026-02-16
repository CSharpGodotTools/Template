namespace PacketGen.Tests;

internal interface IGeneratedFileStore
{
    void Write(string fileName, string source);
    void Preview(string fileName);
    void WriteErrors(string fileName, string contents);
}
