using L4RH;
using L4RH.Model.Solids;
using L4RH.Readers;

namespace UG2Mappings.SolidListReaders;

internal class SolidObjectDescriptor : IChunkReader
{
    public uint ChunkId => 0x00134900;

    public void Deserialize(BinarySpan span, SolidObject solid)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 4;
        span.AlignPosition();
        span.Pointer += 52;

        solid.VerticesNumber = span.ReadInt32();
        solid.Vertices.Capacity = solid.VerticesNumber;
    }
}
