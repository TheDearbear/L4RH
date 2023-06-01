using L4RH;
using L4RH.Model;
using L4RH.Readers;

namespace UG2Mappings.CollisionVolumeReaders;

internal class VolumeIndices : IChunkReader
{
    public uint ChunkId => 0x00039202;

    public void Deserialize(BinarySpan span, CollisionVolume volume)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        for (int i = 0; i < volume.Indices.Length; i++)
            volume.Indices[i] = span.ReadUInt16();
    }
}
