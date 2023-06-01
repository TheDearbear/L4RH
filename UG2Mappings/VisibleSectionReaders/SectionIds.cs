using L4RH;
using L4RH.Readers;

namespace UG2Mappings.VisibleSectionReaders;

internal class SectionIds : IChunkReader
{
    public uint ChunkId => 0x00034151;

    public ushort[] Deserialize(BinarySpan span)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 8;

        int count = span.ReadInt32();
        ushort[] ids = new ushort[count];

        for (int i = 0; i < count; i++)
            ids[i] = span.ReadUInt16();

        return ids;
    }
}
