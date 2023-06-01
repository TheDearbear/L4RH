using L4RH;
using L4RH.Readers;
using SceneryClass = L4RH.Model.Sceneries.Scenery;

namespace UG2Mappings.SceneryReaders;

internal class ScenerySectionHeader : IChunkReader
{
    public uint ChunkId => 0x00034101;

    public void Deserialize(BinarySpan span, SceneryClass scenery)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 16;

        scenery.VisibleSectionId = span.ReadInt32();

        span.Pointer += 44;
    }
}
