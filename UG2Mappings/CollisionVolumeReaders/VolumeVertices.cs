using L4RH;
using L4RH.Model;
using L4RH.Readers;

namespace UG2Mappings.CollisionVolumeReaders;

internal class VolumeVertices : IChunkReader
{
    public uint ChunkId => 0x00039201;

    public void Deserialize(BinarySpan span, CollisionVolume volume)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 4;

        for (uint i = 0; i < volume.Vertices.Length / 3; i++)
        {
            span.Pointer += 16;

            volume.Vertices[i * 3 + 0] = span.ReadSingle();
            volume.Vertices[i * 3 + 1] = span.ReadSingle();
            volume.Vertices[i * 3 + 2] = span.ReadSingle();

            span.Pointer += 20;
        }
    }
}
