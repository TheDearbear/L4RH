using L4RH;
using L4RH.Model.Solids;
using L4RH.Readers;

namespace UG2Mappings.SolidListReaders;

internal class SolidListHeader : IChunkReader
{
    public uint ChunkId => 0x80134001;

    public void Deserialize(BinarySpan span, SolidObjectList list)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 4;
        
        var info = new SolidListInfo();

        while (span.Pointer < span.Length)
        {
            uint id = span.ReadUInt32();
            int length = span.ReadInt32();

            span.Pointer -= 8;

            var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

            if (id == info.ChunkId)
            {
                info.Deserialize(chunkBuffer, list);
            }
        }
    }
}
