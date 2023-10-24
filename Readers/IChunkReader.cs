namespace L4RH.Readers;

public interface IChunkReader
{
    public uint ChunkId { get; }

    public object DeserializeObject(BinarySpan span, long baseDataPosition)
    {
        throw new NotImplementedException("This class is not currently support deserialization!");
    }
}
