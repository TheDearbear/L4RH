namespace L4RH;

public static class MemoryStreamExtensions
{
    public static MemoryStream Take(this MemoryStream stream, int length)
    {
        MemoryStream newStream = new(stream.ToArray().Skip((int)stream.Position).Take(length).ToArray());
        stream.Position += length;
        return newStream;
    }
}
