using L4RH;
using L4RH.Readers;
using SceneryClass = L4RH.Model.Sceneries.Scenery;

namespace UG2Mappings.SceneryReaders;

public class Scenery : ISceneryReader
{
    public uint ChunkId => 0x80034100;

    public object DeserializeObject(BinarySpan span, long dataBasePosition) => Deserialize(span, dataBasePosition);
    public SceneryClass Deserialize(BinarySpan span, long dataBasePosition)
    {
        if (span.ReadUInt32() != ChunkId)
            throw new ArgumentException("Unsupported chunk!");

        if (span.ReadUInt32() != span.Length - 8)
            throw new ArgumentException("Chunk size mismatch!");

        var scenery = new SceneryClass() { Offset = (uint)dataBasePosition };

        var header = new ScenerySectionHeader();
        var info = new SceneryInfo();
        var instance = new SceneryInstance();

        while (span.Pointer < span.Length)
        {
            span.SkipPadding();

            var id = span.ReadUInt32();
            var length = span.ReadInt32();

            span.Pointer -= 8;

            var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

            if (id == header.ChunkId)
            {
                header.Deserialize(chunkBuffer, scenery);
            }
            else if (id == info.ChunkId)
            {
                info.Deserialize(chunkBuffer, scenery);
            }
            else if (id == instance.ChunkId)
            {
                instance.Deserialize(chunkBuffer, scenery);
            }
        }

        return scenery;
    }
}
