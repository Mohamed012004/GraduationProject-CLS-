using TagLib;
using System.IO;
using File = System.IO.File;

public class StreamFileAbstraction : TagLib.File.IFileAbstraction
{
    public string Name { get; }
    public Stream ReadStream { get; }
    public Stream WriteStream { get; }

    public StreamFileAbstraction(string name, Stream readStream, Stream writeStream)
    {
        Name = name;
        ReadStream = readStream;
        WriteStream = writeStream;
    }

    public void CloseStream(Stream stream)
    {
        // nothing needed
    }
}