using L4RH;
using L4RH.Model.Solids;
using L4RH.Readers;

namespace UG2Mappings.SolidListReaders;

internal class SolidObjectChunk : IChunkReader
{
    public uint ChunkId => 0x80134010;

    public void Deserialize(BinarySpan span, SolidObjectList list)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        span.Pointer += 4;

        var objHeader = new SolidObjectHeader();
        var objHashes = new SolidObjectHashes();
        var objBody = new SolidObjectBody();

        SolidObject? solid = null;

        while (span.Pointer < span.Length)
        {
            uint id = span.ReadUInt32();
            int length = span.ReadInt32();

            span.Pointer -= 8;

            var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

            if (id == objHeader.ChunkId)
            {
                solid = objHeader.Deserialize(chunkBuffer, list);
            }
            else if (id == objHashes.ChunkId && solid is not null)
            {
                objHashes.Deserialize(chunkBuffer, solid);
            }
            else if (id == objBody.ChunkId && solid is not null)
            {
                objBody.Deserialize(chunkBuffer, solid);
            }
        }
    }
}
