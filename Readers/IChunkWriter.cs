using System;

namespace L4RH.Readers;

public interface IChunkWriter
{
    public uint ChunkId { get; }

    public byte[] SerializeObject(object obj)
    {
        throw new NotImplementedException("Serialization is not currently supported by L4RH!");
    }
}
