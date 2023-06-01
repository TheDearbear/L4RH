using L4RH;
using L4RH.Model.Solids;
using L4RH.Readers;

namespace UG2Mappings.SolidListReaders;

public class SolidList : ISolidListReader
{
    public uint ChunkId => 0x80134000;

    public object DeserializeObject(BinarySpan span, long dataBasePosition) => Deserialize(span, dataBasePosition);
    public SolidObjectList Deserialize(BinarySpan span, long dataBasePosition)
    {
        var list = new SolidObjectList();

        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        if (span.ReadUInt32() != span.Length - 8)
            throw new ArgumentException("Chunk size mismatch!");

        var header = new SolidListHeader();
        var obj = new SolidObjectChunk();

        while (span.Pointer < span.Length)
        {
            span.SkipPadding();

            uint id = span.ReadUInt32();
            int length = span.ReadInt32();

            span.Pointer -= 8;

            var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

            if (id == header.ChunkId)
            {
                header.Deserialize(chunkBuffer, list);
            }
            else if (id == obj.ChunkId)
            {
                obj.Deserialize(chunkBuffer, list);
            }
        }

        return list;
    }
}
