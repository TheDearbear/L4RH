using L4RH;
using L4RH.Model.Solids;
using L4RH.Readers;

namespace UG2Mappings.SolidListReaders;

internal class SolidListInfo : IChunkReader
{
    public uint ChunkId => 0x00134002;

    public void Deserialize(BinarySpan span, SolidObjectList list)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 12;

        list.Marker = span.ReadInt32();

        span.Pointer += 4;

        list.PipelinePath = span.ReadString(0x38);
        list.ParentSectionName = span.ReadString(0x20);

        span.Pointer += 8;
    }
}
